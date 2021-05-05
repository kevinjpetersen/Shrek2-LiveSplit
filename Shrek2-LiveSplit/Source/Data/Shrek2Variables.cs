using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrek2_LiveSplit.Data
{
    public class Shrek2Variables
    {
        public const string ComponentName = "Shrek 2 Auto Splitter";
        public const string UpdateUrl = "https://raw.githubusercontent.com/kevinjpetersen/Shrek2-LiveSplit/master/";

        public const int SleepTime = 8;

        public static List<string> ExcludedSplitMaps = new List<string>()
        {
            "book_frontend",
            "book_story_1",
            "book_story_2",
            "book_story_3",
            "book_story_4",
            "book_storybook",
            "credits",
            "entry",
            "beanstalk_bonus",
            "beanstalk_bonus_dawn",
            "beanstalk_bonus_knight",
            "4_fgm_pib",
            "1_shreks_swamp",
            "3_the_hunt_part2",
            "3_the_hunt_part3",
            "3_the_hunt_part4",
            "5_fgm_donkey",
            "6_hamlet_end",
            "6_hamlet_mine"
        };
    }
}
