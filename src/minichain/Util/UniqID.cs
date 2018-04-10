using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    class UniqID
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
