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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        await SendMessagesAsync(mqttClient, cancellationTokenSource.Token);

        Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
        await mqttClient.DisconnectAsync();
    }

    //die SendMessagesAsync-Methode wird in einer while-Schleife wiederholt, bis der Nutzer eine Taste drückt und das Programm beendet.
    static async Task SendMessagesAsync(IMqttClient mqttClient, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var heaterMessage = new
            {
                ID = "H1",
                Temperatur = "20C"
            };

            string jsonMessage = JsonConvert.SerializeObject(heaterMessage);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("/Heizungen/")
                .WithPayload(jsonMessage)
                .WithExactlyOnceQoS()
                .Build();

            await mqttClient.PublishAsync(message);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
