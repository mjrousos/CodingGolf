using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cryptography;

namespace XOR_Decrypter
{
    class Program
    {
        static List<BitArray> KeyPossibilities;
        static List<byte[]> CharsByKey;
        static byte[] Message;
        static int Verbosity = 0;

        static void Main(string[] args)
        {
            try
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

                // Cut any obviously incorrect keys based on a coarse cut
                RemoveLowFreqMatches(0.01, FrequencyTables.EnglishGutenbergChraFreq);
                // Further narrow based on sequences
                RemoveLowFrequencySequences(0.06, FrequencyTables.EnglishGutenbergNextCharFreq, FrequencyTables.EnglishGutenbergPrevCharFreq);

                // Cut with a finer grain
                RemoveLowFreqMatches(0.006, FrequencyTables.EnglishGutenbergChraFreq);
                RemoveLowFrequencySequences(0.04, FrequencyTables.EnglishGutenbergNextCharFreq, FrequencyTables.EnglishGutenbergPrevCharFreq);

                // Make our best guess
                RemoveLowFreqMatches(0.000, FrequencyTables.EnglishGutenbergChraFreq);

                string key = GetKey(KeyPossibilities);
                Console.WriteLine();
                Console.WriteLine("Message:");
                Console.WriteLine(Decode(Message, key.ToCharArray().Select(c => (byte)c).ToArray()));
                Console.WriteLine();
                Console.WriteLine("Key:");
                Console.WriteLine(key);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType() + ": " + exc.Message);
            }
        }

        private static string Decode(byte[] msg, byte[] keyBytes)
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
                ret.Append((char)(byte)KeyPossibilities[i].LastIndexOf(true));
            }
            return ret.ToString();
        }

#if FALSE
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
#endif // FALSE

        private static void RemoveLowFrequencySequences(double resolution, double[][] nextFreqTable, double[][] prevFreqTable)
        {
            for (int i = 0; i < KeyPossibilities.Count; i++)
            {
                if (KeyPossibilities[i].GetSetBits() > 1)
                {
                    int prevIndex = i - 1;
                    if (prevIndex < 0) prevIndex = KeyPossibilities.Count - 1;
                    if (nextFreqTable != null && KeyPossibilities[prevIndex].GetSetBits() == 1)
                    {
                        RemoveLowFreqNextMatches(prevIndex, i, resolution, FrequencyTables.EnglishGutenbergNextCharFreq);
                    }
                    // Perhaps we've already found a key and don't need to check prev
                    if (KeyPossibilities[i].GetSetBits() == 1) continue;

                    int nextIndex = i + 1;
                    if (nextIndex == KeyPossibilities.Count) nextIndex = 0;
                    if (prevFreqTable != null && (i == KeyPossibilities.Count - 1 || KeyPossibilities[i + 1].GetSetBits() == 1))
                    {
                        RemoveLowFreqPrevMatches(i, nextIndex, resolution, FrequencyTables.EnglishGutenbergPrevCharFreq);
                    }
                }
            }
        }

        private static void RemoveLowFreqNextMatches(int prevIndex, int nextIndex, double resolution, double[][] freqTable)
        {
            // Assert prevIndex has 1 bit set and nextIndex has > 1 bit set
            byte prevKey = (byte)KeyPossibilities[prevIndex].IndexOf(true);
            ConcurrentDictionary<int, double> likelihoods = new ConcurrentDictionary<int, double>();
            Parallel.For(0, KeyPossibilities[nextIndex].Count, i =>
            {
                if (KeyPossibilities[nextIndex][i])
                {
                    double likelihood = 0;
                    int lCount = 0;
                    for (int j = nextIndex; j < Message.Length; j += KeyPossibilities.Count)
                    {
                        byte prev = ToLower(j == 0 ? (byte)' ' : (byte)(prevKey ^ Message[j - 1]));
                        if (freqTable[prev] != null)
                        {
                            likelihood += freqTable[prev][i ^ ToLower(Message[j])];
                            lCount++;
                        }
                    }
                    likelihood /= lCount;
                    likelihoods.AddOrUpdate(i, likelihood, (a, b) => { throw new InvalidOperationException("Unexpected collission");});
                }
            });

            double cutoff = likelihoods.Values.Max() - resolution;
            foreach (var kvp in likelihoods.Where(k => k.Value < cutoff))
            {
                KeyPossibilities[nextIndex][kvp.Key] = false;
            }
        }

        private static void RemoveLowFreqPrevMatches(int prevIndex, int nextIndex, double resolution, double[][] freqTable)
        {
            // Assert nextIndex has 1 bit set and prevIndex has > 1 bit set
            byte nextKey = (byte)KeyPossibilities[nextIndex].IndexOf(true);
            ConcurrentDictionary<int, double> likelihoods = new ConcurrentDictionary<int, double>();
            Parallel.For(0, KeyPossibilities[prevIndex].Count, i =>
            {
                if (KeyPossibilities[prevIndex][i])
                {
                    double likelihood = 0;
                    int lCount = 0;
                    for (int j = prevIndex; j < Message.Length; j += KeyPossibilities.Count)
                    {
                        byte next = ToLower(j == Message.Length - 1 ? (byte)' ' : (byte)(nextKey ^ Message[j + 1]));
                        if (freqTable[next] != null)
                        {
                            likelihood += freqTable[next][i ^ ToLower(Message[j])];
                            lCount++;
                        }
                    }
                    likelihood /= lCount;
                    likelihoods.AddOrUpdate(i, likelihood, (a, b) => { throw new InvalidOperationException("Unexpected collission"); });
                }
            });

            double cutoff = likelihoods.Values.Max() - resolution;
            foreach (var kvp in likelihoods.Where(k => k.Value < cutoff))
            {
                KeyPossibilities[prevIndex][kvp.Key] = false;
            }
        }

        // Remove possibilities with a distance more than 'resolution' away from the most likely posibility
        private static void RemoveLowFreqMatches(double resolution, double[] freqTable)
        {
            Parallel.For(0, KeyPossibilities.Count, i =>
            {
                RemoveLowFreqMatches(i, resolution, freqTable);
            });
        }

        private static void RemoveLowFreqMatches(int i, double resolution, double[] freqTable)
        {
            if (KeyPossibilities[i].GetSetBits() > 1)
            {
                ConcurrentDictionary<int, double> distances = new ConcurrentDictionary<int, double>();
                Parallel.For(0, KeyPossibilities[i].Count, j =>
                {
                    if (KeyPossibilities[i][j])
                    {
                        double distance = GetDistanceFromAverageFrequency(j, CharsByKey[i], true, freqTable);
                        if (distance < 0)
                        {
                            KeyPossibilities[i][j] = false;
                            Log(2, "Key " + i + " = " + (char)(byte)j + ", removed because it decodes invalid characters ");
                        }
                        else
                        {
                            distances.AddOrUpdate(j, distance, (a, b) => { throw new InvalidOperationException("Unexpected collission"); });
                        }
                    }
                });
                double cutoff = distances.Values.Min() + resolution;
                foreach (var kvp in distances.Where(k => k.Value > cutoff))
                {
                    KeyPossibilities[i][kvp.Key] = false;
                }
            }
        }

