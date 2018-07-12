# TTSLuaExtractor

LUA extractor for Tabletop Simulator

Atom's Tabletop Simulator plugin doesn't currently extract LUA scripts for objects nested inside bags or decks.  Most complex Tabletop Simulator boards have scripts on objects that are on bags.  Updating these scripts is very complex with the existing workflow.

To address this limitation this tool reads the JSON save itself and pulls out each LUA script into a seperate file.  It also creates a nested folder structure for containers such as bags and decks.  It uses the same naming convention as Atom's Tabletop Simulator plugin so you can use either tool.

This tool will either extract or embed LUA into a save.  This allows you to extract the LUA, then use Atom or any editor to modify the LUA files and then put them back into the save.

# Usage

To extract LUA from JSON save file:
```
TTSLuaExtractor extract InputSave OutputFolder
```
Instead of InputSave, use 'latest' to load the most recent save ignoring the autosave slot
```
TTSLuaExtractor extract "C:\Users\KarateSnoopy\Documents\My Games\Tabletop Simulator\Saves\TS_Save_51.json" c:\git\myTTSboard
TTSLuaExtractor extract latest c:\git\myTTSboard
```


To embed LUA into JSON save file:
```
TTSLuaExtractor embed OutputSave InputFolder IncludeFolder
```
Instead of OutputSave, use 'latest' to load the most recent save ignoring the autosave slot
```
TTSLuaExtractor embed "C:\Users\KarateSnoopy\Documents\My Games\Tabletop Simulator\Saves\TS_Save_51.json" c:\git\myTTSboard c:\git\myTTSboard\shared
TTSLuaExtractor embed latest c:\git\myTTSboard c:\git\myTTSboard\shared
```
