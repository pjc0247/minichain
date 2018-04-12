using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace minichain
{
    public class WalletState
    {
        public string address { get; set; }
        public double balance { get; set; }
    }
    public class WalletParameter
    {
        public string address;
        public string publicKey;
        public string privateKey;
    }

    public class Wallet : WalletState
    {
        private string publicKey;
        private string privateKey;

        private ChainState chain;

        public Wallet(ChainState _chain)
        {
            RSA.GenerateKeyPair(out publicKey, out privateKey);

            chain = _chain;
            address = Hash.Calc(publicKey);
        }

        /// <summary>
        /// Imports wallet from json string
        /// </summary>
        public void Import(string json)
        {
            var p = JsonConvert.DeserializeObject<WalletParameter>(json);

            address = p.address;
            privateKey = p.privateKey;
            publicKey = p.publicKey;
        }
        /// <summary>
        /// Exports current wallet to json string
        /// </summary>
        public string Export()
        {
            var p = new WalletParameter()
            {
                address = address,
                privateKey = privateKey,
                publicKey = publicKey
            };

            return JsonConvert.SerializeObject(p, Formatting.Indented);
        }

        public double GetBalanceInBlock(string blockHash)
        {
            return chain.GetBalanceInBlock(address, blockHash);
        }
        public double GetBalance()
        {
            return chain.GetBalance(address);
        }

        /// <summary>
        /// Creates a transaction signed by this wallet
        /// </summary>
        public Transaction CreateSignedTransaction(string receiverAddr, double amount, double fee = 0)
        {
            var tx = new Transaction()
            {
                _in = chain.GetBalance(address),
                _out = amount,

                senderAddr = address,
                receiverAddr = receiverAddr,

                fee = fee
            };
            Sign(tx);
            return tx;
        }
        /// <summary>
        /// Signs a single transaction
        /// </summary>
        public void Sign(Transaction tx)
        {
            tx.Sign(privateKey, publicKey);
        }
    }
}
