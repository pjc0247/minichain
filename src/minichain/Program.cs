using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace minichain
{
    class Program
    {
        private static readonly string Logo = @"
                  _       _      _           _       
                 (_)     (_)    | |         (_)      
        _ __ ___  _ _ __  _  ___| |__   __ _ _ _ __  
       | '_ ` _ \| | '_ \| |/ __| '_ \ / _` | | '_ \ 
       | | | | | | | | | | | (__| | | | (_| | | | | |
       |_| |_| |_|_|_| |_|_|\___|_| |_|\__,_|_|_| |_|
      
                Minimal implementation of blockchain
                                   Written in CSharp
                                   pjc0247@naver.com
      
";

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Title = "minichain";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Logo);
            Console.WriteLine();
            Console.ResetColor();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var miner = new Miner();
            miner.Start();

            try
            {
                var peers = miner.peers;

                while (true)
                {
                    var addr = Console.ReadLine();

                    peers.AddPeer(addr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void UpdateScreen(Miner miner)
        {
            while (true)
            {
                int backupX = Console.CursorLeft;
                int backupY = Console.CursorTop;

                Console.CursorLeft = 60;
                Console.CursorTop = 7;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PEERS: {miner.peers.alivePeers}");

                Console.CursorLeft = backupX;
                Console.CursorTop = backupY;
                Console.ResetColor();

                Thread.Sleep(1000);
            }
        }
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
