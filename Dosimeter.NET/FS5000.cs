
//TODO: Add more control functions of the - FS5000

//NOTE: In this moment, you can only read values from the dosimeter (which is the main goal of this project ). Clear dose and other control function I´ll try add later
//NOTE: I want notice that protocol structure and all commands and values i founded and used from this project https://gist.github.com/brookst/bdbede3a8d40eb8940a5b53e7ca1f6ce 
using System.IO.Ports;
using System.Text;
using System.Threading.Channels;
namespace Dosimeter
{
    class FS5000(string device, Channel<string> ch)
    {
        List<byte> outData = new List<byte> { };
        private string device = device;
        private bool running = false;
        private SerialPort? port;
        private Channel<string> ch = ch;

        //**** Initialization ****/
        public void startCommunication()
        {
            Console.WriteLine("********* Trying open serial port to " + device + "  *********");
            try
            {
                port = new SerialPort(device, 115200)
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
            catch (Exception e)
            {
                Console.WriteLine("[\x1b[31mERROR\x1b[0m]Cant open Serial port ");
                return;
            }
            Console.WriteLine("[\x1b[32mOK\x1b[0m]Port open ");
            running = true;
        }

        public async Task ReadAsynch()
        {
            byte[] received;
            byte[] buffer = new byte[1024];
            var stream = port.BaseStream;
            byte remainRead = 0;

            // send read command //
            // This will make FS5000 to send you data
            writeData();

            while (running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                for (byte i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 0xAA && bytesRead > i)
                    {
                        remainRead = (byte)(buffer[i + 1] - 4); // Minus DLC , Command 0x0E, Checksum and End byte
                        i += 2; //Skip DLC and Command 0x0E
                        continue;
                    }
                    if (remainRead > 0)
                    {
                        outData.Add(buffer[i]);
                        remainRead--;
                    }
                }
                //Whole packet readed
                if (remainRead == 0)
                {
                    received = outData.ToArray();
                    outData.Clear();
                    string textt = Encoding.ASCII.GetString(received, 0, received.Length);
                    await ch.Writer.WriteAsync(textt);
                }
            }
            port.Close();
        }
        /************************  Communication methods ************************/

        /**** Send request to sending data from FS 5000 ****/
        public void writeData()
        {
            //Start byte, DLC, Read out command, Turn_On, CheckSum, EndByte
            List<byte> dataList = new List<byte> { 0xAA, 0x5, 0x0E, 0x01 };
            byte check = checkSum(dataList.ToArray(), 4);

            dataList.Add(check);
            dataList.Add(0x55);
            port.Write(dataList.ToArray(), 0, dataList.Count());
        }
        /**** Calculate checksum ****/
        private byte checkSum(byte[] payload, byte size)
        {
            UInt16 buffer = 0;
            for (int i = 0; i < size; i++)
                buffer += payload[i];

            return (byte)(buffer % 256);
        }
    }
}
