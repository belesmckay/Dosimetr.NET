
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
        private const string measuredDatatopic = "dosimetr/FS5000";
        private const string DiscoveryDoseRatetopic = "homeassistant/sensor/dosimetr/doserate/config";
        private const string DiscoveryDosetopic = "homeassistant/sensor/dosimetr/dose/config";
        private IMqttClient mqttClient;
        private Channel<string> ch;


        /******************** Definition of discovery messages ********************/

        public static readonly object DoseRateDiscoveryPayload = new
        {
            name = "DoseRate",
            state_topic = measuredDatatopic,
            unique_id = "doserate",

            value_template = "{{ value_json.DR | replace(\"uSv/h\", \"\") | float }}",
            unit_of_measurement = "µSv/h",
            device = new
            {
                identifiers = new[] { "dosimetr" },
                name = "Dosimetr"
            }
        };
        public static readonly object DoseDiscoveryPayload = new
        {
            name = "Dose",
            state_topic = measuredDatatopic,
            unique_id = "dose",

            value_template = "{{ value_json.D | replace(\"uSv\", \"\") | float }}",
            unit_of_measurement = "µSv",
            device = new
            {
                identifiers = new[] { "dosimetr" },
                name = "Dosimetr"
            }
        };


        /***************** Methods **********************/


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

            using (mqttClient = mqttFactory.CreateMqttClient())
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

                sendDiscoveryMessages();

                while (true)
                {
                    string serialdata = await ch.Reader.ReadAsync();
                    sendMQTTMessage(measuredDatatopic, formatData(serialdata), false);
                }
                await mqttClient.DisconnectAsync();
            }
        }


        void sendDiscoveryMessages()
        {
            sendMQTTMessage(DiscoveryDoseRatetopic, JsonSerializer.Serialize(DoseRateDiscoveryPayload, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), true);
            sendMQTTMessage(DiscoveryDosetopic, JsonSerializer.Serialize(DoseDiscoveryPayload, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), true);
        }





        async void sendMQTTMessage(string topic, string data, bool retain)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(data)
            .WithRetainFlag(retain)
                        .Build();
            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            Console.WriteLine("[\x1b[32mOK MQTT\x1b[0m]Data sended to broker: " + this.ipAddres + " and topic: " + topic);
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
