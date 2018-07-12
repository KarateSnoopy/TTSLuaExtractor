// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace TTSLuaExtractor
{
    class Program
    {
        static void ShowHelp()
        {
            Console.WriteLine("To extract LUA from JSON save file:");
            Console.WriteLine("TTSLuaExtractor extract InputSave OutputFolder");
            Console.WriteLine("Instead of InputSave, use 'latest' to load the most recent save ignoring the autosave slot");
            Console.WriteLine("ex:");
            Console.WriteLine("TTSLuaExtractor extract \"C:\\Users\\KarateSnoopy\\Documents\\My Games\\Tabletop Simulator\\Saves\\TS_Save_51.json\" c:\\git\\myTTSboard");
            Console.WriteLine("TTSLuaExtractor extract latest c:\\git\\myTTSboard");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("To embed LUA into JSON save file:");
            Console.WriteLine("TTSLuaExtractor embed OutputSave InputFolder IncludeFolder");
            Console.WriteLine("Instead of OutputSave, use 'latest' to load the most recent save ignoring the autosave slot");
            Console.WriteLine("ex:");
            Console.WriteLine("TTSLuaExtractor embed \"C:\\Users\\KarateSnoopy\\Documents\\My Games\\Tabletop Simulator\\Saves\\TS_Save_51.json\" c:\\git\\myTTSboard c:\\git\\myTTSboard\\shared");
            Console.WriteLine("TTSLuaExtractor embed latest c:\\git\\myTTSboard c:\\git\\myTTSboard\\shared");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            if ( args[0] == "extract" )
            {
                if (args.Length != 3)
                {
                    ShowHelp();
                    return;
                }

                ExtractLua(args);
            }
            else if (args[0] == "embed")
            {
                if (args.Length != 4)
                {
                    ShowHelp();
                    return;
                }

                EmbedLua(args);
            }
            else
            {
                ShowHelp();
                return;
            }
        }

        static void EmbedLua(string[] args)
        {
            string OutputSave = args[1];
            string InputFolder = args[2];
            string IncludePath = args[3];
            Console.WriteLine("InputFolder: " + InputFolder);

            if (OutputSave == "latest")
            {
                OutputSave = GetLatestSave();
            }
            Console.WriteLine("OutputSave: " + OutputSave);

            if (!File.Exists(OutputSave))
            {
                Console.WriteLine("OutputSave doesn't exist");
                return;
            }
            if (!Directory.Exists(InputFolder))
            {
                Console.WriteLine("InputFolder doesn't exist");
                return;
            }
            if (!Directory.Exists(IncludePath))
            {
                Console.WriteLine("IncludePath doesn't exist");
                return;
            }
            Console.WriteLine($"IncludePath: {IncludePath}");

            string json = File.ReadAllText(OutputSave);
            dynamic d = JObject.Parse(json);
            Console.WriteLine("SaveName: " + (string)d.SaveName);
            foreach (dynamic obj in d.ObjectStates)
            {
                EmbedLuaInObject(InputFolder, obj, IncludePath);
            }

            string newTxt = d.ToString(Formatting.Indented);
            File.WriteAllText(OutputSave, newTxt);
        }

        static void EmbedLuaInObject(string inputFolder, dynamic obj, string includePath)
        {
            bool hasLua = obj.LuaScript != "";
            bool hasContained = obj.ContainedObjects != null;

            string name = obj.Nickname;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = obj.Name;
                name = name.Replace($"_", " "); // for some reason Atom doesn't change '_' to ' ' in nicknames, only names.
            }

            if (hasLua)
            {
                //Console.WriteLine($"Object: {obj.GUID} LUA:{hasLua} HasContained:{hasContained} {obj.Name}, {obj.Nickname}");

                string inputFile;

                inputFile = $"{name}.{obj.GUID}.ttslua";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    inputFile = inputFile.Replace($"{c}", "");
                }
                string inputFullPath = Path.Combine(inputFolder, inputFile);
                FileInfo fi = new FileInfo(inputFullPath);
                if( fi.Exists )
                {
                    Console.WriteLine($"Reading from {inputFullPath}");
                    string lua = File.ReadAllText(inputFullPath);
                    lua = UncompressIncludes(lua, "", includePath);
                    obj.LuaScript = lua;
                }
                else
                {
                    Console.WriteLine($"Script not found.  {inputFullPath}");
                }
            }

            if (hasContained)
            {
                string outputFolderName;
                outputFolderName = $"{name}.{obj.GUID}";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    outputFolderName = outputFolderName.Replace($"{c}", "");
                }
                string subFolder = Path.Combine(inputFolder, outputFolderName);

                foreach (dynamic innerObj in obj.ContainedObjects)
                {
                    EmbedLuaInObject(subFolder, innerObj, includePath);
                }
            }
        }

        static string GetLatestSave()
        {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fullPath = Path.Combine(myDocs, "My Games\\Tabletop Simulator\\Saves");
            var jsonFiles = Directory.EnumerateFiles(fullPath, "*.json", SearchOption.AllDirectories);

            DateTime dt = DateTime.MinValue;
            string latestFile = string.Empty;
            foreach (string jsonFile in jsonFiles)
            {
                FileInfo fi = new FileInfo(jsonFile);
                if (fi.Name == "SaveFileInfos.json" || fi.Name == "TS_AutoSave.json")
                    continue;

                if (fi.LastWriteTime > dt)
                {
                    dt = fi.LastWriteTime;
                    latestFile = jsonFile;
                }
                
            }

            return latestFile;
        }

        static void ExtractLua(string[] args)
        {
            string inputFile = args[1];
            string outputFolder = args[2];
            Console.WriteLine("OutputFolder: " + outputFolder);

            if (inputFile == "latest")
            {
                inputFile = GetLatestSave();
            }
            Console.WriteLine("InputSave: " + inputFile);

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("InputFile doesn't exist");
                return;
            }
            Directory.CreateDirectory(outputFolder);
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("OutputFolder doesn't exist");
                return;
            }

            string json = File.ReadAllText(inputFile);
            dynamic d = JObject.Parse(json);
            Console.WriteLine("SaveName: " + (string)d.SaveName);
            foreach (dynamic obj in d.ObjectStates)
            {
                ExtractLuaFromObject(outputFolder, obj);
            }
        }

        static string CompressIncludes(string lua)
        {            
            while(true) // Keep looking for #includes in this file until there's no more to be found
            {
                // Looking for something like 
                // ----#include shared/utils
                int firstIncludeIndex = lua.IndexOf("----#include");
                if(firstIncludeIndex == -1)
                {
                    break;
                }

                int firstIncludeIndexEOL = lua.IndexOf("\n", firstIncludeIndex);
                if (firstIncludeIndexEOL == -1)
                {
                    break;
                }
                string originalIncludeLine = lua.Substring(firstIncludeIndex, firstIncludeIndexEOL - firstIncludeIndex);
                string commentedIncludeLine = originalIncludeLine.Replace("----#include", "#include");

                // Find the next matching ----#include.  
                // Everything in-between can be deleted and collapsed
                // into a new line such as
                // #include shared/utils
                int nextIncludeIndex = lua.IndexOf(originalIncludeLine, firstIncludeIndexEOL+1);
                int nextIncludeIndexEOL = lua.IndexOf("\n", nextIncludeIndex);
                if(nextIncludeIndexEOL == -1)
                {
                    lua = lua.Remove(firstIncludeIndex);
                }
                else
                {
                    lua = lua.Remove(firstIncludeIndex, nextIncludeIndexEOL - firstIncludeIndex);
                }

                lua = lua.Insert(firstIncludeIndex, commentedIncludeLine);
            }

            return lua;
        }

        static string ExtractBaseFolder(string baseFolder, string includeFileName)
        {
            string newBaseFolder = baseFolder;

            int newBaseFolderSlash = includeFileName.LastIndexOf("\\");
            if (newBaseFolderSlash != -1)
            {
                newBaseFolder = baseFolder + "/" + includeFileName.Remove(newBaseFolderSlash);
            }

            newBaseFolderSlash = includeFileName.LastIndexOf("/");
            if (newBaseFolderSlash != -1)
            {
                newBaseFolder = baseFolder + "/" + includeFileName.Remove(newBaseFolderSlash);
            }

            if (newBaseFolder.IndexOf("/") == 0)
            {
                newBaseFolder = newBaseFolder.Remove(0, 1);
            }

            return newBaseFolder;
        }

        static string UncompressIncludes(string lua, string baseFolder, string includePath)
        {
            // Repeatedly look for "#include file" and replace them with these 3 lines:
            // ----#include file
            // <contents of file>
            // ----#include file

            // First, switch all #include to %include so we don't get confused later when 
            // it starts inserting "----#include file" lines
            lua = lua.Replace("#include", "%include");
            while (true)
            {
                int firstIncludeIndex = lua.IndexOf("%include");
                if (firstIncludeIndex == -1)
                {
                    break; // Stop if no more #includes to find
                }

                int firstIncludeIndexEOL = lua.IndexOf("\n", firstIncludeIndex);
                string commentedIncludeLine;
                string includeFileName;
                if (firstIncludeIndexEOL == -1)
                {
                    commentedIncludeLine = lua.Substring(firstIncludeIndex);
                    includeFileName = lua.Substring(firstIncludeIndex + 9);
                }
                else
                {
                    commentedIncludeLine = lua.Substring(firstIncludeIndex, firstIncludeIndexEOL - firstIncludeIndex);
                    includeFileName = lua.Substring(firstIncludeIndex + 9, firstIncludeIndexEOL - (firstIncludeIndex + 9));
                }

                includeFileName = includeFileName.Replace("\r", "").Replace("\n", "");
                // Now includeFileName is something like:
                // shared/util

                commentedIncludeLine = commentedIncludeLine.Replace("\r", "");
                commentedIncludeLine = commentedIncludeLine.Replace("%include", "----#include");
                string commentedIncludeLine2 = commentedIncludeLine;
                commentedIncludeLine += "\n";
                // Now commentedIncludeLine should be something like:
                // ----#include shared/util\n

                string sharedFilePath = Path.Combine(includePath, baseFolder);

                // Extract the base folder from the include file.
                // For example, if the includeFileName is "shared/util", then
                // newBaseFolder will be "shared"
                string newBaseFolder = ExtractBaseFolder(baseFolder, includeFileName);

                // Read the contents of the #included LUA file, but uncompress it
                // so that'll expand any includes it has it in recursively 
                string sharedFullFile = Path.Combine(sharedFilePath, includeFileName + ".ttslua");
                string sharedFileContents = File.ReadAllText(sharedFullFile);
                sharedFileContents = UncompressIncludes(sharedFileContents, newBaseFolder, includePath);

                // First, remove the "#include file" line
                if (firstIncludeIndexEOL == -1 )
                {
                    lua = lua.Remove(firstIncludeIndex);
                }
                else
                {
                    lua = lua.Remove(firstIncludeIndex, firstIncludeIndexEOL - firstIncludeIndex);
                }

                // Then add these:
                // ----#include file
                // <contents of file>
                // ----#include file
                lua = lua.Insert(firstIncludeIndex, commentedIncludeLine2);
                lua = lua.Insert(firstIncludeIndex, "\n");
                lua = lua.Insert(firstIncludeIndex, commentedIncludeLine);
                lua = lua.Insert(firstIncludeIndex + commentedIncludeLine.Length, sharedFileContents);
            }

            return lua;
        }

        static void ExtractLuaFromObject(string outputFolder, dynamic obj)
        {
            bool hasLua = obj.LuaScript != "";
            bool hasContained = obj.ContainedObjects != null;
            //Console.WriteLine($"Object: {obj.GUID} LUA:{hasLua} HasContained:{hasContained} {obj.Name}, {obj.Nickname}");

            string name = obj.Nickname;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = obj.Name;
                name = name.Replace($"_", " "); // for some reason Atom doesn't change '_' to ' ' in nicknames, only names.
            }

            if (hasLua)
            {
                string outputFile;
                outputFile = $"{name}.{obj.GUID}.ttslua";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    outputFile = outputFile.Replace($"{c}", "");
                }
                string outputFullPath = Path.Combine(outputFolder, outputFile);
                Console.WriteLine($"Writing {outputFullPath}");
                string lua = obj.LuaScript;
                lua = CompressIncludes(lua);
                Directory.CreateDirectory(outputFolder);
                File.WriteAllText(outputFullPath, lua);
            }

            if (hasContained)
            {
                string outputFolderName;
                outputFolderName = $"{name}.{obj.GUID}";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    outputFolderName = outputFolderName.Replace($"{c}", "");
                }

                string subFolder = Path.Combine(outputFolder, outputFolderName);
                foreach (dynamic innerObj in obj.ContainedObjects)
                {
                    ExtractLuaFromObject(subFolder, innerObj);
                }
            }
        }
    }
}
