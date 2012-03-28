using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace OEA.Utils
{
    public static class StringHelper
    {
        public static string MD5(string str)
        {
            if (String.IsNullOrEmpty(str)) return str;

            MD5 m = new MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            return BitConverter.ToString(s);
        }
    }
}
