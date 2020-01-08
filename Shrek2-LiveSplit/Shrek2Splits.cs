using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrek2_LiveSplit
{
    public static class Shrek2Splits
    {
        public static List<Split> Splits = new List<Split>()
        {
            new Split("Tutorial", "CutLog: [SWAMPTRANSTOCAR.CutScene]"),
            new Split("Wheel Stealers", "CutLog: [CARRIAGE_AFTERLOGS.CutScene]"),
            new Split("Spooky Forest", new List<string>() { "CutLog: [FGM_OFFICE.CutScene]", "CutLog: [PIBAMBUSHINTRO.CutScene]" }),
            new Split("Puss in Boots Boss", "CutLog: [DEFEATEDPIB.CutScene]"),
            new Split("Factory", "CutLog: [HAMLETINTRO.CutScene]"),
            new Split("Bandits Forest", "CutLog: [HAMLETENDFARMHOUSE.CutScene]"),
            new Split("Knight boss with Donkey", "8_Prison_PIB"),
            new Split("Knight boss with Puss in Boots", "CutLog: [PRISPIB_FATKNIGHTENDLEVEL.CutScene]"),
            new Split("Knight boss with Shrek", "CutLog: [PRISSHREK_MONGOINTRO.CutScene]"),
            new Split("Castle", "CutLog: [FINALBATTLE_FGMPREBATTLE.CutScene]"),
            new Split("Fairy Godmother", "CutLog: [FINALBATTLE_YOUWIN.CutScene]")
        };

        public static string NewGame_MemoryLine { get; set; } = "Log: Server switch level: Book_Story_1.unr?GameState=GSTATE000";
    }

    public class Split
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<string> CutSceneTriggers { get; set; }

        public Split(string name, List<string> cutSceneTriggers)
        {
            ID = Guid.NewGuid().ToString();
            Name = name;
            CutSceneTriggers = cutSceneTriggers;
        }

        public Split(string name, string cutSceneTrigger)
        {
            ID = Guid.NewGuid().ToString();
            Name = name;
            CutSceneTriggers = new List<string>()
            {
                cutSceneTrigger
            };
        }
    }
}
