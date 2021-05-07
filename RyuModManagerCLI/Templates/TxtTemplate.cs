namespace RyuCLI.Templates
{
    public static class TxtTemplate
    {
        public const string TxtContent =
            "; This is the Mod Load Order file.\n" +
            "; Add mod names to this file before running RyuModManagerCLI.\n" +
            "; A mod's name is the name of its folder inside the /mods/ directory\n" +
            "\n" +
            "; Comments start with ';'.\n" +
            "; You can comment out a mod to prevent it from being added to the load order.\n" +
            "\n" +
            "; Load order: LAST mod in the list has the HIGHEST priority, and its files\n" +
            "; will override those of other mods if there are any conflicts.\n" +
            "\n" +
            "ExampleMod\n" +
            "AnotherMod\n" +
            ";CommentedOutMod\n" +
            "\n\n";
    }
}
