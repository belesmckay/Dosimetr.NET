
//TODO: Add control more control function of the - FS5000

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
            int payloadWilRead = 0;
            byte[] buffer = new byte[1024];
            var stream = port.BaseStream;

            // send read command //
            writeData();

            while (running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                Console.WriteLine("[\x1b[32mOK FS5000\x1b[0m]Data from serial port available ");
                //TODO: I have to rework this in to more elegant state. This is mess
                //Check start of the packet
                if (buffer[0] == 0xAA)
                {
                    payloadWilRead = buffer[1] - 2;
                    // i= 3 becouse i drop 0XAA, payload size and checksum 
                    for (int i = 3; i <= bytesRead; i++, payloadWilRead--)
                    {
                        outData.Add(buffer[i]);
                    }
                }
                else
                {
                    for (int i = 0; i <= bytesRead; i++, payloadWilRead--)
                    {
                        if (payloadWilRead == 0)
                        {
                            byte[] filteredOut = outData.ToArray();
                            string textt = Encoding.ASCII.GetString(filteredOut, 0, filteredOut.Count());

                            //Give datat to IoT task 
                            await ch.Writer.WriteAsync(textt);

                            outData.Clear();
                            break;
                        }
                        outData.Add(buffer[i]);
                    }
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
