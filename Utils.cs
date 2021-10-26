using System;

namespace doturn
{
    class Utils
    {
        public static byte[] stringToByteArray(string input)
        {
            var res = new byte[input.Length / 2];
            int j = 0;
            for (int i = 0; i < input.Length; i += 2)
            {
                string w = input.Substring(i, 2);
                res[j] = Convert.ToByte(w, 16);
                j++;
            }
            return res;
        }
    }
}