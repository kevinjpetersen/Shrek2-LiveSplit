using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shrek2_LiveSplit
{
    class GameMemory
    {
        public const int SLEEP_TIME = 15;

        public static List<string> splits = new List<string>(new string[]
        {
            "Tutorial",
            "Wheel_Stealers",
            "Spooky_Forest",
            "Puss_in_Boots_boss",
            "Factory",
            "Bandits_Forest",
            "Knight_boss_with_Horse",
            "Knight_boss_with_Puss_in_Boots",
            "Knight_boss_with_Shrek",
            "Castle",
            "End"
        });

        public event EventHandler OnMainMenuLoad;
        public event EventHandler OnNewGame;
        public delegate void SplitCompletedEventHandler(object sender, string type);
        public event SplitCompletedEventHandler OnSplitCompleted;
        public event EventHandler OnLoadStart;
        public event EventHandler OnLoadEnd;

        private Task _thread;
        private CancellationTokenSource _cancelSource;
        private SynchronizationContext _uiThread;
        private List<int> _ignorePIDs;

        private DeepPointer _logBufferPtr;
        private DeepPointer _logBufferCursorPtr;
        private DeepPointer _isLoadingPtr;
        private DeepPointer _isSavingPtr;

        public uint frameCounter = 0;
        private enum ExpectedExeSizes
        {
            v433 = 438272,
        }

        public bool[] SplitStates { get; set; }

        public void resetSplitStates()
        {
            for (int i = 0; i < splits.Count; i++)
            {
                SplitStates[i] = false;
            }
        }

        public GameMemory()
        {
            SplitStates = new bool[splits.Count];

            _logBufferPtr = new DeepPointer(0x000566B4, 0x50);
            _logBufferCursorPtr = new DeepPointer(0x000566B4, 0x4c);
            _isLoadingPtr = new DeepPointer("Engine.dll", 0x000012E0, 0x5a4, 0x68);
            _isSavingPtr = new DeepPointer("Core.dll", 0x001CA8B8, 0xbc, 0x50c, 0x400);

            resetSplitStates();

            _ignorePIDs = new List<int>();
        }

        public void StartMonitoring()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
            {
                throw new InvalidOperationException();
            }
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
            {
                throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");
            }

            _uiThread = SynchronizationContext.Current;
            _cancelSource = new CancellationTokenSource();
            _thread = Task.Factory.StartNew(MemoryReadThread);
        }

        public void Stop()
        {
            if (_cancelSource == null || _thread == null || _thread.Status != TaskStatus.Running)
            {
                return;
            }

            _cancelSource.Cancel();
            _thread.Wait();
        }
        void MemoryReadThread()
        {
            Trace.WriteLine("[NoLoads] MemoryReadThread");

            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    Trace.WriteLine("[NoLoads] Waiting for game.exe...");

                    Process game;
                    while ((game = GetGameProcess()) == null)
                    {
                        Thread.Sleep(250);
                        if (_cancelSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    Trace.WriteLine("[NoLoads] Got game.exe!");

                    frameCounter = 0;
                    int prevBufCursor = 0;
                    string prevBuf = String.Empty;
                    string currentMap = String.Empty;
                    string prevCurrentMap = String.Empty;
                    bool prevIsLoading = false;

                    while (!game.HasExited)
                    {
                        string buf;
                        _logBufferPtr.DerefString(game, 4096, out buf);

                        int bufCursor;
                        _logBufferCursorPtr.Deref(game, out bufCursor);

                        int ret;
                        _isLoadingPtr.Deref(game, out ret);

                        int isSaving;
                        _isSavingPtr.Deref(game, out isSaving);

                        bool isLoading = (ret == 2 || isSaving == 256);

                        string log = String.Empty;

                        if ((!buf.Equals(prevBuf) && !prevBuf.Equals(String.Empty)))
                        {
                            int length = (prevBufCursor > bufCursor) ? 4096 - prevBufCursor : bufCursor - prevBufCursor;
                            log = buf.Substring(prevBufCursor, length);
                            if (prevBufCursor > bufCursor)
                                log += buf.Substring(0, bufCursor);

                            string[] logLines = Regex.Split(log, @"(?<=\r\n)");

                            Debug.WriteLine(String.Format("------bufCursor: {0} prevBufCursor: {1}-------", bufCursor, prevBufCursor));
                            Debug.WriteLine("--------------------" + frameCounter + "---------------------");
                            Debug.Write(log);

                            int cursor = prevBufCursor;
                            uint i = 0;
                            foreach (string line in logLines)
                            {
                                Match validLine = Regex.Match(line, @"^(?:Log:|Init:|ScriptLog:|CutLog:|DevSound:|Localization:|Warning:|ScriptWarning:|Exit:|Uninitialized:).+\r\n");
                                Match loadMapRegex = Regex.Match(line, @"LoadMap: ([^?]+)\?");
                                Match splitRegex = Regex.Match(line, @"Bringing Level ([^ ?]+)\.MyLevel up for play");
                                Match loadTimeRegex = Regex.Match(line, @"Load time (?:.+\\)?([^?]+): (\d+\.\d+) seconds total, (\d+\.\d+) app");
                                Match saveStartRegex = Regex.Match(line, @"Saving game\.\.\. filename: .+\.usa");
                                Match saveEndRegex = Regex.Match(line, @"Log: Moving '.+\.tmp' to '.+\.usa'");

                                if (line.Equals(""))
                                    continue;

                                // If the line is incorrect, read again from there next frame
                                if (!validLine.Success && i > 0)
                                {
                                    Debug.WriteLine("\n[Invalid line] " + line);
                                    bufCursor = cursor;
                                    if (bufCursor >= 4096)
                                        bufCursor -= 4096;
                                    break;
                                }
                                cursor += line.Length;

                                if (loadMapRegex.Success)
                                {
                                    currentMap = loadMapRegex.Groups[1].Value.ToLower();
                                    if (splits.Contains(currentMap))
                                        Split(currentMap, frameCounter);
                                }
                                else if (line.Contains("Log: Server switch level: Book_Story_1.unr?GameState=GSTATE000"))
                                {
                                    _uiThread.Post(d =>
                                    {
                                        if (this.OnNewGame != null)
                                        {
                                            this.OnNewGame(this, EventArgs.Empty);
                                        }
                                    }, null);
                                }
                                else if (line.Contains("CutLog: [SWAMPTRANSTOCAR.CutScene]"))
                                {
                                    Split("Tutorial", frameCounter);
                                }
                                else if (line.Contains("CutLog: [CARRIAGE_AFTERLOGS.CutScene]"))
                                {
                                    Split("Wheel_Stealers", frameCounter);
                                }
                                else if (line.Contains("CutLog: [PIBAMBUSHINTRO.CutScene]"))
                                {
                                    Split("Spooky_Forest", frameCounter);
                                }
                                else if (line.Contains("CutLog: [DEFEATEDPIB.CutScene]"))
                                {
                                    Split("Puss_in_Boots_boss", frameCounter);
                                }
                                else if (line.Contains("CutLog: [HAMLETINTRO.CutScene]"))
                                {
                                    Split("Factory", frameCounter);
                                }
                                else if (line.Contains("CutLog: [HAMLETENDFARMHOUSE.CutScene]"))
                                {
                                    Split("Bandits_Forest", frameCounter);
                                }
                                else if (line.Contains("CutLog: [PRISDONKEY_FATNIGHTENDLEVEL.CutScene]"))
                                {
                                    Split("Knight_boss_with_Horse", frameCounter);
                                }
                                else if (line.Contains("CutLog: [PRISPIB_FATKNIGHTENDLEVEL.CutScene]"))
                                {
                                    Split("Knight_boss_with_Puss_in_Boots", frameCounter);
                                }
                                else if (line.Contains("CutLog: [PRISSHREK_MONGOINTRO.CutScene]"))
                                {
                                    Split("Knight_boss_with_Shrek", frameCounter);
                                }
                                else if (line.Contains("CutLog: [FINALBATTLE_FGMPREBATTLE.CutScene]"))
                                {
                                    Split("Castle", frameCounter);
                                }
                                else if (line.Contains("CutLog: [FINALBATTLE_YOUWIN.CutScene]"))
                                {
                                    Split("End", frameCounter);
                                }

                                i++;
                            }
                        }

                        if (currentMap != prevCurrentMap)
                        {
                            Debug.WriteLine(String.Format("[NoLoads] Detected map change from \"{0}\" to \"{1}\" - {2}", prevCurrentMap, currentMap, frameCounter));

                            if (currentMap == "book_frontend.unr")
                            {
                                _uiThread.Post(d =>
                                {
                                    if (this.OnMainMenuLoad != null)
                                    {
                                        this.OnMainMenuLoad(this, EventArgs.Empty);
                                    }
                                }, null);
                            }

                        }

                        if (isLoading != prevIsLoading)
                        {
                            if (isLoading)
                            {
                                Trace.WriteLine("[NoLoads] Loading started - " + frameCounter);
                                _uiThread.Post(d =>
                                {
                                    if (this.OnLoadStart != null)
                                    {
                                        this.OnLoadStart(this, EventArgs.Empty);
                                    }
                                }, null);
                            }
                            else
                            {
                                Trace.WriteLine("[NoLoads] Loading ended - " + frameCounter);
                                _uiThread.Post(d =>
                                {
                                    if (this.OnLoadEnd != null)
                                    {
                                        this.OnLoadEnd(this, EventArgs.Empty);
                                    }
                                }, null);
                            }
                        }

                        frameCounter++;
                        prevBuf = buf;
                        prevBufCursor = bufCursor;
                        prevCurrentMap = currentMap;
                        prevIsLoading = isLoading;

                        Thread.Sleep(SLEEP_TIME);

                        if (_cancelSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    Thread.Sleep(1000);
                }
            }
        }

        private void Split(string split, uint frame)
        {
            _uiThread.Post(d =>
            {
                if (this.OnSplitCompleted != null)
                {
                    this.OnSplitCompleted(this, split);
                }
            }, null);
        }

        Process GetGameProcess()
        {
            Process game = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower() == "game"
                && !p.HasExited && !_ignorePIDs.Contains(p.Id));
            if (game == null)
            {
                return null;
            }

            if (game.MainModule.ModuleMemorySize != (int)ExpectedExeSizes.v433)
            {
                _ignorePIDs.Add(game.Id);
                _uiThread.Send(d => MessageBox.Show("Unexpected game version." + game.MainModule.ModuleMemorySize, "LiveSplit.Shrek2",
                    MessageBoxButtons.OK, MessageBoxIcon.Error), null);
                return null;
            }

            return game;
        }
    }
}
