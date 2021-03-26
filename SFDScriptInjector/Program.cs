using SFDScriptInjector.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SFDScriptInjector
{
    static class Program
    {

        private static string MapDependantDataFileContent;
        private static string MapDependantDataText;

        private static string HardcoreClassFilePath;
        private static string HardcoreClassFileContent;
        private static string HardcoreScriptText;

        private static string ScriptToInsertIntoMap;

        private static string SFDMapFilePath;
        private static byte[] SFDMapFileBytesContent;


        static async Task<int> Main(string[] args)
        {
            try
            {
                // another class for arguments management?
                await GetFileContentsAndPathsFromArguments(args);

                // one script from hardcore.cs
                await GetScriptTextFromFileContent();
                // and another script from mapdependantdata.cs each would have its own instance of SFDScript
                await GetMapDependantDataFromMapDependantDataFileContent();

                // script aggregator does this via a method
                CreateScriptTextToInsertInMap();

                // script injector does this via a method
                InsertSFDScriptTextIntoSFDMapFileContent();

                // having received a Map with desired contents, we can just persist its contents
                await WriteSFDMapFileBytesIntoSFDMapFile();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected error: " + e.Message);
                return 1;
            }
        }

        #region Arguments and Checks
        private static async Task GetFileContentsAndPathsFromArguments(string[] args)
        {
            VerifyArgumentsAndGetFilePaths(args);
            await ReadAndVerifyFileContents();
        }

        private static void VerifyArgumentsAndGetFilePaths(string[] args)
        {
            CheckThatArgumentsAreFilled(args);
            CheckAndRecoverBothFilePaths(args);
        }

        private static void CheckThatArgumentsAreFilled(string[] args)
        {
            if (!ThereAreEnoughArguments(args))
            {
                throw new InvalidOperationException("Use the app by passing a C# file path as the first parameter and a SFD Map (*.sfdm) as a second parameter.");
            }
        }

        private static void CheckAndRecoverBothFilePaths(string[] args)
        {
            HardcoreClassFilePath = RecoverFirstArgument(args);
            SFDMapFilePath = RecoverSecondArgument(args);

            CheckThatBothFilesExist(HardcoreClassFilePath, SFDMapFilePath);
        }

        private static void CheckThatBothFilesExist(string cSharpFilePath, string sfdMapFilePath)
        {
            if (!File.Exists(cSharpFilePath))
                throw new InvalidOperationException($"A file does not exist at path: \"{cSharpFilePath}\"");

            if (!File.Exists(sfdMapFilePath))
                throw new InvalidOperationException($"A file does not exist at path: \"{sfdMapFilePath}\"");
        }

        private static string RecoverSecondArgument(string[] args)
        {
            return args[1];
        }

        private static string RecoverFirstArgument(string[] args)
        {
            return args[0];
        }

        private static bool ThereAreEnoughArguments(string[] args)
        {
            return args.Length > 1;
        }

        #endregion

        #region Read And Get File Contents

        private static async Task ReadAndVerifyFileContents()
        {
            // to do by script injector from the received files
            await ReadAndVerifyHardcoreClassFileContent();
            await ReadAndVerifyMapDependantDataFileContent();
            await ReadAndVerifySFDMapFileContent();
        }

        private static async Task ReadAndVerifyHardcoreClassFileContent()
        {
            HardcoreClassFileContent = await File.ReadAllTextAsync(HardcoreClassFilePath, CancellationToken.None);

            if (HardcoreClassFileContent == null || HardcoreClassFileContent.Length < 0)
            {
                throw new InvalidDataException($"The file at {HardcoreClassFilePath} has no data inside or was null");
            }
        }

        private static async Task ReadAndVerifyMapDependantDataFileContent()
        {
            var lastSlashIndex = SFDMapFilePath.LastIndexOf("\\");
            var mapDependantDataFolder = SFDMapFilePath.Substring(0, SFDMapFilePath.Substring(0, lastSlashIndex).LastIndexOf("\\") + 1);
            var mapFileName = SFDMapFilePath.Substring(lastSlashIndex + 1, (SFDMapFilePath.Length - 1) - lastSlashIndex);
            var mapClassFileName = mapFileName.Replace(".sfdm", ".cs").Replace(" ", string.Empty);
            var mapClassFilePath = mapDependantDataFolder + mapClassFileName;

            MapDependantDataFileContent = await File.ReadAllTextAsync(mapClassFilePath, CancellationToken.None);
            if (MapDependantDataFileContent.Length < 0)
            {
                throw new InvalidDataException($"No data was found inside the map dependant data file at {mapClassFilePath}");
            }
        }

        private static async Task ReadAndVerifySFDMapFileContent()
        {
            if (string.IsNullOrEmpty(SFDMapFilePath)) throw new InvalidOperationException("Cannot recover map file content from an empty map file path.");

            SFDMapFileBytesContent = await File.ReadAllBytesAsync(SFDMapFilePath, CancellationToken.None);

            if (SFDMapFileBytesContent.Length < 0) throw new InvalidDataException($"No content was found inside the map file read at {SFDMapFilePath}");
        }

        private static async Task GetMapDependantDataFromMapDependantDataFileContent()
        {
            if (string.IsNullOrEmpty(MapDependantDataFileContent))
            {
                throw new InvalidDataException("The map dependant data file content is null or empty");
            }

            await DoRegexToRecoverMapDependantDataTextFromFileContent();

            if (string.IsNullOrEmpty(MapDependantDataText))
            {
                throw new InvalidDataException($"The regex could not find the needed information inside the map dependant data file.");
            }
        }

        private static async Task DoRegexToRecoverMapDependantDataTextFromFileContent()
        {
            await Task.Run(() =>
            {
                Regex regex = new Regex(@"#region Map Dependant Data([\s\S\r]*)#endregion");
                string incomingValue = MapDependantDataFileContent;
                Match match = regex.Match(incomingValue);
                if (!match.Success) return;
                MapDependantDataText = match.Groups.Values.FirstOrDefault()?.Value.ToString();
            });
        }

        private static async Task GetScriptTextFromFileContent()
        {
            if (string.IsNullOrEmpty(HardcoreClassFileContent))
            {
                throw new InvalidDataException("The data inside the script class file was null or empty");
            }
            await DoRegexToRecoverHardcoreScriptTextFromFileContent();

            if (string.IsNullOrEmpty(HardcoreScriptText))
            {
                throw new InvalidDataException($"No script text could be found after regex execution inside {HardcoreClassFilePath}");
            }
        }

        private static async Task DoRegexToRecoverHardcoreScriptTextFromFileContent()
        {
            await Task.Run(() =>
            {
                Regex regex = new Regex(@"#region Generic Script([\s\S\r]*)#endregion");
                string incomingValue = HardcoreClassFileContent;
                var match = regex.Match(incomingValue);
                if (!match.Success) return;
                HardcoreScriptText = "#region Generic Script \n" + match.Groups[1]?.Value.ToString();
            });
        }
        #endregion

        #region Creating New Script Text To Insert
        private static void CreateScriptTextToInsertInMap()
        {
            ScriptToInsertIntoMap = MapDependantDataText + '\n' + HardcoreScriptText;
        }

        #endregion

        #region Writing New Script Into SFD Map File

        private static async Task WriteSFDMapFileBytesIntoSFDMapFile()
        {
            if (SFDMapFileBytesContent == null)
            {
                throw new InvalidOperationException("Cannot save empty content for the generated map file");
            }
            await File.WriteAllBytesAsync(SFDMapFilePath, SFDMapFileBytesContent, CancellationToken.None);
        }

        private static void InsertSFDScriptTextIntoSFDMapFileContent()
        {
            if (string.IsNullOrEmpty(ScriptToInsertIntoMap))
            {
                throw new InvalidOperationException("The script to insert into the map file was null or empty");
            }

            FindingAndReplacingScriptTextWithBytes();

        }



        private static void FindingAndReplacingScriptTextWithBytes()
        {
            var scriptTagInBytes = Encoding.UTF8.GetBytes("c_scrpt");
            var numberOfCharactersInScriptTag = scriptTagInBytes.Length;

            var nextTagInBytes = Encoding.UTF8.GetBytes("c_lr");

            var indexArrayOfFirstTagInBytes = SFDMapFileBytesContent.Locate(scriptTagInBytes);
            var indexesArrayNextTagInBytes = SFDMapFileBytesContent.Locate(nextTagInBytes);

            if (indexArrayOfFirstTagInBytes.Length == 0 || indexesArrayNextTagInBytes.Length == 0) return;

            var indexOfFirstTagInBytes = indexArrayOfFirstTagInBytes[0];
            var indexOfNextTagInBytes = indexesArrayNextTagInBytes[0];
            var endOfTransmissionCharacterCount = 1;
            var indexOfFirstSizeByte = indexOfFirstTagInBytes + numberOfCharactersInScriptTag;
            var indexOfByteBeforeFirstSizeByte = indexOfFirstSizeByte - 1;
            var indexOfLastByteOfScript = indexOfNextTagInBytes - endOfTransmissionCharacterCount - 1;
            var indexOfByteAfterLastByteOfScript = indexOfLastByteOfScript + 1;


            var newScriptTextBytes = Encoding.UTF8.GetBytes(ScriptToInsertIntoMap);
            var newScriptTextInBase64 = Convert.ToBase64String(newScriptTextBytes);

            var newScriptInBase64SizePrefixedInBytes = newScriptTextInBase64.ToSizePrefixedByteArray();

            var byteArrayUntilByteBeforeFirstSizeByte = new byte[indexOfByteBeforeFirstSizeByte + 1];

            var bytesCountFromByteAfterLastByteOfScriptUntilTheEnd = SFDMapFileBytesContent.Length - indexOfByteAfterLastByteOfScript;
            var byteArrayFromByteAfterLastByteOfScriptUntilTheEnd = new byte[bytesCountFromByteAfterLastByteOfScriptUntilTheEnd + 1];

            Array.Copy(SFDMapFileBytesContent, sourceIndex: 0, destinationArray: byteArrayUntilByteBeforeFirstSizeByte, destinationIndex: 0, length: indexOfByteBeforeFirstSizeByte + 1);
            Array.Copy(SFDMapFileBytesContent, sourceIndex: indexOfByteAfterLastByteOfScript, destinationArray: byteArrayFromByteAfterLastByteOfScriptUntilTheEnd, destinationIndex: 0, length: bytesCountFromByteAfterLastByteOfScriptUntilTheEnd);

            SFDMapFileBytesContent = ByteExtensions.CombineByteArrays(byteArrayUntilByteBeforeFirstSizeByte, newScriptInBase64SizePrefixedInBytes, byteArrayFromByteAfterLastByteOfScriptUntilTheEnd);
        }

        #endregion
    }
}
