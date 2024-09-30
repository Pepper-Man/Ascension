# Ascension
Ascension is a C# [ManagedBlam](https://c20.reclaimers.net/general/mod-tools/managed-blam/)-based program for converting H2 scenario and shader data into H3 format. 
This is a UI-based compilation of all my previously released H2-to-H3 converters, primarily built for use by the porting team I am in, but feel free to use it for you own ports. Let the data be ascended!

# Requirements
* Requires [.NET 4.8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

# Features
## Shader Converter
* Extracts all required bitmaps
* If requested, can instead use provided bitmaps from another source, such as [the original .tiff source](http://vaporeon.io/hosted/halo/data/)
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
* That's pretty much it!

## Scenario Data Converter
* This converter translates most object placements into your H3 scenario. Works for both SP and MP scenarios.
* It currently converts:
    * H2 multiplayer starting locations to H3 respawn point scenery. Includes team data.
    * All weapon palette and placements. Weapon palette references will attempt to automatically use existing H3 weapon tags where applicable. Grenades included.
    * All scenery types and placements, including variant names. Scenery type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
    * All trigger volumes, including names.
    * All vehicle types and placements. Vehicle palette references will attempt to automatically use existing H3 vehicle tags where applicable. Some variants may not be applied. Turret types not available in H3 will be switched for the standard mounted turret type for the given faction.
    * All crates, including variant names. Crate type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
    * All (netgame) gamemode items (CTF flag spawns, territories, bomb spawns/goals, teleporter sender/receivers etc etc) to H3 gametype crates. Unused or unapplicable gametype objects, such as race checkpoints and headhunter bins, are included but replaced with temporary forerunner core crates for easy identification.
    * All decal placements and types. Due to system incompatibilities between engines, decals may appear rotated and/or stretched incorrectly. To fix stretching, simply touch the rotation handle. Rotate with the handle to fix rotations where necessary.

## AI/Scripting Data Converter
* Translates everything required for functional squads, as well as data used for mission scripting.
* Currently includes:
    * All jump hints, including correct height
    * All flight hints
    * All cutscene flags
    * All point sets
    * Conversion of palette data for weapons, vehicles, and characters
    * Squad folders if the user provides a .txt of folder names to use
    * All squad groups, with correct hierarchy
    * All squads. For each squad, the following data is set:
    <details>
        <summary>&nbsp;&nbsp;Click to show/hide further squad details</summary>
        
        - Name
        - Flags
        - Team
        - Squad group
        - Spawn count
        - Upgrade chance
        - Vehicle type
        - Character type
        - Primary weapon type
        - Secondary weapon type
        - Grenade type
        - Initial zone
        - Vehicle variant
        - Placement script reference
        - Squad folder
        - All starting locations. For each starting location, the following data is set:
        
            - Name
            - Position
            - Rotation
            - Flags
            - Character type
            - Primary weapon type
            - Secondary weapon type
            - Vehicle type
            - Vehicle seat type
            - Grenade type
            - Swarm count
            - Actor variant
            - Vehicle variant
            - Initial movement distance
            - Initial movement mode
            - Placement script reference

    </details>


# Usage
* Download the latest release, or compile your own version.
* Extract any required H2 BSP or Scenario tags to XML with `tool export-tag-to-xml`. E.g. `tool export-tag-to-xml "C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_1.scenario_structure_bsp" "C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_1.xml"`
* I cannot distribute the required ManagedBlam.dll, so you will need to either:
    * Copy your Halo 3 ManagedBlam.dll (found in "H3EK\bin") into the same folder as this exe
    * Alternatively, simply place the files of this program directly into your "H3EK\bin" directory.
* Make sure the BSPs in your H3 scenario are in the same order as in H2
* Run this .exe, tick the converters you wish to run, and fill in any active filepath boxes.
* The program will exit when clicking "close" in the output window. This is to avoid an issue with managedblam being run more than once.

# Notes
* The program should now automatically handle XML files straight from H2 - no manual cleanup/"xFF" removal required!
* Some shaders will fail to translate properly. This is due to the sheer complexity of perfecting the shader converter. It will struggle with:
    * Glass shaders
    * Alpha test (sometimes)
* H2 bump map height values can often be too high to look good in H3. This is most obviously seen on terrain-style shaders. You can lower this in the bump map .bitmap tag.
* The program features multiple dictionaries to help with figuring out which H2 tags map to which H3 tags. This list may be incomplete, and is also designed for the filepaths used by our team, thus YMMV. The output window will report H2 tag paths that it was unable to find the dictionary entry for. You can update palette references in Guerilla to point to whatever tag you want after the converter has run, to easily update/fix the reference for all instances of that object.
* You should be able to have the scenario open in Sapien whilst the converter runs. However for unknown reasons this very rarely may prevent the program from being able to save the tag correctly.
* Be prepared for the shader converter to use up to 8GB of memory on bigger scenarios. Yeah this ain't good but neither is parsing XML.
* There will very likely be bugs and issues that I haven't caught due to the complexity, as well as time needed to test extensively on all scenarios. Please let me know on Discord @ `pepperman`

# Credits
* All code by PepperMan
* Special thanks to [Crisp](https://github.com/ILoveAGoodCrisp) and [Krevil](https://github.com/Krevil) for their assistance with ManagedBlam
* Special thanks to [Berthalamew](https://github.com/berthalamew) and [Lord Zedd](https://github.com/Lord-Zedd) for their help with obtaining bungie cook torrance values for H2 objects via the H3 Pimps at Sea build
* [Magick.NET](https://github.com/dlemstra/Magick.NET) for their free image manipulation library
