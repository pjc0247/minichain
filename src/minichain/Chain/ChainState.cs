using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class ChainState
    {
        public Block currentBlock { get; private set; }
        private object blockLock = new object();

        private FileDB fdb;
        private StateDB sdb;

        public ChainState()
        {
            fdb = new FileDB();
            sdb = new StateDB(fdb);

            // Every node starts with genesisBlock.
            //   This will be overwritten if there is any other live nodes.
            PushBlock(Block.GenesisBlock());
        }

        public Transaction GetTransaction(string transactionHash)
        {
            return fdb.Read<Transaction>($"tx/{transactionHash}");
        }
        public Block GetBlock(string blockHash)
        {
            return fdb.Read<Block>($"block/{blockHash}");
        }

        public double GetBalanceInBlock(string address, string blockHash)
        {
            return sdb.GetState(blockHash, address).balance;
        }
        public double GetBalance(string address)
        {
            return GetBalanceInBlock(address, currentBlock.hash);
        }   

        internal bool PushBlock(Block block)
        {
            if (block == null) 
                throw new ArgumentNullException(nameof(block));
            if (currentBlock == null)
                currentBlock = block;

            try
            {
                lock (blockLock)
                {
                    // In case that new block is came from another branch:
                    if (currentBlock != null &&
                        block.prevBlockHash != currentBlock.hash)
                        ;

                    fdb.Write($"block/{block.hash}", block);

                    ApplyTransactions(block);

                    // CONFIRMED
                    currentBlock = block;

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        /// <summary>
        /// Backtracks the branched block.
        ///             
        /// ex) TrackBranchedBlock("block13", "block12") 
        ///       => "block10"
        ///       
        ///             13
        ///             12    12
        ///              11  11
        ///                10
        ///                 9
        ///                 8
        /// </summary>
        private Block TrackBranchedBlock(Block a, Block b)
        {
            var history = new HashSet<string>();

            for (int i = 0;i <Consensus.TrustedConfirmations; i++)
            {
                if (a.hash == b.hash) return a;

                a = GetBlock(a.prevBlockHash);
                b = GetBlock(b.prevBlockHash);

                if (history.Add(a.hash) == false) return a;
                if (history.Add(b.hash) == false) return b;
            }

            // 어디서 혼자 놀고있다가 갑자기 존나 긴 체인 들고온 경우 무시
            throw new InvalidOperationException();
        }
        private void ApplyTransactions(Block newBlock)
        {
            var txs = newBlock.txs;
            var changes = new HashSet<WalletState>();

            foreach (var tx in txs)
            {
                if (tx.senderAddr != Consensus.RewardSenderAddress)
                {
                    var senderWallet = changes.FirstOrDefault(x => x.address == tx.senderAddr);
                    if (senderWallet == null)
                        senderWallet = sdb.GetState(currentBlock.hash, tx.senderAddr);

                    if (senderWallet.balance != tx._in)
                        throw new InvalidOperationException();

                    // Actual OUT is (_out + fee)
                    senderWallet.balance -= tx._out + tx.fee;
                    changes.Add(senderWallet);
                }

                var receiverWallet = changes.FirstOrDefault(x => x.address == tx.receiverAddr);
                if (receiverWallet == null)
                    receiverWallet = sdb.GetState(currentBlock.hash, tx.receiverAddr);

                receiverWallet.balance += tx._out;
                changes.Add(receiverWallet);
            }

            sdb.PushState(currentBlock.hash, newBlock.hash, changes.ToArray());
        }
    }
}
    