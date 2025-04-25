using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
namespace Dosimeter
{
class Program
{
    private 
    static async Task Main(string[] args)
    {
        Channel<string> ch = Channel.CreateUnbounded<string>();
        
        FS5000 FS = new FS5000("/dev/ttyUSB0",ch);
        IoT iot = new IoT("192.168.1.23","karel","AVV17def",1883,ch);

        FS.startCommunication();
        var task1 = Task.Run(() => FS.CtiAsynchronne());
        var task2 = Task.Run(() => iot.startMQTT());

        await Task.WhenAll(task1,task2);
    

        
        
    }
}
}