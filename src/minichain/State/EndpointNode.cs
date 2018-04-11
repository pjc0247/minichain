﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace minichain
{
    public class EndpointNode : NodeBase
    {
        protected delegate void NewBlockDiscoveredDelegate(Block block);

        protected NewBlockDiscoveredDelegate onNewBlockDiscovered;

        public Wallet wallet { get; private set; }
        public ChainState chain { get; private set; }

        private RpcServer rpcServer;

        private Thread discoverThread;

        public EndpointNode()
        {
            rpcServer = new RpcServer(this, 9916);

            chain = new ChainState();
            wallet = new Wallet(chain);

            Subscribe<PktBroadcastNewBlock>(OnNewBlock);
            Subscribe<PktNewTransaction>(OnNewTransaction);
            Subscribe<PktRequestPeers>(OnRequestPeers);
            Subscribe<PktResponsePeers>(OnResponsePeers);

            discoverThread = new Thread(DiscoverWorker);
            discoverThread.Start();
        }
        public override void Stop()
        {
            base.Stop();

            discoverThread.Join();
        }

        public void SendTransaction(Transaction tx)
        {
            if (Transaction.IsValidTransaction(tx) == false)
                throw new ArgumentException(nameof(tx));

            txPool.AddTransaction(tx);
            SendPacketToAllPeers(new PktNewTransaction()
            {
                sentBlockNo = chain.currentBlock.blockNo,
                tx = tx
            });
        }

        private void DiscoverWorker()
        {
            while (isAlive)
            {
                // Already reached to MaxPeers, Skip discovery
                if (peers.alivePeers >= PeerPool.MaxPeers)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                SendPacketToAllPeers(new PktRequestPeers() { });
                Thread.Sleep(5000);
            }
        }

        private void OnRequestPeers(Peer sender, PktRequestPeers pkt)
        {
            SendPacket(sender, new PktResponsePeers()
            {
                addrs = peers.GetPeerAddresses()
            });
        }
        private void OnResponsePeers(Peer sender, PktResponsePeers pkt)
        {
            foreach (var addr in pkt.addrs)
                peers.AddPeer(addr);
        }
        protected void DiscoverBlock(Block block)
        {
            SendPacketToAllPeers(new PktBroadcastNewBlock()
            {
                block = block
            });
        }
        protected virtual void OnNewBlock(Peer sender, PktBroadcastNewBlock pkt)
        {
            // Peer sent invalid block.
            if (Block.IsValidBlockLight(pkt.block, pkt.block.nonce) == false)  { Console.WriteLine("Invalid block"); return; }
            // My block is longer than received
            if (chain.currentBlock.blockNo >= pkt.block.blockNo) return;

            chain.PushBlock(pkt.block);
            onNewBlockDiscovered?.Invoke(pkt.block);
        }
        protected virtual void OnNewTransaction(Peer sender, PktNewTransaction pkt)
        {
            // Peer sent invalid transaction
            if (pkt.tx == null) return;
            // Future transaction which cannot be processed at this time
            if (chain.currentBlock.blockNo < pkt.sentBlockNo) return;
            if (chain.currentBlock.blockNo >= pkt.sentBlockNo + 10) return;
            if (Transaction.IsValidTransaction(pkt.tx) == false) return;

            txPool.AddTransaction(pkt.tx);
        }
    }
}
