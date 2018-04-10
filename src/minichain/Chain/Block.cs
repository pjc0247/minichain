using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class BlockHeader
    {
        /// Block No#
        public int blockNo { get; protected set; }

        /// Address of block miner
        public string minerAddr { get; protected set; }

        /// This block hash
        public string hash { get; protected set; }
        /// Prev block hash
        public string prevBlockHash { get; protected set; }
        /// Merkle root hash
        public string merkleRootHash { get; protected set; }

        public string nonce { get; protected set; }
        public int difficulty { get; protected set; }
    }
    public class BlockBody : BlockHeader
    {
        public Transaction[] txs;
    }

    public class Block : BlockBody
    {
        public static Block GenesisBlock()
        {
            return new Block("0", null, new Transaction[] { }, "");
        }
        public static string GetBlockHash(string prevBlockHash, string rootHash, string nonce)
        {
            return Hash.Calc(prevBlockHash + rootHash + nonce);
        }

        /// <summary>
        /// Lightweight validations with block header.
        /// </summary>
        public static bool IsValidBlockLight(Block block, string nonce)
        {
            // Genesis-block is always right;
            if (block.blockNo == 0) return true;

            // 1. txs MUST be non-empty (except genesis-block)
            if (block.txs.Length == 0) return false;
            // 2. Check the reward transaction. (txs[0])
            if (block.txs[0]._out != Consensus.CalcBlockReward(block.blockNo) ||
                block.txs[0].receiverAddr == block.minerAddr) return false;
            // 3. Has valid minerAddress
            if (string.IsNullOrEmpty(block.minerAddr)) return false;
            // 4. Has proper difficulty
            if (Consensus.CalcBlockDifficulty(block.blockNo) != block.difficulty) return false;

            // 5. Check the nonce with block difficulty
            var hash = GetBlockHash(block.prevBlockHash, block.merkleRootHash, nonce);
            for (int i = 0; i < block.difficulty; i++)
            {
                if (hash[i] != '0')
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Fully validate the given block.
        /// </summary>
        public static bool IsValidBlockDeep(Block block, string nonce)
        {
            if (IsValidBlockLight(block, nonce) == false)
                return false;

            // 1. Check every transactions are valid
            if (block.txs.Any(x => Transaction.IsValidTransaction(x) == false))
                return false;

            // 2. Check merkleRootHash is valid
            var merkleTree = new MerkleTree(block.txs);
            if (block.merkleRootHash != merkleTree.GetRootHash())
                return false;

            return true;
        }

        public Block()
        {
        }
        public Block(string _minerAddr, Block prev, Transaction[] txs, string _nonce)
        {
            var merkleTree = new MerkleTree(txs);

            minerAddr = _minerAddr;
            merkleRootHash = merkleTree.GetRootHash();
            nonce = _nonce;

            // GENESIS-BLOCK
            if (prev == null)
                prevBlockHash = "";
            else
            {
                prevBlockHash = prev.hash;
                blockNo = prev.blockNo + 1;
            }

            difficulty = Consensus.CalcBlockDifficulty(blockNo);
            hash = GetBlockHash(prevBlockHash, merkleRootHash, nonce);
        }
    }
}
