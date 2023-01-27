using System.Collections;
using System.Collections.Generic;

public static class StringParser 
{

    public static int ToInt(string input)
    {
        int res = 0; int.TryParse(input, out res);

        return res;
    }

    public static float ToFloat(string input)
    {
        float res = 0; float.TryParse(input, out res);

        return res;
    }

    public static bool ToBool(string input)
    {
        bool res = false; bool.TryParse(input, out res);

        return res;
    }
    public static byte ToByte(string input)
    {
        byte res = 0; byte.TryParse(input, out res);

        return res;
    }

}
