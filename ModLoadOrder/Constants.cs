namespace ModLoadOrder
{
    using System.Collections.Generic;

    public static class Constants
    {
        public const string INI = "YakuzaParless.ini";
        public const string TXT = "ModLoadOrder.txt";
        public const string MLO = "YakuzaParless.mlo";

        public static readonly List<string> IncompatiblePars = new List<string> {
            "chara",
            "map_",
            "effect",
            "pausepar",
            "2dpar",
            "cse_",
            "prep",
            "light_anim",
        };

        /*
        public static readonly List<string> IncompatiblePars = new List<string> {
            @"^chara\/[^]*\/mesh(|_hires)\/", // Is inside chara and has /mesh/ or /mesh_hires/
            @"^chara\/[^]*\/tex[0-9]{2}(|_hires)\/", // Is inside chara and has /texXX/ or /texXX_hires/
            @"^(map_(par|en|jp|zh|ko)\/)(?!map_cmn\.bin)", // Anything inside map_xxx   //(not needed cause .bin is not a folder)that is not map_cmn.bin(_x)
            @"^effect\/effect_always\/", // Starts with effect/effect_always/
            @"^(pausepar\/)(?!pause_(c|j|z|k|en|jp|zh|ko))", // Anything inside pausepar that is not pause_xx
            @"^2d\/cse_(en|jp|zh|ko)\/pjs_(?![^]*\.csb$)", // Anything inside 2d/cse_xx/ that starts with pjs_   //(not needed cause .csb is not a folder) and does NOT end with .csb
            @"^stage\/[^]*texcmn\/", // Anything inside stage that has texcmn/
            @"^light_anim\/light_anim\/",
        };
        */
    }
}
