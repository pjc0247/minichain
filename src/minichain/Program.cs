using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minichain
{
    class Program
    {
        private static readonly string Logo = @"            _       _      _           _       
           (_)     (_)    | |         (_)      
  _ __ ___  _ _ __  _  ___| |__   __ _ _ _ __  
 | '_ ` _ \| | '_ \| |/ __| '_ \ / _` | | '_ \ 
 | | | | | | | | | | | (__| | | | (_| | | | | |
 |_| |_| |_|_|_| |_|_|\___|_| |_|\__,_|_|_| |_|";

        static void Main(string[] args)
        {
            Console.Title = "MINICHAIN";
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

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
