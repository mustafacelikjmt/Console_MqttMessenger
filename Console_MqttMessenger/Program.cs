using MQTTnet;
using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var certificate = new X509Certificate2("C:\\Users\\musta\\source\\repos\\Console_MqttMessenger\\Console_MqttMessenger\\Certificate.pfx", "1010");
        var mqttFactory = new MqttFactory();
        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("an726pjx0w8v9-ats.iot.eu-north-1.amazonaws.com", 8883)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    Certificates = new[] { certificate }
                })
                .WithClientId("AspClientAlpha")
                .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithoutPacketFragmentation()
                .Build();
            string topic = "esp32";
            string subtopic = "#";
            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to AWS IoT");
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(subtopic).Build());
                Console.WriteLine($"Subscribed to topic: {subtopic}");
            };
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received message in topic: " + e.ApplicationMessage.Topic);
                Console.WriteLine("Received message: " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                return Task.CompletedTask;
            };
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            while (true)
            {
                Console.WriteLine("Lütfen Mesajınızı Yazın (çıkmak için 'exit' yazın):");
                string mesaj = Console.ReadLine();
                if (mesaj.ToLower() == "exit")
                {
                    break;
                }
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(mesaj)
                    .Build();
                await mqttClient.PublishAsync(message);
                Console.WriteLine("Message published");
            }
            Console.WriteLine("Exit made.");
            await mqttClient.DisconnectAsync();
        }
    }
}