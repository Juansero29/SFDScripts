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

        private static int Result;

        private static string MapDependantDataFileContent;
        private static string MapDependantDataText;

        private static string HardcoreClassFilePath;
        private static string HardcoreClassFileContent;
        private static string HardcoreClassFileScriptText;

        private static string ScriptToInsertIntoMap;

        private static string SFDMapFilePath;
        private static byte[] SFDMapFileBytesContent;
        public static string SFDMapFileTextContent;
        public static int NumberOfBytesUsedByExistingScriptInBase64 { get; set; }


        static async Task<int> Main(string[] args)
        {
            await GetFileContentsAndPathsFromArguments(args);

            await GetSFDScriptTextFromCSharpFileContent();

            await GetMapDependantDataFromMapDependantDataFileContent();

            CreateScriptTextToInsertInMap();

            InsertSFDScriptTextIntoSFDMapFileContent();

            await WriteSFDMapFileBytesIntoSFDMapFile();

            return Result;
        }
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

        #region Arguments and Checks

        private static void CheckThatArgumentsAreFilled(string[] args)
        {
            if (!ThereAreEnoughArguments(args))
            {
                Console.WriteLine($"Use the app by passing a C# file path as the first parameter and a SFD Map (*.sfdm) as a second parameter.");
                Result = 1;
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
            {
                Console.WriteLine($"A file does not exist at path: \"{cSharpFilePath}\"");
                Result = 1;
            }

            if (!File.Exists(sfdMapFilePath))
            {
                Console.WriteLine($"A file does not exist at path: \"{sfdMapFilePath}\"");
                Result = 1;
            }
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
            await ReadAndVerifyHardcoreClassFileContent();
            await ReadAndVerifyMapDependantDataFileContent();
            await ReadAndVerifySFDMapFileContent();
        }

        private static async Task ReadAndVerifyHardcoreClassFileContent()
        {
            HardcoreClassFileContent = await File.ReadAllTextAsync(HardcoreClassFilePath, CancellationToken.None);
            if (HardcoreClassFileContent.Length < 0)
            {
                Result = 1;
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
                Result = 1;
            }
        }

        private static async Task ReadAndVerifySFDMapFileContent()
        {
            SFDMapFileBytesContent = await File.ReadAllBytesAsync(SFDMapFilePath, CancellationToken.None);
            if (SFDMapFileBytesContent.Length < 0)
            {
                Result = 1;
            }

            SFDMapFileTextContent = await File.ReadAllTextAsync(SFDMapFilePath, Encoding.UTF8);
        }

        private static async Task GetMapDependantDataFromMapDependantDataFileContent()
        {
            if (string.IsNullOrEmpty(MapDependantDataFileContent))
            {
                Result = 1;
                return;
            }

            await Task.Run(() =>
            {
                Regex regex = new Regex(@"#region Map Dependant Data([\s\S\r]*)#endregion");
                string incomingValue = MapDependantDataFileContent;
                Match match = regex.Match(incomingValue);
                if (match.Success)
                {
                    MapDependantDataText = match.Groups.Values.FirstOrDefault()?.Value.ToString();
                }
            });

            if (string.IsNullOrEmpty(HardcoreClassFileScriptText))
            {
                Result = 1;
            }
        }

        private static async Task GetSFDScriptTextFromCSharpFileContent()
        {
            if (string.IsNullOrEmpty(HardcoreClassFileContent))
            {
                Result = 1;
                return;
            }

            await Task.Run(() =>
            {
                Regex regex = new Regex(@"#region Generic Script([\s\S\r]*)#endregion");
                string incomingValue = HardcoreClassFileContent;
                var match = regex.Match(incomingValue);
                if (match.Success)
                {
                    HardcoreClassFileScriptText = "#region Generic Script \n" + match.Groups[1]?.Value.ToString();
                }
            });

            if (string.IsNullOrEmpty(HardcoreClassFileScriptText))
            {
                Result = 1;
            }
        }
        #endregion

        #region Creating New Script Text To Insert
        private static void CreateScriptTextToInsertInMap()
        {
            ScriptToInsertIntoMap = MapDependantDataText + '\n' + HardcoreClassFileScriptText;
        }

        #endregion

        #region Writing New Script Into SFD Map File

        private static async Task WriteSFDMapFileBytesIntoSFDMapFile()
        {
            if (SFDMapFileBytesContent == null)
            {
                Result = 1;
                await Task.FromResult(default(Task));
            }
            await File.WriteAllBytesAsync(SFDMapFilePath, SFDMapFileBytesContent, CancellationToken.None);
        }

        private static void InsertSFDScriptTextIntoSFDMapFileContent()
        {
            if (string.IsNullOrEmpty(ScriptToInsertIntoMap))
            {
                Result = 1;
                return;
            }

            GetNumberOfBytesUsedByExistingScriptInBase64();


            FindingAndReplacingScriptTextWithBytes();

        }

        private static void GetNumberOfBytesUsedByExistingScriptInBase64()
        {
            // https://regex101.com/ - to understand regex
            Regex regex = new Regex(@"c_scrpt[^\x1F-\x7F]+([A-Za-z0-9+\/=]*)");
            var match = regex.Match(SFDMapFileTextContent);
            var scriptInBase64 = match.Groups[1].Value;
            NumberOfBytesUsedByExistingScriptInBase64 = scriptInBase64.Length;
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

        //private static byte[] CombineByteArrays(byte[] a1, byte[] a2, byte[] a3)
        //{
        //    byte[] result = new byte[a1.Length + a2.Length + a3.Length];
        //    Array.Copy(a1, 0, result, 0, a1.Length);
        //    Array.Copy(a2, 0, result, a1.Length, a2.Length);
        //    Array.Copy(a3, 0, result, a1.Length + a2.Length, a3.Length);
        //    return result;
        //}

    }
}
