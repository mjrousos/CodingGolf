using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ExtensionMethods
{
    public static int GetSetBits(this BitArray b)
    {
        int ret = 0;
        for (int i = 0; i < b.Count; i++) if (b[i]) ret++;
        return ret;
    }

    public static int IndexOf(this BitArray a, bool b)
    {
        for (int i = 0; i < a.Count; i++) if (a[i] == b) return i;
        return -1;
    }
}
