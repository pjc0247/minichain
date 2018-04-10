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
            txs.Add(Transaction.CreateRewardTransaction(blockNo, wallet.addr));
            txs.AddRange(txPool.GetTransactionsWithHighestFee(1024));

            return txs.ToArray();
        }
        private void MinerHQ()
        {
            while (isAlive)
            {
                var workingBlockNo = currentBlock.blockNo;
                var txs = PrepareBlockTransactions(workingBlockNo + 1);
                var vblock = new Block(wallet.addr, currentBlock, txs, "");

                var startTime = DateTime.Now;
                PrepareWorkers(vblock, 8);

                if (ev.WaitOne())
                {
                    if (currentBlock.blockNo != workingBlockNo)
                        continue;

                    currentBlock = new Block(wallet.addr, currentBlock, txs, solution);
                    DiscoverBlock(currentBlock);
                    Console.WriteLine(
                        $"   * FindBlock#{currentBlock.blockNo}, elapsed {(DateTime.Now - startTime).TotalSeconds} sec(s)\r\n" +
                        $"        nonce: {solution} \r\n" +
                        $"        prevBlock: {currentBlock.prevBlockHash}");
                }
            }
        }

        private void OnNewBlockDiscovered(Block block)
        {
            ev.Set();
        }

        private void PrepareWorkers(Block vblock, int nThread)
        {
            Console.WriteLine("Preparing job, block#" + vblock.blockNo + " with " + nThread + " thread(s).");

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

            while(vblock.blockNo == currentBlock.blockNo + 1)
            {
                var nonce = Solver.FindSolution(vblock, start, limit);

                if (Block.IsValidBlockLight(vblock, nonce))
                {
                    solution = nonce;
                    ev.Set();
                }

                start += limit;
            }
        }
    }
}
