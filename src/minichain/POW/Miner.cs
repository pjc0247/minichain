using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class Miner : EndpointNode
    {
        private List<Thread> workers = new List<Thread>();

        private string solution;
        private AutoResetEvent ev = new AutoResetEvent(false);

        private Thread miningThread;
        private long hashCounter = 0;

        public void Start()
        {
            onNewBlockDiscovered += OnNewBlockDiscovered;

            isAlive = true;

            miningThread = new Thread(MinerHQ);
            miningThread.Start();
        }
        public override void Stop()
        {
            base.Stop();

            ev.Set();
            miningThread.Join();
        }

        private Transaction[] PrepareBlockTransactions(int blockNo)
        {
            var txs = new List<Transaction>();

            // First transaction is block reward
            txs.Add(Transaction.CreateRewardTransaction(blockNo, wallet.address));
            txs.AddRange(txPool.GetTransactionsWithHighestFee(1024));

            return txs.ToArray();
        }

        private void MinerHQ()
        {
            while (isAlive)
            {
                var workingBlockNo = chain.currentBlock.blockNo;
                var txs = PrepareBlockTransactions(workingBlockNo + 1);
                var vblock = new Block(wallet.address, chain.currentBlock, txs, "");

                var startTime = DateTime.Now;
                PrepareWorkers(vblock, 8);

                if (ev.WaitOne())
                {
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    Console.WriteLine(
                        $"   * HASHRATE: {(int)(Interlocked.Read(ref hashCounter) / elapsed)}/s");

                    if (chain.currentBlock.blockNo != workingBlockNo)
                    {
                        txPool.AddTransactions(txs.Skip(1).ToArray());
                        continue;
                    }

                    chain.PushBlock(new Block(wallet.address, chain.currentBlock, txs, solution));

                    DiscoverBlock(chain.currentBlock);
                    Console.WriteLine(
                        $"   * FindBlock#{chain.currentBlock.blockNo}, elapsed {elapsed} sec(s)\r\n" +
                        $"        nonce: {solution} \r\n" +
                        $"        prevBlock: {chain.currentBlock.prevBlockHash} \r\n" + 
                        $"        txs: {chain.currentBlock.txs.Length}");
                }
            }
        }

        private void OnNewBlockDiscovered(Block block)
        {
            ev.Set();
        }

        private void PrepareWorkers(Block vblock, int nThread)
        {
            Console.WriteLine(
                "------------------------------------------------------------\r\n" +
                "Preparing job, block#" + vblock.blockNo + " with " + nThread + " thread(s).");

            Interlocked.Exchange(ref hashCounter, 0);
            for (int i = 0; i < nThread; i++)
            {
                var t = new Thread(() =>
                {
                    Worker(vblock, 100000000 * i);
                });
                t.Start();
            }
        }
        private void Worker(Block vblock, int start)
        {
            var limit = 10000;

            while(vblock.blockNo == chain.currentBlock.blockNo + 1)
            {
                var nonce = Solver.FindSolution(vblock, start, limit);
                // DoubleCheck
                if (vblock.blockNo != chain.currentBlock.blockNo + 1)
                    return;

                Interlocked.Add(ref hashCounter, limit);
                if (Block.IsValidNonce(vblock, nonce))
                {
                    solution = nonce;
                    ev.Set();
                }

                start += limit;
            }
        }
    }
}
