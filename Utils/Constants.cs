﻿using System.Collections.Generic;

namespace Utils
{
    public static class Constants
    {
        public const string INI = "YakuzaParless.ini";
        public const string TXT = "ModList.txt";
        public const string TXT_OLD = "ModLoadOrder.txt";
        public const string MLO = "YakuzaParless.mlo";
        public const string ASI = "YakuzaParless.asi";
        public const string DINPUT8DLL = "dinput8.dll";
        public const string VERSIONDLL = "version.dll";
        public const string WINMMDLL = "winmm.dll";
        public const string WINMMLJ = "winmm.lj";
        public const string PARLESS_NAME = ".parless paths";
        public const string EXTERNAL_MODS = "_externalMods";
        public const string VORTEX_MANAGED_FILE = "__folder_managed_by_vortex";

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
            "stage",
        };
    }
}
