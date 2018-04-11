using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class RpcPacketBase
    {
    }

    public class RpcUnlock : RpcPacketBase
    {
        public string password;
    }
    public class RpcSendFund : RpcPacketBase
    {
        public string receiverAddr;
        public double amount;
    }
}
