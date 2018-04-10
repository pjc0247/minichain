using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class Transaction
    {
        private static Random rd = new Random();

        public string senderAddr, receiverAddr;

        public string hash;
        public double fee;

        public double _in, _out;

        public static bool IsValidTransaction(Transaction tx)
        {
            return true;
        }
        public static Transaction CreateRewardTransaction(int blockNo, string minerAddr)
        {
            return new Transaction()
            {
                senderAddr = "0",
                receiverAddr = minerAddr,
                fee = 0,
                _out = Consensus.CalcBlockReward(blockNo)
            };
        }

        public Transaction()
        {
            hash = rd.Next(0, 100).ToString();
        }
    }
}
