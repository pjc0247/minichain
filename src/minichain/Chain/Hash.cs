﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace minichain
{
    class Hash
    {
        public static string Calc(string a)
        {
            var sha = SHA1.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(a));

            return string.Join("", hash.Select(x => x.ToString("x2")).ToArray());
        }
    }
}
