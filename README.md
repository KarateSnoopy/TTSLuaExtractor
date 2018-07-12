# TTSLuaExtractor

LUA extractor for Tabletop Simulator

Atom's Tabletop Simulator plugin doesn't currently extract LUA scripts for objects nested inside bags or decks.  Most complex Tabletop Simulator boards have scripts on objects that are on bags.  Updating these scripts is very complex with the existing workflow.

To address this limitation this tool reads the JSON save itself and pulls out each LUA script into a seperate file.  It also creates a nested folder structure for containers such as bags and decks.  It uses the same naming convention as Atom's Tabletop Simulator plugin so you can use either tool.

This tool will either extract or embed LUA into a save.  This allows you to extract the LUA, then use Atom or any editor to modify the LUA files and then put them back into the save.


