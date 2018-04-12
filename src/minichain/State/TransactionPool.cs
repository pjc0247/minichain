using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    /// <summary>
    /// Management pending transactions
    /// </summary>
    public class TransactionPool
    {
        private CappedList<Transaction> pendingTxs = new CappedList<Transaction>(1024);

        public TransactionPool()
        {
        }

        public void AddTransaction(Transaction tx)
        {
            lock (pendingTxs)
            {
                pendingTxs.Add(tx);
            }
        }
        public void AddTransactions(Transaction[] txs)
        {
            lock (pendingTxs)
            {
                foreach (var tx in txs)
                    pendingTxs.Add(tx);
            }
        }
        public void RemoveTransactions(Transaction[] txs)
        {
            lock (pendingTxs)
            {
                foreach (var tx in txs)
                    pendingTxs.Remove(tx);
            }
        }
        public Transaction[] GetTransactionsWithHighestFee(int n)
        {
            lock (pendingTxs)
            {
                var txs = pendingTxs
                    .OrderByDescending(x => x.fee)
                    .Take(n).Select(x => x).ToArray();

                for (int i = 0; i < txs.Length; i++)
                    pendingTxs.RemoveAt(0);

                return txs;
            }
        }
    }
}
