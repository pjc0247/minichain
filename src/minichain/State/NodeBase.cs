using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    public class NodeBase
    {
        public bool isAlive { get; protected set; }

        public PeerPool peers { get; private set; }
        public Wallet wallet { get; private set; }
        protected TransactionPool txPool = new TransactionPool();

        private Dictionary<Type, Action<Peer, object>> subscribers = new Dictionary<Type, Action<Peer, object>>();
        private HashSet<string> processedPackets = new HashSet<string>();

        public NodeBase()
        {
            peers = new PeerPool(this);
            wallet = new Wallet();
        }

        public virtual void Stop()
        {
            isAlive = true;
        }

        public void Subscribe<T>(Action<Peer, T> callback)
        {
            subscribers[typeof(T)] = (peer, pkt) =>
            {
                callback(peer, (T)pkt);
            };
        }
        public void ProcessPacket(Peer sender, PacketBase packet)
        {
            if (string.IsNullOrEmpty(packet.pid)) return;
            if (processedPackets.Contains(packet.pid)) return;

            if (subscribers.ContainsKey(packet.GetType()))
            {
                try
                {
                    subscribers[packet.GetType()]?.Invoke(sender, packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            processedPackets.Add(packet.pid);

            if (packet is BroadcastPacket bpacket)
                BroadcastPacket(bpacket);
        }

        private void BroadcastPacket(BroadcastPacket packet)
        {
            if (packet.ttl == 0) return;

            packet.ttl -= 1;
            SendPacketToAllPeers(packet);
        }
        protected void SendPacket(Peer sender, PacketBase packet)
        {
            sender.SendPacket(packet);
        }
        protected void SendPacketToAllPeers(PacketBase packet)
        {
            peers.SendPacketToAllPeers(packet);
        }
    }
}
