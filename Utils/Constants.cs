using System.Collections.Generic;

namespace Utils
{
    public static class Constants
    {
        public const string INI = "YakuzaParless.ini";
        public const string TXT = "ModLoadOrder.txt";
        public const string MLO = "YakuzaParless.mlo";
        public const string PARLESS_NAME = ".parless paths";

        public static readonly List<string> IncompatiblePars = new List<string> {
            "chara",
            "map_",
            "effect",
            "pausepar",
            "2d",
            "cse",
            "prep",
            "light_anim",
            "particle",
        };
    }
}
