using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class TransactionHeader
    {
        public string hash;
        public string publicKey;
        /// RSA encrypted sign to validate this transaction.
        public string sign;

        public string senderAddr, receiverAddr;

        public double fee;
        public double _in, _out;

        public bool isSigned =>
            !string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(sign);
    }

    public class Transaction : TransactionHeader
    {
        public static Transaction EmptyTransaction()
        {
            return new Transaction() { hash = "0000000000000000" };
        }
        public static bool IsValidTransaction(Transaction tx)
        {
            if (tx._out == 0) return false;
            if (tx.isSigned == false) return false;

            if (Hash.Calc(tx.publicKey) != tx.senderAddr)
                return false;
            if (RSA.VerifyWithPrivateKey(tx.publicKey, tx.GetTransactionSigniture(), tx.sign) == false)
                return false;

            return true;
        }
        public static bool IsValidTransactionDeep(Transaction tx, 
            BlockHeader blockHeader, string[] merkleRoute)
        {
            if (IsValidTransaction(tx) == false) return false;

            var accHash = tx.hash;
            foreach (var hash in merkleRoute)
                accHash = Hash.Calc2(accHash, hash);

            return accHash == blockHeader.merkleRootHash;
        }

        public static Transaction CreateRewardTransaction(int blockNo, string minerAddr, Transaction[] txs)
        {
            var totalFee = txs.Sum(x => x.fee);

            return new Transaction()
            {
                senderAddr = Consensus.RewardSenderAddress,
                receiverAddr = minerAddr,
                fee = 0,
                _out = Consensus.CalcBlockReward(blockNo) + totalFee
            };
        }

        public Transaction()
        {
            hash = UniqID.Generate();
        }

        public string GetTransactionSigniture()
        {
            return Hash.Calc(senderAddr + receiverAddr + _in + _out + fee);
        }
        public void Sign(string _privateKey, string _publicKey)
        {
            var original = GetTransactionSigniture();

            sign = RSA.SignWithPrivateKey(_privateKey, original);
            publicKey = _publicKey;
        }
    }
}
