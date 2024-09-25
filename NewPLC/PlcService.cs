using S7.Net;
using S7.Net.Types;

namespace NewPLC
{
    public class PlcService
    {
        private string _plcName;
        private CpuType _cpuType;
        private string _ipAdress;
        private int _rack;
        private int _slot;
        private int _timeCycle;
        private CancellationTokenSource _cancelTokenSource;
        private Plc _plc;

        private List<DataItem> _dataForRequest = new List<DataItem>();
        private bool _erroConection;


        public PlcService(string cpuName, CpuType cpuType, string ipAdress, int rack, int slot, int timeCycle)
        {
            _plcName = cpuName;
            _cpuType = cpuType;
            _ipAdress = ipAdress;
            _rack = rack;
            _slot = slot;
            _timeCycle = timeCycle;
            _cancelTokenSource = new CancellationTokenSource();
            _plc = new Plc(_cpuType, _ipAdress, (short)_rack, (short)_slot);
        }

        public async Task Start(CancellationTokenSource CancelToken) 
        {
            try
            {
                var openPlcTask = _plc.OpenAsync(CancelToken.Token);

                if (await Task.WhenAny(openPlcTask, Task.Delay(10000)) == openPlcTask)
                {
                    if (openPlcTask.IsCompletedSuccessfully)
                    {
                        _erroConection = false;
                        Console.WriteLine($"Plc is Connected = {_plc.IsConnected}");
                        StartPeriodicTaskAsync(_cancelTokenSource.Token);
                    }
                    else
                    {
                        _erroConection = true;
                       _cancelTokenSource.Cancel();
                        Console.WriteLine("Plc failed to connect.");
                    }
                }
                else
                {
                    _erroConection = true;
                    _cancelTokenSource.Cancel();
                     Console.WriteLine("Plc not Available (timeout)");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
              
            };

        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_plc.IsConnected)
                {
                    _cancelTokenSource = new CancellationTokenSource();
                    await Start(_cancelTokenSource);
                }
              
            }
               
        }

        public async Task StartPeriodicTaskAsync(CancellationToken cancellationToken)
        {

            using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(2)))
            {
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    Console.WriteLine("Task executed at: " + System.DateTime.Now);
                    try
                    {
                        if (_plc.IsConnected)
                        {
                            var data = _plc.Read(DataType.DataBlock, 2, 1, VarType.Bit, 1);
                            Console.WriteLine($"Data from PLC : {System.DateTime.Now} data = {data}" );
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        _cancelTokenSource.Cancel();
                        _plc.Close();
                    }
                }
            }
        }
    }
}
