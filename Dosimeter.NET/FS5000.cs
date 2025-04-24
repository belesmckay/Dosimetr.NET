using System.IO.Ports;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
namespace Dosimeter
{
class FS5000
{
    List<byte> outData = new List<byte>{};
    private string device;
    private SerialPort port;
    private Channel<string> chanenel;


    public FS5000(string device)
    {
        this.device = device;
        
    }
    public void startCommunication()
    {
            Console.WriteLine("Communication Start");
            try
            {
                port = new SerialPort(device,115200)
                {
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                port.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine("Pruser");
            }     
    }
    

    public async Task CtiAsynchronne()
    {
        
        byte[] buffer = new byte[1024];
        var stream = port.BaseStream;
        
        // Posle start command //
        writeData();
        int payloadWilRead = 0;
        while (true)
        {
            
           int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

           if(buffer[0] == 0xAA)
           {

            payloadWilRead = buffer[1] - 1;
            
            for(int i = 2; i <= bytesRead;i++,payloadWilRead--)
            {
                outData.Add(buffer[i]);
            }
           }
           else
           {
            for(int i = 0; i <= bytesRead; i++,payloadWilRead--)
            {
                if(payloadWilRead == 0)
                {
                    byte[] filteredOut = outData.ToArray();
                    string textt = Encoding.ASCII.GetString(filteredOut, 0, filteredOut.Count());
                    string hexx = BitConverter.ToString(filteredOut,0,filteredOut.Count());
                    Console.WriteLine(textt);
                    outData.Clear();
                    break;
                }
                outData.Add(buffer[i]);
            }
           }  
        }
        port.Close();
    }
    public void writeData()
    {
        // 0x0E Read all 0x06 version
        List<byte> dataList = new List<byte> {0xAA,0x5,0x0E,0x01};
        Console.WriteLine();
        byte check = checkSum(dataList.ToArray(),4);
        dataList.Add(check);
        dataList.Add(0x55);
        port.Write(dataList.ToArray(),0,dataList.Count());
    }
    private byte checkSum(byte[] payload,byte size)
    {
        UInt16 buffer = 0;
        for(int i = 0;i < size;i++)
            buffer += payload[i];

        return (byte)(buffer % 256);

    }

}
}