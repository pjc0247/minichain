using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class TransactionPool
    {
        private SortedList<double, Transaction> pendingTxs = new SortedList<double, Transaction>();

        public TransactionPool()
        {
        }

        public void AddTransaction(Transaction tx)
        {
            pendingTxs.Add(tx.fee, tx);
        }
        public Transaction[] GetTransactionsWithHighestFee(int n)
        {
            return pendingTxs
                .OrderByDescending(x => x.Value.fee)
                .Take(n).Select(x => x.Value).ToArray();
        }
    }
}