#if FALSE
                 private static void NarrowPossibilities(double cutoff, bool allowNonVisibleMessageChars = false)
                {
                    Parallel.For(0, KeyPossibilities.Count, i =>
                    {
                        NarrowPossibility(i, cutoff, allowNonVisibleMessageChars);
                    });
                }

                private static void NarrowPossibility(int i, double cutoff, bool allowNonVisibleMessageChars)
                {
                    if (KeyPossibilities[i].GetSetBits() > 1)
                    {
                        ConcurrentBag<int> indecesRemoved = new ConcurrentBag<int>();
                        Parallel.For(0, KeyPossibilities[i].Count, j =>
                        {
                            if (KeyPossibilities[i][j])
                            {
                                double distance = GetDistanceFromAverageFrequency(j, CharsByKey[i], allowNonVisibleMessageChars);
                                if (distance > cutoff || distance < 0)
                                {
                                    KeyPossibilities[i][j] = false;
                                    indecesRemoved.Add(j);
                                    Log(2, "Key " + i + " = " + (char)(byte)j + ", above cutoff (" + cutoff + ") - Distance = " + distance.ToString());
                                }
                                else
                                {
                                    Log(2, "Key " + i + " = " + (char)(byte)j + ", below cutoff (" + cutoff + ") - Distance = " + distance.ToString());
                                }
                            }
                        });
                        // If we removed all possibilities, then roll back
                        if (KeyPossibilities[i].GetSetBits() == 0)
                        {
                            foreach (int j in indecesRemoved)
                            {
                                KeyPossibilities[i][j] = true;
                            }
                        }
                    }
                }

                // Experimental
                private static void NarrowPossibilitiesExp(bool allowNonVisisbleMessageChars)
                {
                    Parallel.For(0, KeyPossibilities.Count, i =>
                    {
                        double cutoff = 0.05;
                        while (KeyPossibilities[i].GetSetBits() > 1 && cutoff > 0)
                        {
                            NarrowPossibility(i, cutoff, allowNonVisisbleMessageChars);
                            cutoff -= 0.002;
                        }
                    });
                }

        private static string Decode(byte[] bytes, byte key)
        {
            return new string(bytes.Select(b => (char)(b ^ key)).ToArray());
        }
#endif // FALSE

        private static double GetDistanceFromAverageFrequency(int key, byte[] chars, bool allowNonVisibleMessageChars, double[] freqTable)
        {
            int[] counts = new int[256];
            foreach (byte c in chars)
            {
                counts[ToLower((byte)(c ^ (byte)key))]++;
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
                ret += Math.Pow(freq - freqTable[i], 2);
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
            if (Verbosity >= level)
            {
                Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.f] ") + msg);
            }
        }
    }
}
