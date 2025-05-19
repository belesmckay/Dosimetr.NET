using System.Text;
using System.Threading.Channels;
namespace Dosimeter
{
    class Program
    {
        /****************** Variables ******************/
        static private string[] logo = {"██████   ██████  ███████ ██ ███    ███ ███████ ████████ ███████ ██████         ███    ██ ███████ ████████ ",
                                    "██   ██ ██    ██ ██      ██ ████  ████ ██         ██    ██      ██   ██        ████   ██ ██         ██    ",
                                    "██   ██ ██    ██ ███████ ██ ██ ████ ██ █████      ██    █████   ██████         ██ ██  ██ █████      ██    ",
                                    "██   ██ ██    ██      ██ ██ ██  ██  ██ ██         ██    ██      ██   ██        ██  ██ ██ ██         ██    ",
                                    "██████   ██████  ███████ ██ ██      ██ ███████    ██    ███████ ██   ██     ██ ██   ████ ███████    ██    ",
                                   };

        private static string device = "/dev/ttyUSB0";
        private static string MQTT_USER = "MQTT";
        private static string MQTT_PASSWORD = "password";
        private static string MQTT_IP = "localhost";
        private static ushort MQTT_PORT = 1883;
        const Int32 BufferSize = 128;

        /****************** Methods ******************/
        static private bool getConfiguration(string path)
        {
            try
            {
                var file = File.OpenRead(path);
                var streamReader = new StreamReader(file, Encoding.UTF8, true, BufferSize);
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    int index = line.IndexOf(':');
                    if (index > 0)
                    {
                        switch (line.Substring(0, index).Replace(" ", "").Replace(":", ""))
                        {
                            case "MQTT_USER":
                                MQTT_USER = line.Substring(index).Replace(" ", "").Replace(":", "");
                                break;
                            case "MQTT_PASSWORD":
                                MQTT_PASSWORD = line.Substring(index).Replace(" ", "").Replace(":", "");
                                break;
                            case "MQTT_HOST":
                                MQTT_IP = line.Substring(index).Replace(" ", "").Replace(":", "");
                                break;
                            case "MQTT_PORT":
                                MQTT_PORT = ushort.Parse(line.Substring(index).Replace(" ", "").Replace(":", ""));
                                break;
                            case "USB_DEVICE":
                                device = line.Substring(index).Replace(" ", "").Replace(":", "");
                                break;
                        }
                    }
                }
                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("[\x1b[31mERROR\x1b[0m]Dosimeter.NET have not acces right to configuration file.");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("[\x1b[31mERROR\x1b[0m]Configuration file is not exist.");
                return false;
            }
        }
        static private bool procesArguments(string[] args, out string configPath)
        {
            configPath = "";

            for (byte i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-c"))
                {
                    if ((i + 1) < args.Length)
                    {
                        configPath = args[i + 1];
                        Console.WriteLine("[\x1b[32mOK\x1b[0m]Configuration argument detected ");
                    }
                    else
                    {
                        Console.WriteLine("[\x1b[31mERROR\x1b[0m]Path to configuration file argument after |-c| is missing.");
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        static async Task Main(string[] args)
        {
            /**** Print logo ****/
            for (byte i = 0; i < logo.Length; i++)
                Console.WriteLine(logo[i]);

            /**** get server configuration ****/
            if (!procesArguments(args, out string configPath))
            {
                Console.WriteLine("[\x1b[32mOK\x1b[0m]Non configuration path was find. Default setting will used.");
            }

            if (configPath.Length != 0)
            {
                if (getConfiguration(configPath) == false)
                    return;
            }
            Channel<string> ch = Channel.CreateUnbounded<string>();

            FS5000 FS = new FS5000(device, ch);
            IoT iot = new IoT(MQTT_IP, MQTT_USER, MQTT_PASSWORD, MQTT_PORT, ch);

            FS.startCommunication();
            var task1 = Task.Run(() => FS.ReadAsynch());
            var task2 = Task.Run(() => iot.StartMQTT());
            //await Task.WhenAny(task1);
            await Task.WhenAny(task1, task2);

        }
    }
}
