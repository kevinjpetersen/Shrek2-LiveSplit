using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrek2_LiveSplit.Data
{
    public class Shrek2Utils
    {
        public static Process GetGameProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName("game");
                if (processes == null || processes.Length <= 0) return null;

                return processes[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
