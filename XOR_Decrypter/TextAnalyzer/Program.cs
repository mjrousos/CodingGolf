using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TextAnalyzer.exe [Path to txt files]");
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("ERROR - Directory does not exist");
                return;
            }

            long totalChars = 0;
            Dictionary<byte, int> charCount = new Dictionary<byte, int>();
            Dictionary<byte, Dictionary<byte, int>> nextCharCount = new Dictionary<byte, Dictionary<byte, int>>(); ;
            Dictionary<byte, Dictionary<byte, int>> prevCharCount = new Dictionary<byte, Dictionary<byte, int>>(); ;

            for (byte i = 0; i < 255; i++)
            {
                charCount.Add(i, 0);
                nextCharCount.Add(i, new Dictionary<byte, int>());
                prevCharCount.Add(i, new Dictionary<byte, int>());
                for (byte j = 0; j < 255; j++)
                {
                    nextCharCount[i].Add(j, 0);
                    prevCharCount[i].Add(j, 0);
                }
            }

            foreach (string file in Directory.GetFiles(args[0]))
            {
                Console.WriteLine("Reading " + file + "...");
                byte[] bytes = File.ReadAllText(file).Replace('\n', ' ').ToLowerInvariant().ToCharArray().Select(c => (byte)c).ToArray();
                totalChars += bytes.Length;

                Console.WriteLine("Analyzing " + file + "...");

                // First char
                charCount[bytes[0]]++;
                prevCharCount[bytes[0]][(byte)' ']++;
                nextCharCount[(byte)' '][bytes[0]]++;

                // Last char
                charCount[bytes[bytes.Length - 1]]++;
                nextCharCount[bytes[bytes.Length - 1]][(byte)' ']++;
                prevCharCount[(byte)' '][bytes[bytes.Length - 1]]++;

                // All other chars
                for (int i = 1; i < bytes.Length - 1; i++)
                {
                    charCount[bytes[i]]++;
                    nextCharCount[bytes[i]][bytes[i + 1]]++;
                    prevCharCount[bytes[i + 1]][bytes[i]]++;
                }

                Console.WriteLine("Done with " + file);
            }

            Console.WriteLine("Analyzed " + totalChars.ToString() + " characters");

            Console.WriteLine("Writing to .cs file...");
            string output = "EnglishGutenbergAnalysis.cs";

            using (StreamWriter sw = new StreamWriter(new FileStream(output, FileMode.Create, FileAccess.ReadWrite)))
            {
                
            }

            Console.WriteLine("Output written to " + output);
        }
    }
}
