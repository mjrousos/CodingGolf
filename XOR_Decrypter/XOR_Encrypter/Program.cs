using System;
using System.Collections.Generic;

public class Generator
{
    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Generator.exe PasswordLength Sentence [ToLower]");
            return -1;
        }

        string password = GeneratePassword(Convert.ToInt32(args[0]));
        string text = args[1];
        byte[] key = new byte[password.Length];

        if (args.Length >= 3)
        {
            text = text.ToLower();
        }

        byte[] output = new byte[text.Length];

        // generate key
        for (int i = 0; i < key.Length; i++) key[i] = (byte)password[i];

        // generate the output
        for (int i = 0; i < output.Length; i++)
        {
            char letter = text[i];
            output[i] = (byte)((byte)letter ^ key[i % key.Length]);
        }

        Console.WriteLine("{0}\t{1}\t", password, password.Length);

        // output the encoded bytes
        for (int i = 0; i < output.Length; i++)
        {
            Console.Write("{0}", output[i]);
            if (i < output.Length - 1) Console.Write(",");
        }
        Console.WriteLine("");

        return 100;
    }

    private static string GeneratePassword(int length)
    {
        // generate 'length' random characters within the visible range of ascii (33 - 126)
        Random rand = new Random();
        string password = "";

        while (length-- > 0)
        {
            password += (char)((rand.Next() % 93) + 33);
        }

        return password;
    }
}