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
    
    private Channel<string> ch;
    public IoT(string ipAddres, string user, string password,UInt16 port,Channel<string> ch)
    {
        this.ipAddres   = ipAddres;
        this.user       = user;
        this.password   = password;
        this.port       = port;
        this.ch         = ch;
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


            while (true)
            {
                //Console.WriteLine("SEND");
                string serialdata = await ch.Reader.ReadAsync();
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("sensor/dosimetr")
                    .WithPayload(formatData(serialdata))
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                //formatData(serialdata);
            }
            await mqttClient.DisconnectAsync();
        }
    }
    string formatData(string data)
    {
        try{
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        string[] split = data.Split(';');
        string[] buffer;

        

        foreach(string value in split)
        {
            buffer = value.Split(':');
            if(buffer.Length != 2)
                continue;  
            dictionary.Add(buffer[0],buffer[1]);
        }
        return JsonSerializer.Serialize(dictionary);
        
        }
        catch(Exception e)
        {
            Console.WriteLine(e);

        }
        return JsonSerializer.Serialize("ERR");
        
       
    }
}
}