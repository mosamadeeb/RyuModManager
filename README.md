# RyuModManager
Mod manager for Yakuza series PC games. Please check the [Supported Games](../../wiki/Supported-Games) list before using.

Allows loading mods from a `/mods/` folder inside the game's directory.
Mods do not have to contain repacked PAR archives, as Ryu Mod Manager can load loose files from the mod folder.
Repacking is needed only for some PAR archives in Old Engine games (Yakuza games released before Yakuza 6). Other games do not need any PAR repacking.

# Installing
Unpack the [latest release](../../releases/latest) into the game's directory, in the same folder as the game's executable.

# Usage
A command line interface is available, as well as a simple GUI. For better operation, using an external mod manager (such as [Vortex](https://www.nexusmods.com/about/vortex/) or [Mod Organizer](https://github.com/ModOrganizer2/modorganizer) is recommended. Ryu Mod Manager needs to be installed as well. Please check the [Yakuza Support Plugins](https://github.com/SutandoTsukai181/vortex_mo2_yakuza_plugins) repository for more info.

For actual usage, check the [Installing Mods](../../wiki/Installing-Mods) and [Creating A New Mod](../../wiki/Creating-A-New-Mod) articles in the [wiki](../../wiki).

To run the program, just launch it with no arguments and it will generate an MLO file to be used by [Parless](https://github.com/SutandoTsukai181/YakuzaParless), the Yakuza mod loader.
All of the mod manager [releases](../../releases) include Parless and all necessary files for usage, so no need to download Parless separately.

# Building
Clone the repository and fetch the submodules, then open the solution file (.sln) in Visual Studio. You can then `dotnet publish` the `RyuGUI` project.

# Credits
Thanks to [Kaplas](https://github.com/Kaplas80) for [ParLibrary](https://github.com/Kaplas80/ParManager), which is used for repacking pars.

Thanks to [Pleonex](https://github.com/pleonex) for [Yarhl](https://github.com/SceneGate/Yarhl).

Thanks to Kent for providing the icon.

For the mod loader credits, please check the [YakuzaParless](https://github.com/SutandoTsukai181/YakuzaParless) repository.

# License
This project uses the MIT License, so feel free to include it in whatever you want.
