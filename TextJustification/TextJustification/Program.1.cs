using System;
class P
{
    static void Main(string[] a)
    {
        int x,y,d,i,j,l,m=0;
        for(i=0;i<a.Length;i++)m=Math.Max(a[i].Length,m);
        for(i=0;i<a.Length;i++)
        {
            var s = a[i].Split();
            var o = "";
            if (s.Length <= 1) o = a[i];
            else
            {
                l = 0;
                foreach (var b in s) l += b.Length;
                d = m - l;
                x = d / (s.Length - 1);
                y = d % (s.Length - 1);
                for (j = 0; j < s.Length; j++)
                {
                    o += s[j] + new string(' ', x);
                    if (j <= y) o += " ";
                }
            }
            Console.WriteLine(o);
        }
    }
}
