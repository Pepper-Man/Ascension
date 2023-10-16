# Pepper's H2 to H3 Converter UI
A C# program for converting H2 scenario and shader data into H3 format. 
This is a UI-based compilation of all my previously released H2-to-H3 converters.
This is primarily built for use by the porting team I am in, but feel free to use it for you own ports.

# Requirements
* Requires [.NET 4.8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

# Features
## Shader Converter
* Extracts all required bitmaps
* If requested, can instead use provided bitmaps from another source.
* Imports all bitmaps into H3 with correct settings (usage, compression, bump height etc)
* Creates all shaders referenced in the given BSP(s)
* Can handle special plasma shaders
* Can handle basic shader foliage for trees and bushes
* Within each shader, this program automatically sets (if required):
    * Base map
    * Standard bump mapping
    * Detail map(s) (handles single, two-detail/detail_blend and three-detail)
    * Self illum map
    * Anisotropic filtering set to 4x on all bitmaps
    * Specular mask from diffuse
    * Uniform bitmap scaling, or separate x and y scaling (if required)
    * Cook torrance, with different settings applied based on the H2 shader's specular type
    * Specular, fresnel and env tint colours
    * Alpha test/blend
    * Dynamic environment mapping

## Zones/Firing Positions Converter
* Translates all zone, area and firing position data into your H3 scenario
* That's pretty much it

## Scenario Data Converter
* This converter translates most object placements into your H3 scenario.
* It currently converts:
    * H2 multiplayer starting locations to H3 respawn point scenery. Includes team data.
    * Weapon palette and placements. Weapon palette references will attempt to automatically use existing H3 weapon tags where applicable. Grenades included.
    * All scenery types and placements, including variant names. Scenery type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
    * All trigger volumes, including names.
    * All vehicle types and placements. Vehicle palette references will attempt to automatically use existing H3 vehicle tags where applicable. Some variants may not be applied. Turret types not available in H3 will be switched for the standard mounted turret type for the given faction.
    * All crates, including variant names. Crate type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
    * All (netgame) gamemode items (CTF flag spawns, territories, bomb spawns/goals, teleporter sender/receivers etc etc) to H3 gametype crates. Unused or unapplicable gametype objects, such as race checkpoints and headhunter bins, are included but replaced with temporary forerunner core crates for easy identification.
    * All decal placements and types. Due to system incompatibilities between engines, decals may appear rotated and/or stretched incorrectly. To fix stretching, simply touch the rotation handle. Rotate with the handle to fix rotations where necessary.

# Usage
* Download the latest release, or compile your own version.
* Extract any required H2 BSP or Scenario tags to XML with `tool export-tag-to-xml`. E.g. `tool export-tag-to-xml "C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_1.scenario_structure_bsp" "C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_1.xml"`
* I cannot distribute the required ManagedBlam.dll, so you will need to either:
    * Copy your Halo 3 ManagedBlam.dll (found in "H3EK\bin") into the same folder as this exe
    * Alternatively, simply place the files of this program directly into your "H3EK\bin" directory.
* Run this .exe, provide the file paths when prompted.

# Notes
* The program may crash when parsing the XML of a BSP. If this happens, its because the XML file contains erroneous data that needs to be removed. You will need to open the XML file in a text editor, and located and remove any instances of `xFF`. These may not display under normal searches - I am looking for a way to fix this issue.
* There will very likely be bugs and issues that I haven't caught. Please let me know on Discord - `pepperman`

# Credits
* All code by me
* Special thanks to [Crisp](https://github.com/ILoveAGoodCrisp) and [Krevil](https://github.com/Krevil) for their assistance with ManagedBlam
* Special thanks to [Berthalamew](https://github.com/berthalamew) and [Lord Zedd](https://github.com/Lord-Zedd) for their help with obtaining bungie cook torrance values for H2 objects via the H3 Pimps at Sea build
* [Magick.NET](https://github.com/dlemstra/Magick.NET) for their free image manipulation library
