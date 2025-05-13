using System.IO.Ports;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Reflection.Metadata;
namespace Dosimeter
{
    class DosimeterData
    {
        private double doserate;

        public double Doserate
        {
            get { return doserate; }
            set { this.doserate = value; }
        }
        private double dose;
        public double Dose
        {
            get { return dose; }
            set { this.dose = value; }
        }
        private double avg_doserate;
        public double Avg_doserate
        {
            get { return avg_doserate; }
            set { this.avg_doserate = value; }
        }
        private UInt16 cps;
        public UInt16 Cps
        {
            get { return cps; }
            set { this.cps = value; }
        }
        private UInt16 cpm;
        public UInt16 Cpm
        {
            get { return cpm; }
            set { this.cpm = value; }
        }

        private byte warning;
        public byte Warning
        {
            get { return warning; }
            set { this.warning = value; }
        }
    }
    class FS5000(string device, Channel<string> ch)
    {
        List<byte> outData = new List<byte> { };
        private string device = device;

        private bool running = false;
        private SerialPort? port;
        private Channel<string> ch = ch;

        public void startCommunication()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("********* Tryin open serial port to " + device + "  *********");
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


        public async Task CtiAsynchronne()
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

                if (buffer[0] == 0xAA)
                {

                    payloadWilRead = buffer[1] - 2;

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
        public void writeData()
        {
            List<byte> dataList = new List<byte> { 0xAA, 0x5, 0x0E, 0x01 };
            // 0x0E Read all 0x06 version

            byte check = checkSum(dataList.ToArray(), 4);
            dataList.Add(check);
            dataList.Add(0x55);
            port.Write(dataList.ToArray(), 0, dataList.Count());
        }
        private byte checkSum(byte[] payload, byte size)
        {
            UInt16 buffer = 0;
            for (int i = 0; i < size; i++)
                buffer += payload[i];

            return (byte)(buffer % 256);

        }


    }
}
