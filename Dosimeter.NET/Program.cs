using System;
using System.Text;
using System.Threading.Tasks;
namespace Dosimeter
{
class Program
{
    static async Task Main(string[] args)
    {
        
        FS5000 FS = new FS5000("/dev/ttyUSB0");
        IoT iot = new IoT("localhost","beles","strike",1883);

        FS.startCommunication();
        var task1 = Task.Run(() => FS.CtiAsynchronne());
        var task2 = Task.Run(() => iot.startMQTT());

        await Task.WhenAll(task1,task2);
    

        
        
    }
}
}