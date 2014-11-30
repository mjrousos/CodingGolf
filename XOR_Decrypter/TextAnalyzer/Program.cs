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
            int[] charCount = new int[255];
            int[][] nextCharCount = new int[255][];
            int[][] prevCharCount = new int[255][];
            
            for (byte i = 0; i < 255; i++)
            {
                nextCharCount[i] = new int[255];
                prevCharCount[i] = new int[255];
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
                sw.WriteLine(@"
// Data from text analysis of files in " + Path.GetFileName(args[0]) + @"

using System;
using System.Collections.Generic;

namespace Cryptography
{
    public static partial class FrequencyTables
    {
        #region Single char frequency
        public static double[] EnglishGutenbergChraFreq = new double[] {");
        for (int i = 0; i < charCount.Length; i++)
        {
            sw.WriteLine("            " + (((double)charCount[i]) / totalChars).ToString("F5") + ",  // " + i.ToString() + " : " + (char)(byte)i);
        }
        sw.WriteLine(@"        }
        #endregion Single char frequency

        #region Next char frequency
        public static double[][] EnglishGutenbergNextCharFreq = new double[][] {");
        for (int i = 0; i < 255; i++)
        {
            int count = nextCharCount[i].Sum();
            sw.WriteLine(@"
            // Following " + i.ToString());
            if (count == 0)
            {
                sw.WriteLine("            null,");
            }
            else
            {
                sw.WriteLine(@"            new double[] {");
                for (int j = 0; j < 255; j++)
                {
                    sw.WriteLine("                " + (((double)nextCharCount[i][j]) / count).ToString("F5") + ",  // " + j.ToString());
                }
                sw.WriteLine(@"
            },");
            }
        }
        sw.WriteLine(@"
        };
        #endregion Next char frequency");

        sw.WriteLine(@"
        #region Prev char frequency
        public static double[][] EnglishGutenbergPrevCharFreq = new double[][] {");
        for (int i = 0; i < 255; i++)
        {
            int count = prevCharCount[i].Sum();
            sw.WriteLine(@"
            // Preceding " + i.ToString());
            if (count == 0)
            {
                sw.WriteLine("            null,");
            }
            else
            {
                sw.WriteLine(@"            new double[] {");
                for (int j = 0; j < 255; j++)
                {
                    sw.WriteLine("                " + (((double)prevCharCount[i][j]) / count).ToString("F5") + ",  // " + j.ToString());
                }
                sw.WriteLine(@"
            },");
            }
        }
        sw.WriteLine(@"
        };
        #endregion Prev char frequency");

        sw.WriteLine(@"
    }
}
");
            }

            Console.WriteLine("Output written to " + output);
        }
    }
}
