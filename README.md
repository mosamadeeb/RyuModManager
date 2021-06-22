# RyuModManager
Mod manager for Yakuza series PC games. Currently is a CLI app, no GUI has been made yet. Please check the [Supported Games](../../wiki/Supported-Games) list before using.

Allows loading mods from a `/mods/` folder inside the game's directory.
Mods do not have to contain repacked PAR archives, as Ryu Mod Manager can load loose files from the mod folder.
Repacking is needed only for some PAR archives in Old Engine games (Yakuza games released before Yakuza 6). Other games do not need any PAR repacking.

# Installing
Unpack the [latest release](../../releases/latest) into the game's directory, in the same folder as the game's executable.

# Usage
For actual usage, check the [Installing Mods](../../wiki/Installing-Mods) and [Creating A New Mod](../../wiki/Creating-A-New-Mod) articles in the [wiki](../../wiki).

To run the program, just launch it with no arguments and it will generate an MLO file to be used by [Parless](https://github.com/SutandoTsukai181/YakuzaParless), the Yakuza mod loader.
All of the mod manager [releases](../../releases) include Parless and all necessary files for usage, so no need to download Parless separately.

# Building
Clone the repository and fetch the submodules, then open the solution file (.sln) in Visual Studio.

# Credits
Thanks to [Kaplas](https://github.com/Kaplas80) for [ParLibrary](https://github.com/Kaplas80/ParManager), which is used for repacking pars.

Thanks to [Pleonex](https://github.com/pleonex) for [Yarhl](https://github.com/SceneGate/Yarhl).

Thanks to Kent for providing the icon.

For the mod loader credits, please check the [YakuzaParless](https://github.com/SutandoTsukai181/YakuzaParless) repository.

# License
This project uses the MIT License, so feel free to include it in whatever you want.
