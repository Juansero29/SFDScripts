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

        private static string CSharpFilePath;
        private static string CSharpFileContent;
        private static string SFDScriptText;


        private static string SFDMapFilePath;
        private static byte[] SFDMapFileBytesContent;
        public static string SFDMapFileTextContent;




        static async Task<int> Main(string[] args)
        {
            await GetFileContentsAndPathsFromArguments(args);
            await GetSFDScriptTextFromCSharpFileContent();
            InsertSFDScriptTextIntoSFDMapFileContent();
            // await WriteSFDMapFileContentIntoSFDMapFile();
            await WriteSFDMapFileBytesIntoSFDMapFile();

            return Result;
        }

        private static async Task WriteSFDMapFileBytesIntoSFDMapFile()
        {
            if (SFDMapFileBytesContent != null)
            {
                Result = 1;
                await Task.FromResult(default(Task));
            }
            using FileStream fout = new FileStream(SFDMapFilePath + ".tmp", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using var bw = new BinaryWriter(fout);
            bw.Write(SFDMapFileBytesContent);
            bw.Close();
            // await File.WriteAllBytesAsync(SFDMapFilePath + ".tmp", SFDMapFileBytesContent, CancellationToken.None);
        }

        private static async Task GetSFDScriptTextFromCSharpFileContent()
        {
            if (string.IsNullOrEmpty(CSharpFileContent))
            {
                Result = 1;
                return;
            }


            await Task.Run(() =>
            {
                Regex regex = new Regex(@"#region Generic Script([\s\S\r]*)#endregion");
                string incomingValue = CSharpFileContent;
                Match match = regex.Match(incomingValue);
                if (match.Success)
                {
                    SFDScriptText = match.Groups.Values.FirstOrDefault()?.Value.ToString();
                }
            });

            if (string.IsNullOrEmpty(SFDScriptText))
            {
                Result = 1;
            }
        }

        private static void InsertSFDScriptTextIntoSFDMapFileContent()
        {
            if (string.IsNullOrEmpty(SFDScriptText))
            {
                Result = 1;
                return;
            }

            FindingAndReplacingScriptTextWithBytes();

            // FindingAndReplacingWithRegex();
        }

        private static void FindingAndReplacingScriptTextWithBytes()
        {
            var numberOfCharactersInScriptTag = 7;
            var scriptTagInBytes = Encoding.UTF8.GetBytes("c_scrpt");

            var numberOfPaddingBytesBetweenScriptTagAndScriptText = 3;

            var bytesFromFirstScriptTagByteUntilFirstScriptTextByte = numberOfCharactersInScriptTag + numberOfPaddingBytesBetweenScriptTagAndScriptText;

            var nextTagInBytes = Encoding.UTF8.GetBytes("c_lr");


            var indexArrayOfFirstTagInBytes = SFDMapFileBytesContent.Locate(scriptTagInBytes);
            var indexesArrayNextTagInBytes = SFDMapFileBytesContent.Locate(nextTagInBytes);


            if (indexArrayOfFirstTagInBytes.Length == 0 || indexesArrayNextTagInBytes.Length == 0) return;

            var indexOfFirstTagInBytes = indexArrayOfFirstTagInBytes[0];
            var indexOfNextTagInBytes = indexesArrayNextTagInBytes[0];
            var endOfTransmissionCharacterCount = 1;


            var indexOfFirstByteOfScript = indexOfFirstTagInBytes + bytesFromFirstScriptTagByteUntilFirstScriptTextByte;
            var indexOfLastByteOfScript = indexOfNextTagInBytes - endOfTransmissionCharacterCount - 1;
            
            var bytesCountUntilByteBeforeFirstByteOfScript = indexOfFirstByteOfScript - 1;
            var indexOfByteAfterLastByteOfScript = indexOfLastByteOfScript + 1;

            var firstByteOfScriptText = SFDMapFileBytesContent[indexOfFirstByteOfScript];
            var lastByteOfScriptText = SFDMapFileBytesContent[indexOfLastByteOfScript];

            // SFDMapFileBytesContent[indexOfNextTagInBytes[0] - 1]
            // System.Text.Encoding.UTF8.GetString(new byte[] { SFDMapFileBytesContent[indexOfLastByteOfScript] })

            var bytesCountFromByteAfterLastByteOfScriptUntilTheEnd = SFDMapFileBytesContent.Length - indexOfByteAfterLastByteOfScript;

            var oldScriptTextBase64InBytes = new byte[indexOfLastByteOfScript - indexOfFirstByteOfScript + 1];
            Array.Copy(SFDMapFileBytesContent, indexOfFirstByteOfScript, oldScriptTextBase64InBytes, 0, oldScriptTextBase64InBytes.Length);
            var oldScriptTextInBase64 = System.Text.Encoding.UTF8.GetString(oldScriptTextBase64InBytes);
            var oldScriptTextInBytes = Convert.FromBase64String(oldScriptTextInBase64);
            var oldScriptText = System.Text.Encoding.UTF8.GetString(oldScriptTextInBytes);

            var scriptTextBytes = System.Text.Encoding.UTF8.GetBytes(SFDScriptText);
            var newScriptTextInBase64 = Convert.ToBase64String(scriptTextBytes);
            var newScriptTextInBytes = Encoding.UTF8.GetBytes(newScriptTextInBase64);
            var byteArrayUntilByteBeforeFirstByteOfScript = new byte[bytesCountUntilByteBeforeFirstByteOfScript + 1];
            var byteArrayFromByteAfterLastByteOfScriptUntilTheEnd = new byte[bytesCountFromByteAfterLastByteOfScriptUntilTheEnd + 1];

            Array.Copy(SFDMapFileBytesContent, sourceIndex: 0, destinationArray: byteArrayUntilByteBeforeFirstByteOfScript, destinationIndex: 0, length: bytesCountUntilByteBeforeFirstByteOfScript);
            Array.Copy(SFDMapFileBytesContent, sourceIndex: indexOfByteAfterLastByteOfScript, destinationArray: byteArrayFromByteAfterLastByteOfScriptUntilTheEnd, destinationIndex:  0, length:  bytesCountFromByteAfterLastByteOfScriptUntilTheEnd);

            SFDMapFileBytesContent = CombineByteArrays(byteArrayUntilByteBeforeFirstByteOfScript, newScriptTextInBytes, byteArrayFromByteAfterLastByteOfScriptUntilTheEnd);
        }


        private static byte[] CombineByteArrays(byte[] a1, byte[] a2, byte[] a3)
        {
            byte[] result = new byte[a1.Length + a2.Length + a3.Length];
            Array.Copy(a1, 0, result, 0, a1.Length);
            Array.Copy(a2, 0, result, a1.Length, a2.Length);
            Array.Copy(a3, 0, result, a1.Length + a2.Length, a3.Length);
            return result;
        }
        private static void FindingAndReplacingScripTextWithRegex()
        {
            Regex regex = new Regex(@"c_scrpt...([A-Za-z0-9+\/=]*)");
            var match = regex.Match(SFDMapFileTextContent);
            var firstIndex = match.Groups[1].Index;
            var lastIndex = (firstIndex + match.Length) - 1;

            //var reg = new Regex(@"c_lr");
            //var metch = reg.Match(SFDMapFileTextContent);
            //var lastIndex = metch.Index;
            var scriptInBase64 = match.Groups[1].Value;

            var startOfDataUntilLastCharacterBeforeScriptString = SFDMapFileTextContent.Substring(0, firstIndex);
            var stringAfterTheLastCharacterOfScriptUntilOfEndOfData = SFDMapFileTextContent.Substring(lastIndex);

            var scriptTextBytes = System.Text.Encoding.UTF8.GetBytes(SFDScriptText);
            var newScriptTextInBase64 = Convert.ToBase64String(scriptTextBytes);

            // byte[] data = Convert.FromBase64String(scriptInBase64);
            // string decodedString = Encoding.UTF8.GetString(data);

            SFDMapFileTextContent = startOfDataUntilLastCharacterBeforeScriptString + newScriptTextInBase64 + stringAfterTheLastCharacterOfScriptUntilOfEndOfData + char.MinValue;
        }

        private static async Task WriteSFDMapFileContentIntoSFDMapFile()
        {
            if (string.IsNullOrEmpty(SFDMapFileTextContent))
            {
                Result = 1;
                await Task.FromResult(default(Task));
            }
            var bytes = Encoding.UTF8.GetBytes(SFDMapFileTextContent);
            await File.WriteAllBytesAsync(SFDMapFilePath + ".tmp", SFDMapFileBytesContent, CancellationToken.None);
        }


        private static async Task GetFileContentsAndPathsFromArguments(string[] args)
        {
            VerifyArgumentsAndGetFilePaths(args);
            await ReadAndVerifyFileContents();
        }

        private static async Task ReadAndVerifyFileContents()
        {
            await ReadAndVerifyCSharpFileContent();
            await ReadAndVerifySFDMapFileContent();
        }

        private static async Task ReadAndVerifySFDMapFileContent()
        {
            SFDMapFileBytesContent = await File.ReadAllBytesAsync(SFDMapFilePath, CancellationToken.None);
            if (SFDMapFileBytesContent.Length < 0)
            {
                Result = 1;
            }

            //using FileStream fs = new FileStream(SFDMapFilePath, FileMode.Open);
            //using StreamReader sr = new StreamReader(fs, true);
            //var stri = await sr.ReadToEndAsync();
            //sr.Close();
            //var ascii = await File.ReadAllTextAsync(SFDMapFilePath, Encoding.ASCII);
            //var utf8 = 
            //var unicode = await File.ReadAllTextAsync(SFDMapFilePath, Encoding.Unicode);
            SFDMapFileTextContent = await File.ReadAllTextAsync(SFDMapFilePath, Encoding.UTF8);
        }


        static readonly int[] Empty = new int[0];

        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }


        private static async Task ReadAndVerifyCSharpFileContent()
        {
            CSharpFileContent = await File.ReadAllTextAsync(CSharpFilePath, CancellationToken.None);
            if (CSharpFileContent.Length < 0)
            {
                Result = 1;
            }
        }

        private static void VerifyArgumentsAndGetFilePaths(string[] args)
        {
            CheckThatArgumentsAreFilled(args);
            CheckAndRecoverBothFilePaths(args);
        }

        private static void CheckAndRecoverBothFilePaths(string[] args)
        {
            CSharpFilePath = RecoverFirstArgument(args);
            SFDMapFilePath = RecoverSecondArgument(args);

            CheckThatBothFilesExist(CSharpFilePath, SFDMapFilePath);
        }

        private static void CheckThatArgumentsAreFilled(string[] args)
        {
            if (!ThereAreEnoughArguments(args))
            {
                Console.WriteLine($"Use the app by passing a C# file path as the first parameter and a SFD Map (*.sfdm) as a second parameter.");
                Result = 1;
            }
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
    }
}
