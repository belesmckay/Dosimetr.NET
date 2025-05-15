
//TODO: Add TLS support


using System.Threading.Channels;
using System.Text.Json;

using MQTTnet;
namespace Dosimeter
{
    class IoT
    {
        private string ipAddres;
        private string user;
        private string password;
        private UInt16 port;
        private const string topic = "sensor/dosimetr";

        private Channel<string> ch;
        public IoT(string ipAddres, string user, string password, UInt16 port, Channel<string> ch)
        {
            this.ipAddres = ipAddres;
            this.user = user;
            this.password = password;
            this.port = port;
            this.ch = ch;
        }
        public async Task StartMQTT()
        {
            var mqttFactory = new MqttClientFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {

                Console.WriteLine("********* Tryin open connection to MQTT server " + ipAddres + "  *********");
                try
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(this.ipAddres, this.port)
                        .WithCredentials(this.user, this.password)
                        .Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                }
                catch (Exception)
                {
                    Console.WriteLine("[\x1b[31mERROR\x1b[0m]Cant connect to MQTT server. ");
                    return;
                }

                while (true)
                {
                    string serialdata = await ch.Reader.ReadAsync();
                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(formatData(serialdata))
                        .Build();
                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    Console.WriteLine("[\x1b[32mOK MQTT\x1b[0m]Data sended to broker: " + this.ipAddres + " and topic: " + topic);
                }
                await mqttClient.DisconnectAsync();
            }
        }
        string formatData(string data)
        {
            try
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                string[] split = data.Split(';');
                string[] buffer;

                foreach (string value in split)
                {
                    buffer = value.Split(':');
                    if (buffer.Length != 2)
                        continue;
                    dictionary.Add(buffer[0], buffer[1]);
                }
                return JsonSerializer.Serialize(dictionary);
            }
            catch (Exception e)
            {

            }
            return JsonSerializer.Serialize("ERR");

        }
    }
}
