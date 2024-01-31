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
        Random random = new Random();

        string[] heaterIds = { "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10" };

        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var heaterId in heaterIds)
            {
                int istTemperatur = random.Next(15, 26);
                int sollTemperatur = random.Next(15, 26);

                var heaterMessage = new
                {
                    ID = heaterId,
                    IstTemperatur = $"{istTemperatur}C",
                    SollTemperatur = $"{sollTemperatur}C"
                };

                string jsonMessage = JsonConvert.SerializeObject(heaterMessage);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"/Heizungen/{heaterId}")
                    .WithPayload(jsonMessage)
                    .WithExactlyOnceQoS()
                    .Build();

                await mqttClient.PublishAsync(message);
            }

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
