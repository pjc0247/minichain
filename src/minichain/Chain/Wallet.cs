using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class Wallet
    {
        public string addr;

        public Wallet()
        {
            addr = Hash.Calc(Guid.NewGuid().ToString());
        }

        public Transaction CreateTransaction(string receiver, double amount)
        {
            return new Transaction();
        }
    }
}
