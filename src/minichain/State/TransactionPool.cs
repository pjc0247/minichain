using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
            lock (pendingTxs)
            {
                pendingTxs.Add(tx.fee, tx);
            }
        }
        public void AddTransactions(Transaction[] txs)
        {
            lock (pendingTxs)
            {
                foreach (var tx in txs)
                    pendingTxs.Add(tx.fee, tx);
            }
        }
        public Transaction[] GetTransactionsWithHighestFee(int n)
        {
            lock (pendingTxs)
            {
                var txs = pendingTxs
                    .Take(n).Select(x => x.Value).ToArray();

                for (int i = 0; i < txs.Length; i++)
                    pendingTxs.RemoveAt(0);

                return txs;
            }
        }
    }
}
