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
    class Shrek2GameLogic
    {
        public event EventHandler OnMainMenuLoad;
        public event EventHandler OnNewGame;
        public delegate void SplitCompletedEventHandler(object sender, string type);
        public event SplitCompletedEventHandler OnSplitCompleted;
        public event EventHandler OnLoadStart;
        public event EventHandler OnLoadEnd;

        private Task ListenGameThread;
        private CancellationTokenSource ListenGameCancelSource;
        private SynchronizationContext ListenGameUIThread;
        private List<int> IgnorePIDs;

        private DeepPointer LogBufferPtr;
        private DeepPointer LogBufferCursorPtr;
        private DeepPointer IsLoadingPtr;
        private DeepPointer IsSavingPtr;

        public uint FrameCounter = 0;
        private enum ExpectedExeSizes
        {
            v433 = 438272,
        }

        public bool[] SplitStates { get; set; }

        public Shrek2GameLogic()
        {
            SplitStates = new bool[Shrek2Splits.Splits.Count];

            LogBufferPtr = new DeepPointer(0x000566B4, 0x50);
            LogBufferCursorPtr = new DeepPointer(0x000566B4, 0x4c);
            IsLoadingPtr = new DeepPointer("Engine.dll", 0x000012E0, 0x5a4, 0x68);
            IsSavingPtr = new DeepPointer("Core.dll", 0x001CA8B8, 0xbc, 0x50c, 0x400);

            ResetSplitStates();

            IgnorePIDs = new List<int>();
        }

        public void ResetSplitStates()
        {
            for (int i = 0; i < Shrek2Splits.Splits.Count; i++) SplitStates[i] = false;
        }

        public void StartMonitoring()
        {
            if (ListenGameThread != null && ListenGameThread.Status == TaskStatus.Running)
            {
                throw new InvalidOperationException();
            }
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
            {
                throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");
            }

            ListenGameUIThread = SynchronizationContext.Current;
            ListenGameCancelSource = new CancellationTokenSource();
            ListenGameThread = Task.Factory.StartNew(GameMemoryReadThread);
        }

        public void Stop()
        {
            if (ListenGameCancelSource == null || ListenGameThread == null || ListenGameThread.Status != TaskStatus.Running)
            {
                return;
            }

            ListenGameCancelSource.Cancel();
            ListenGameThread.Wait();
        }
        void GameMemoryReadThread()
        {
            Trace.WriteLine("[NoLoads] MemoryReadThread");

            while (!ListenGameCancelSource.IsCancellationRequested)
            {
                try
                {
                    Trace.WriteLine("[NoLoads] Waiting for game.exe...");

                    Process game;
                    while ((game = GetGameProcess()) == null)
                    {
                        Thread.Sleep(250);
                        if (ListenGameCancelSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    Trace.WriteLine("[NoLoads] Got game.exe!");

                    FrameCounter = 0;
                    int prevBufCursor = 0;
                    string prevBuf = String.Empty;
                    string currentMap = String.Empty;
                    string prevCurrentMap = String.Empty;
                    bool prevIsLoading = false;

                    while (!game.HasExited)
                    {
                        LogBufferPtr.DerefString(game, 4096, out string buf);
                        LogBufferCursorPtr.Deref(game, out int bufCursor);
                        IsLoadingPtr.Deref(game, out int ret);
                        IsSavingPtr.Deref(game, out int isSaving);

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
                            Debug.WriteLine("--------------------" + FrameCounter + "---------------------");
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
                                    if (Shrek2Splits.Splits.Any(p => p.Name == currentMap))
                                        Split(Shrek2Splits.Splits.First(p => p.Name == currentMap).ID, FrameCounter);
                                }
                                else if (line.Contains(Shrek2Splits.NewGame_MemoryLine))
                                {
                                    ListenGameUIThread.Post(d =>
                                    {
                                        OnNewGame?.Invoke(this, EventArgs.Empty);
                                    }, null);
                                }
                                else
                                {
                                    if(Shrek2Splits.Splits.Any(p => p.CutSceneTriggers.Any(x => line.Contains(x))))
                                    {
                                        var splitValue = Shrek2Splits.Splits.FirstOrDefault(p => p.CutSceneTriggers.Any(x => line.Contains(x)));
                                        Split(splitValue.ID, FrameCounter);
                                    }
                                }

                                i++;
                            }
                        }

                        if (currentMap != prevCurrentMap)
                        {
                            Debug.WriteLine(String.Format("[NoLoads] Detected map change from \"{0}\" to \"{1}\" - {2}", prevCurrentMap, currentMap, FrameCounter));

                            if (currentMap == "book_frontend.unr")
                            {
                                ListenGameUIThread.Post(d =>
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
                                Trace.WriteLine("[NoLoads] Loading started - " + FrameCounter);
                                ListenGameUIThread.Post(d =>
                                {
                                    if (this.OnLoadStart != null)
                                    {
                                        this.OnLoadStart(this, EventArgs.Empty);
                                    }
                                }, null);
                            }
                            else
                            {
                                Trace.WriteLine("[NoLoads] Loading ended - " + FrameCounter);
                                ListenGameUIThread.Post(d =>
                                {
                                    if (this.OnLoadEnd != null)
                                    {
                                        this.OnLoadEnd(this, EventArgs.Empty);
                                    }
                                }, null);
                            }
                        }

                        FrameCounter++;
                        prevBuf = buf;
                        prevBufCursor = bufCursor;
                        prevCurrentMap = currentMap;
                        prevIsLoading = isLoading;

                        Thread.Sleep(Shrek2Variables.GameLogic_SleepTime);

                        if (ListenGameCancelSource.IsCancellationRequested)
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
            ListenGameUIThread.Post(d =>
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
                && !p.HasExited && !IgnorePIDs.Contains(p.Id));
            if (game == null)
            {
                return null;
            }

            if (game.MainModule.ModuleMemorySize != (int)ExpectedExeSizes.v433)
            {
                IgnorePIDs.Add(game.Id);
                ListenGameUIThread.Send(d => MessageBox.Show("Unexpected game version." + game.MainModule.ModuleMemorySize, "Shrek2-LiveSplit",
                    MessageBoxButtons.OK, MessageBoxIcon.Error), null);
                return null;
            }

            return game;
        }
    }
}
