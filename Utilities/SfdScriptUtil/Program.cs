using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SfdScriptUtil
{
    
    class Program
    {

        const string start = "/* SCRIPT STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */";
        const string end = "/* SCRIPT ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */";
        const string startClass = "/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */";
        const string endClass = "/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */";

        static Dictionary<string, List<string>> configuration = new Dictionary<string, List<string>>();
        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help"))
            {
                ShowHelp();
                return;
            }

            bool dryRun = true;

            List<string> outputPaths = new List<string>();
            configuration.Add("output", outputPaths);
            configuration.Add("o", outputPaths);

            List<string> headerPaths = new List<string>();
            configuration.Add("header", headerPaths);
            configuration.Add("h", headerPaths);

            List<string> footerPaths = new List<string>();
            configuration.Add("footer", footerPaths);
            configuration.Add("f", footerPaths);

            List<string> inputFolders = new List<string>();
            configuration.Add("sourcedir", inputFolders);
            configuration.Add("s", inputFolders);

            List<string> inputFiles = new List<string>();
            configuration.Add("input", inputFiles);
            configuration.Add("i", inputFiles);

            List<string> excludeFiles = new List<string>();
            configuration.Add("exclude", excludeFiles);
            configuration.Add("e", excludeFiles);


            if (File.Exists(".sfconfig"))
            {
                foreach (string line in File.ReadAllLines(".sfconfig"))
                {
                    string[] lines = line.Split(':');
                    if (lines.Length != 2) continue;


                    string command = line.Split(':')[0].ToLower();
                    string value = line.Split(':')[1].Trim();
                    if (configuration.ContainsKey(command))
                    {
                        configuration[command].Add(value);
                    }
                    else if (command == "dry" || command == "d")
                    {
                        if (value.ToLower() == "false")
                        {
                            dryRun = false;
                        }
                    }
                }
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                string command = args[i].Replace("-", "");
                string value = args[i + 1];

                if (configuration.ContainsKey(command))
                {
                    configuration[command].Add(value);
                }
                else if (command == "dry" || command == "d")
                {
                    if (value.ToLower() == "false")
                    {
                        dryRun = false;
                    }
                }
            }

            string script = "";
            string editorScript = "";

            foreach (string path in headerPaths)
            {
                if (File.Exists(path))
                {
                    script += File.ReadAllText(path) + "\n";
                }
            }

            foreach (string path in inputFiles)
            {
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    content = content.Replace(startClass, start).Replace(endClass, end);
                    content = content.Replace('\t', ' ');
                    Regex rx = new Regex(Regex.Escape(start) + "(.|\n)*?" + Regex.Escape(end));
                    Match rxm = rx.Match(content);
                    if (rxm.Success)
                    {
                        script += rxm.Value.Replace(start, "/* " + Path.GetFileName(path) + " */").Replace(end, "\r\n");
                        editorScript += rxm.Value.Replace(start, "/* " + Path.GetFileName(path) + " */").Replace(end, "\r\n");
                    }
                }
            }

            foreach (string dir in inputFolders)
            {
                foreach (string path in Directory.GetFiles(dir))
                {
                    if (path.ToLower().EndsWith(".cs") && !excludeFiles.Contains(path))
                    {
                        string content = File.ReadAllText(path);
                        content = content.Replace(startClass, start).Replace(endClass, end);
                        content = content.Replace('\t', ' ');
                        Regex rx = new Regex(Regex.Escape(start) + "(.|\n)*?" + Regex.Escape(end));
                        Match rxm = rx.Match(content);
                        if (rxm.Success)
                        {
                            script += rxm.Value.Replace(start, "/* " + Path.GetFileName(path) + " */").Replace(end, "\r\n");
                            editorScript += rxm.Value.Replace(start, "/* " + Path.GetFileName(path) + " */").Replace(end, "\r\n");
                        }
                    }
                }
            }

            foreach (string path in footerPaths)
            {
                if (File.Exists(path))
                {
                    script += File.ReadAllText(path) + "\n";
                }
            }

            foreach (string path in outputPaths)
            {
                File.WriteAllText(path, script);
            }

            if (!dryRun) {
                EditorInterface.PasteScript(editorScript);
                EditorInterface.StartMap();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("tip: you can create .sfconfig file and use the same options in the following format:");
            Console.WriteLine("option: value");
            Console.WriteLine();
            Console.WriteLine("-o | --output [file]".PadRight(30) + "file to write the output to");
            Console.WriteLine("-h | --header [file]".PadRight(30) + "file that will be added to the begging of output. Will not be present in editor");
            Console.WriteLine("-s | --sourcedir [directory]".PadRight(30) + "every .cs file in the directory will be added to output");
            Console.WriteLine("-i | --input [file]".PadRight(30) + "file that will be added to output");
            Console.WriteLine("-e | --exclude [file]".PadRight(30) + "this file won't be included even if it is in sourcedir");
            Console.WriteLine("-f | --footer [file]".PadRight(30) + "file that will be added to the end of output. Will not be present in editor");
            Console.WriteLine("-d | --dry [false]".PadRight(30) + "when set to false, output will be pasted to the editor too, and the map will be launched.");
            Console.WriteLine();
            Console.WriteLine("All options, except --dry can be specified multiple times, keeping the previous value as well.");
            Console.WriteLine("Be aware that .sfconfig headers and footers will be used before the parameter ones.");
        }
    }
}
