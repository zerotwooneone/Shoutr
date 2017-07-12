using Library;
using Library.Broadcast;
using Library.Throttle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static string basicHelp = "To run the listener, run with the argument --listen. To broadcast, specify a file with --file=\"filename.extension\"";
        const int BUFSIZE = 16;

        static void runListen()
        {
            UdpClient receiver = new UdpClient(3036);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] received = receiver.Receive(ref sender);
            Console.Write(received);
        }

        static void runBroadcast(string name)
        {
            if (File.Exists(name))
            {
                FileStream fs = new FileStream(name, FileMode.Open);
                byte[] buffer = new byte[BUFSIZE];
                object fsLock = new object();
                UdpClient sender = new UdpClient(3036);
                IPEndPoint listener = new IPEndPoint(IPAddress.Broadcast, 3036);
                long offset = 0;
                int aOffset = 0;
                sender.Connect(listener);
                while (offset < fs.Length)
                {

                    lock (fsLock)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                        long read = fs.Read(buffer, aOffset, BUFSIZE);
                        //for (int i = 0; i < read; i++)
                        //{
                        //    Console.Write(buffer[i] + " ");
                        //}
                        //string s = System.Text.Encoding.UTF8.GetString(buffer);
                        //Console.Write(s.Substring(0, (int)read));
                        offset += read;

                        sender.Send(buffer, (int)read);
                    }
                }
                sender.Close();
            }
            else Console.WriteLine("file doesn't exist");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(basicHelp);
                return;
            }
            else if (args.Length == 1)
            {
                if (args[0] == "--listen")
                {
                    runListen();
                    return;
                }
                else
                {
                    string[] separators = { "=", "\"" };
                    string[] bargs = args[0].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (bargs[0] == "--file" && bargs.Length == 2)
                        runBroadcast(bargs[1]);
                    else
                        Console.WriteLine(basicHelp);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Multi-argument support not implemented.");
                Console.WriteLine(basicHelp);
            }
        }
    }
}
