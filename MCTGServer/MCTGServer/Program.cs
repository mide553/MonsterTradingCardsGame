using System;
using System.Threading;

namespace MCTG
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Thread serverThread = new Thread(new ThreadStart(server.Start));
            serverThread.Start();
        }
    }
}