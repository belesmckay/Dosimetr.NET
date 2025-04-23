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
    public bool open()
    {
        return false;
    }
    public void Run()
    {
        
    }

}

}