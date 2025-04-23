using System;
using System.Text;
using System.Threading.Tasks;
namespace Dosimeter
{
class Program
{
    static void Main(string[] args)
    {
        
        FS5000 FS = new FS5000("/dev/ttyUSB0");

        FS.startCommunication();
        _ = Task.Run(() => FS.CtiAsynchronne());

        while(true)
        {
            
        }
        
        
    }
}
}