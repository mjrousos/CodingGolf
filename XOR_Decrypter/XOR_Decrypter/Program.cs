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
            Console.WriteLine("- v1.0; MikeRou -");
            Console.WriteLine("-----------------");
            Console.WriteLine();

            if (!ParseArgs(args))
            {
                ShowUsage();
                return;
            }

            NarrowPossibilities(0.5);
        }

        private static void NarrowPossibilities(double cutoff, bool allowNonVisibleMessageChars = false)
        {
            for (int i = 0; i < KeyPossibilities.Count; i++)
            {
                for (int j = 0; j < KeyPossibilities[i].Count; j++)
                {
                    if (KeyPossibilities[i][j])
                    {
                        double distance = GetDistanceFromAverageFrequency(j, CharsByKey[i], allowNonVisibleMessageChars);
                        if (distance < cutoff)
                        {
                            KeyPossibilities[i][j] = false;
                        }
                    }
                }
            }
        }

        private static double GetDistanceFromAverageFrequency(int key, byte[] chars, bool allowNonVisibleMessageChars)
        {
            throw new NotImplementedException();
        }

        private static bool ParseArgs(string[] args)
        {
            // Check args length
            if (args.Length != 2) return false;

            // Get key length
            int keyLength;
            if (int.TryParse(args[0], out keyLength)) return false;
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
                for (int j = i; j < Message.Length; j+= keyLength)
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
    }
}
