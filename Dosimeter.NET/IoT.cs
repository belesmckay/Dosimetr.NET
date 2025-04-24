using MQTTnet;
namespace Dosimeter
{
class IoT
{
    private string ipAddres;
    private string user;
    private string password;
    private UInt16 port;
    public IoT(string ipAddres, string user, string password,UInt16 port)
    {
        this.ipAddres = ipAddres;
        this.user = user;
        this.password = password;
        this.port = port;
    }
    public async Task startMQTT()
    {
        var mqttFactory = new MqttClientFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(this.ipAddres,this.port)
                .WithCredentials(this.user,this.password)
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("test/topic")
                .WithPayload("19.5")
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();

            Console.WriteLine("MQTT application message is published.");
        }

    }
    public bool open()
    {
        return false;
    }
    public void Run()
    {
        
    }

}

}