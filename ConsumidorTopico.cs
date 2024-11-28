using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Livedio
{
    public class ConsumidorTopico : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusReceiver _receiver;
        private readonly ILogger<ConsumidorTopico> _log;
        private readonly string _connectionString ="connectionString"; // Substitua pela sua string de conexão do Azure Service Bus
        private readonly string _topicName = "top1"; // Substitua pela nome do seu topico na Azure Service Bus
        private readonly string _subscriptionName = "assinatura1"; // Substitua nome da sua  Assinatura

        public ConsumidorTopico(ILogger<ConsumidorTopico> log)
        {
            _log = log;

            // Inicializando o cliente do Service Bus
            _client = new ServiceBusClient(_connectionString);
            _receiver = _client.CreateReceiver(_topicName, _subscriptionName, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock // Usando PeekLock para garantir o processamento correto
            });

            Console.WriteLine("Iniciando a leitura do tópico no ServiceBus...");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop para processar as mensagens enquanto o serviço não for cancelado
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Recebe uma mensagem do tópico
                    ServiceBusReceivedMessage message = await _receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);

                    if (message != null)
                    {
                        // Processa a mensagem
                        await ProcessarMensagem(message, stoppingToken);
                    }
                    else
                    {
                        // Caso não haja mensagens, aguarda um pequeno tempo antes de tentar novamente
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Em caso de erro, chama o método de processamento de erro
                    await ProcessarErro(ex);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            // Fechando a conexão com o Service Bus
            await _receiver.CloseAsync();
            await _client.DisposeAsync();
            Console.WriteLine("Finalizando conexão com o Azure Service Bus.");
        }

        private async Task ProcessarMensagem(ServiceBusReceivedMessage message, CancellationToken token)
        {
            var corpo = Encoding.UTF8.GetString(message.Body);

            Console.WriteLine("[Nova Mensagem Recebida no Tópico] " + corpo);

            // Completa a mensagem após o processamento
            await _receiver.CompleteMessageAsync(message);
        }

        private Task ProcessarErro(Exception ex)
        {
            Console.WriteLine("[Erro] " +
                ex.GetType().FullName + " " +
                ex.Message);
            return Task.CompletedTask;
        }
    }
}