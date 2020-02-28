using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CstLemmaLibrary
{

    public static class CstLemmaWrapper
    {
        public static Dictionary<string, List<string>> GetLemmasByTextDictionary(List<string> texts)
        {
            Guid tempGuid = Guid.NewGuid();
            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); // nifty way of getting the correct working directory
            string fullTempFolder = Path.Combine(currentDirectory, tempGuid.ToString());

            string fullFlexFilePath = Path.Combine(currentDirectory, "flexrules");
            string fullDictFilePath = Path.Combine(currentDirectory, "dict");

            Directory.CreateDirectory(fullTempFolder);
            string inputFileName = "input.txt";
            string fullInputFilePath = Path.Combine(fullTempFolder, inputFileName);
            File.AppendAllLines(fullInputFilePath, texts);


            string outputFileName = "output.txt";
            string fullOutFilePath = Path.Combine(fullTempFolder, outputFileName);

            string outputMessage;
            using (Process process = new Process())
            {
                if (Environment.Is64BitProcess)
                {
                    process.StartInfo.FileName = Path.Combine(currentDirectory, "cstlemma64.exe");
                }
                else
                {
                    process.StartInfo.FileName = Path.Combine(currentDirectory, "cstlemma.exe");
                }
                process.StartInfo.Arguments = @"-L -eU -p- -q- -t- -f " + fullFlexFilePath + " -l -d " + fullDictFilePath + @" -b""$w"" -B""$w"" -c""\[[$b$b?]>0[$B$b0]\]$s"" -u -i " + fullInputFilePath + " -o " + fullOutFilePath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                outputMessage = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            string[] linesFromFile = File.ReadAllLines(fullOutFilePath);
            Directory.Delete(fullTempFolder, true);
            linesFromFile = linesFromFile.Take(linesFromFile.Length - 1).ToArray(); // Last element is always empty
            Dictionary<string, List<string>> lemmasByText = new Dictionary<string, List<string>>();
            if (texts.Count == linesFromFile.Length)
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    List<string> lemmas = new List<string>();
                    Regex regex = new Regex(@"\[(.*?)\]");
                    MatchCollection match = regex.Matches(linesFromFile[i]);
                    foreach (Match item in match)
                    {
                        lemmas.Add(item.Groups[1].Value);
                    }
                    lemmasByText.Add(texts[i], lemmas);

                }
            }

            return lemmasByText;
        }
    }
}
