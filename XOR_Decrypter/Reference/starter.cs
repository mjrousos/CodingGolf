using System;

public class Starter
{
	public static int Main(string[] args)
	{
		int passwordLength = Convert.ToInt32(args[0]);
		byte[] encoded = Array.ConvertAll<string,byte>(args[1].Split(new char[] {','}), b => Convert.ToByte(b));
		string key = "";

		Console.WriteLine(key);

		return 100;
	}
}