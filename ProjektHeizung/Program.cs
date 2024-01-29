using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

class MqttPublisher
{
    static async Task Main(string[] args)
    {
        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .Build();

        await mqttClient.ConnectAsync(options);

        await SendMessagesAsync(mqttClient);

        Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
        Console.ReadLine();

        await mqttClient.DisconnectAsync();
    }

    static async Task SendMessagesAsync(IMqttClient mqttClient)
    {
        var heaterMessage = new
        {
            ID = "H1",
            Temperatur = "25C"
        };

        string jsonMessage = JsonConvert.SerializeObject(heaterMessage);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("/Heizungen/")
            .WithPayload(jsonMessage)
            .WithExactlyOnceQoS()
            .Build();

        await mqttClient.PublishAsync(message);
    }
}
