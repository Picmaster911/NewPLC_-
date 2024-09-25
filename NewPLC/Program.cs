using S7.Net;

namespace NewPLC
{
    internal class Program
    {
        static  void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var plc = new PlcService("S71200", CpuType.S71200, "192.168.88.110", 0, 1,1000);
            var _cancelTokenSource = new CancellationTokenSource();
            plc.Connect(_cancelTokenSource.Token);
            Console.ReadLine();
        }
    }
}
