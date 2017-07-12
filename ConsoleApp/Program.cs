using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static string basicHelp = "To run the listener, run with the argument --listen. To broadcast, specify a file with --file=\"filename.extension\"";
        const int BUFSIZE = 16;

        static void writeFileContents(string name)
        {
            if (File.Exists(name))
            {
                FileStream fs = new FileStream(name, FileMode.Open);
                byte[] buffer = new byte[BUFSIZE];
                object fsLock = new object();
                long offset = 0;
                int aOffset = 0;
                while (offset < fs.Length)
                {
                    lock (fsLock)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                        long read = fs.Read(buffer, aOffset, BUFSIZE);
                        for (int i = 0; i < read; i++)
                        {
                            Console.Write(buffer[i] + " ");
                        }
                        offset += read;
                        Console.WriteLine();
                        Console.WriteLine(read);
                    }
                }
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
                    Console.WriteLine("Listener not implemented");
                    return;
                }
                else
                {
                    string[] separators = { "=", "\"" };
                    string[] bargs = args[0].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (bargs[0] == "--file" && bargs.Length == 2)
                        writeFileContents(bargs[1]);
                    else
                        Console.WriteLine(basicHelp);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Multi-argument support not yet implemented.");
                Console.WriteLine(basicHelp);
            }
        }
    }
}
