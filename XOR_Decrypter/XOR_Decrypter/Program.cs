using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XOR_Decrypter
{
    class Program
    {
        static List<BitArray> KeyPossibilities;
        static List<byte[]> CharsByKey;
        static byte[] Message;

        static void Main(string[] args)
        {
            Console.WriteLine("-----------------");
            Console.WriteLine("- XOR Decrypter -");
            Console.WriteLine("- v0.1; MikeRou -");
            Console.WriteLine("-----------------");
            Console.WriteLine();

            if (!ParseArgs(args))
            {
                ShowUsage();
                return;
            }

            NarrowPossibilities(0.01);

            // TODO - Check which key characters we know

            // If needed, check letters after/before other letters

            // NarrowPossibilities more, perhaps?

            SelectRandomKeys();

            string key = GetKey(KeyPossibilities);
            Console.WriteLine();
            Console.WriteLine("Message:");
            Console.WriteLine(Decoder(Message, key.ToCharArray().Select(c => (byte)c).ToArray()));
            Console.WriteLine();
            Console.WriteLine("Key:");
            Console.WriteLine(key);
        }

        private static string Decoder(byte[] msg, byte[] keyBytes)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < msg.Length; i++)
            {
                ret.Append((char)(msg[i] ^ keyBytes[i % keyBytes.Length]));
            }
            return ret.ToString();
        }

        private static string GetKey(List<BitArray> KeyPossibilities)
        {
            // TODO : Warn if there are multiple possibilities for a key
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < KeyPossibilities.Count; i++)
            {
                ret.Append((char)(byte)KeyPossibilities[i].IndexOf(true));
            }
            return ret.ToString();
        }

        // Set all to false except for one random true
        private static void SelectRandomKeys()
        {
            for(int i = 0; i < KeyPossibilities.Count; i++)
            {
                int count = KeyPossibilities[i].GetSetBits();
                if (count == 1) continue;

                Random r = new Random();
                int select = r.Next(count);

                for (int j = 0; j < KeyPossibilities[i].Count; j++)
                {
                    if (KeyPossibilities[i][j])
                    {
                        if (select == 0)
                        {
                            Log(2, "Key " + i + " (" + count + " possibilities) randomly set to " + (char)(byte)j);
                            continue;
                        }
                        else
                        {
                            KeyPossibilities[i][j] = false;
                        }
                        select--;
                    }
                }
            }
        }

        private static void NarrowPossibilities(double cutoff, bool allowNonVisibleMessageChars = false)
        {
            Parallel.For(0, KeyPossibilities.Count, i =>
            {
                Parallel.For(0, KeyPossibilities[i].Count, j =>
                {
                    if (KeyPossibilities[i][j])
                    {
                        double distance = GetDistanceFromAverageFrequency(j, CharsByKey[i], allowNonVisibleMessageChars);
                        if (distance > cutoff || distance < 0)
                        {
                            KeyPossibilities[i][j] = false;
                            Log(2, "Key " + i + " = " + (char)(byte)j + ", above cutoff (" + cutoff + ") - Distance = " + distance.ToString());
                        }
                        else
                        {
                            Log(2, "Key " + i + " = " + (char)(byte)j + ", below cutoff (" + cutoff + ") - Distance = " + distance.ToString());
                        }
                    }
                });
            });
        }

        private static double GetDistanceFromAverageFrequency(int key, byte[] chars, bool allowNonVisibleMessageChars)
        {
            int[] counts = new int[256];
            foreach (byte c in chars)
            {
                counts[c ^ ToLower((byte)key)]++;
            }
            double ret = 0;
            for (int i = 0; i < 256; i++)
            {
                // Shortcut out if we decode an invalid character
                if (!allowNonVisibleMessageChars && counts[i] > 0 && (i < 32 || i > 126))
                {
                    return -1;
                }

                double freq = ((double)counts[i]) / chars.Length;
                ret += Math.Pow(freq - FrequencyTables.EnglishCharFreq[i], 2);
            }

            ret = Math.Sqrt(ret / counts.Length);
            return ret;
        }

        private static byte ToLower(byte b)
        {
            if (65 <= b && b <= 90)
            {
                return (byte)(b + 32);
            }
            else
            {
                return b;
            }
        }

        private static bool ParseArgs(string[] args)
        {
            // Check args length
            if (args.Length != 2) return false;

            // Get key length
            int keyLength;
            if (!int.TryParse(args[0], out keyLength)) return false;
            if (keyLength < 1) return false;
            KeyPossibilities = new List<BitArray>();
            for (int i = 0; i < keyLength; i++)
            {
                KeyPossibilities.Add(GetNewKeyPossibility());
            }

            // Get message
            Message = args[1].Split(new char[] { ',', ' ', '-', ';', '\t', '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => byte.Parse(s)).ToArray();
            if (Message == null || Message.Length < 1) return false;

            // Get chars by their xor'd key
            CharsByKey = new List<byte[]>();
            for (int i = 0; i < keyLength; i++)
            {
                List<byte> bytes = new List<byte>();
                for (int j = i; j < Message.Length; j += keyLength)
                {
                    bytes.Add(Message[j]);
                }
                CharsByKey.Add(bytes.ToArray());
            }

            return true;
        }

        private static BitArray GetNewKeyPossibility()
        {
            BitArray ret = new BitArray(256);

            // As per generator.cs, all key characters fall in the ASCII visible range [33, 126]
            for (int i = 33; i < 127; i++)
            {
                ret[i] = true;
            }
            return ret;
        }

        private static void ShowUsage()
        {
            //                 ---------1---------2---------3---------4---------5---------6---------7---------
            Console.WriteLine("Usage:");
            Console.WriteLine("   XOR_Decrypter.exe <key length> <encrypted bytes>");
            Console.WriteLine("   Note that the bytes should be comma or space delimited.");
            Console.WriteLine();
        }

        private static void Log(int level, string msg)
        {
            // TODO : Check level against desired verbosity
            Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.f] ") + msg);
        }
    }
}
