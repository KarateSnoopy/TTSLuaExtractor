# TTSLuaExtractor

LUA extractor for Tabletop Simulator

Atom's Tabletop Simulator plugin doesn't currently extract LUA scripts for objects nested inside bags or decks.  Most complex Tabletop Simulator boards have scripts on objects that either inside bags or inside decks.  Updating these scripts is painful with the existing workflow because you have to pull it out of the bag or deck and then save the game, update the script using Atom, and then put it back into the bag manually.  Some complex boards have sciprts on objects that are inside nested bags which are very hard to get to and reassemble. 

To address this limitation, this tool reads the JSON save itself and pulls out each LUA script into a seperate file.  It drops that file in a nested folder structure, so its easy to deal with scripts inside nested containers such as bags and decks.  It uses the same naming convention as Atom's Tabletop Simulator plugin so you can use either tool for scripts that are outside bags.

This tool also embed the LUA back into a save.  This allows you to extract the LUA scripts, then use Atom or any editor to modify the LUA scripts and then embed them back into the save.

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
