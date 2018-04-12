using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{

    /// Inefficient, but human-readable state tree database
    /// 
    /// Why we should use state-tree based DB
    ///    * Saves only the changes on the disk.
    ///    * Can easily revert state to specific point.
    ///    * Can read state from specific point.
    /// 
    /// There are 3 components in StateDB
    ///    * Header
    ///    * Indexed path
    ///    * State
    ///  
    /// see more details:
    /// https://github.com/pjc0247/minichain_state_db
    public class StateDB
    {
        class DataHeader
        {
            public Dictionary<string, string> path;

            public static DataHeader EmptyState()
            {
                return new DataHeader()
                {
                    path = new Dictionary<string, string>()
                };
            }
        }

        private FileDB fdb;

        public StateDB()
        {
            fdb = new FileDB();
        }
        public StateDB(FileDB _fdb)
        {
            fdb = _fdb;
        }

        private DataHeader ReadHeader(string stateRoot)
        {
            var header = fdb.Read<DataHeader>($"root/{stateRoot}");
            if (header == null) return DataHeader.EmptyState();
            return header;
        }
        private string WriteHeader(string stateRoot, DataHeader header)
        {
            fdb.Write($"root/{stateRoot}", header);
            return stateRoot;
        }

        private WalletState[] ReadStateBlob(string index, string uid)
        {
            return fdb.Read<WalletState[]>($"addr/{index}/{uid}");
        }
        private string WriteStateBlob(string index, WalletState[] wallets)
        {
            var uid = UniqID.Generate();
            fdb.Write($"addr/{index}/{uid}", wallets);
            return uid;
        }

        private string GetIndexFromAddress(string address)
        {
            return address.Substring(0, 2);
        }
        public WalletState GetState(string stateRoot, string address)
        {
            var header = ReadHeader(stateRoot);
            var index = GetIndexFromAddress(address);

            // An address that has never appeared in the chain.
            if (header.path.ContainsKey(index) == false)
                goto EmptyAccount;
            var wallets = ReadStateBlob(index, header.path[index]);

            foreach (var wallet in wallets)
            {
                if (wallet.address == address)
                    return wallet;
            }

            // Empty account
            EmptyAccount:
            return new WalletState()
            {
                address = address,
                balance = 0
            };
        }

        /// <summary>
        /// Pushes the changes into database.
        /// </summary>
        public string PushState(string prevStateRoot, string stateRoot, WalletState[] changedWallets)
        {
            DataHeader header = null;

            if (prevStateRoot == null)
                header = DataHeader.EmptyState();
            else header = ReadHeader(prevStateRoot);

            var changes = new Dictionary<string, List<WalletState>>();
            foreach (var wallet in changedWallets)
            {
                var index = GetIndexFromAddress(wallet.address);

                if (changes.ContainsKey(index) == false)
                    changes[index] = new List<WalletState>();

                changes[index].Add(wallet);
            }

            foreach (var change in changes)
                header.path[change.Key] = WriteStateBlob(change.Key, change.Value.ToArray());

            return WriteHeader(stateRoot, header);
        }
    }
}
