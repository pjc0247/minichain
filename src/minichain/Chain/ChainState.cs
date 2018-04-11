using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class ChainState
    {
        // Every node starts with genesisBlock.
        //   This will be overwritten if there is any other live nodes.
        public Block currentBlock { get; private set; }

        private FileDB fdb;
        private StateDB sdb;

        public ChainState()
        {
            fdb = new FileDB();
            sdb = new StateDB(fdb);

            PushBlock(Block.GenesisBlock());
        }

        public double GetBalanceInBlock(string address, string blockHash)
        {
            return sdb.GetState(blockHash, address).balance;
        }
        public double GetBalance(string address)
        {
            return GetBalanceInBlock(address, currentBlock.hash);
        }

        internal void PushBlock(Block block)
        {
            currentBlock = block;
            fdb.Write($"block/{block.hash}", block);

            ApplyTransactions(block.txs);
        }
        private void ApplyTransactions(Transaction[] txs)
        {
            var changes = new HashSet<WalletState>();

            foreach (var tx in txs)
            {
                if (tx.senderAddr != Consensus.RewardSenderAddress)
                {
                    var senderWallet = changes.FirstOrDefault(x => x.address == tx.senderAddr);
                    if (senderWallet == null)
                        senderWallet = sdb.GetState(currentBlock.prevBlockHash, tx.senderAddr);

                    // Actual OUT is (_out + fee)
                    senderWallet.balance -= tx._out - tx.fee;
                    changes.Add(senderWallet);
                }

                var receiverWallet = changes.FirstOrDefault(x => x.address == tx.receiverAddr);
                if (receiverWallet == null)
                    receiverWallet = sdb.GetState(currentBlock.prevBlockHash, tx.receiverAddr);

                receiverWallet.balance += tx._out;
                changes.Add(receiverWallet);
            }

            sdb.PushState(currentBlock.prevBlockHash, currentBlock.hash, changes.ToArray());
        }
    }
}
