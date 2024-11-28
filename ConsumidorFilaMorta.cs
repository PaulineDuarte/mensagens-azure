using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace livedio
{
    public class ConsumidorFilaMorta : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusReceiver _receiver;
        private readonly string _queueName = "fila1/$DeadLetterQueue";
        private readonly string _connectionString = "connectionString";  // Substitua pela sua string de conexão do Azure Service Bus
        public ConsumidorFilaMorta()
        {
            // Inicializando o client e receiver para ler mensagens da fila morta
            _client = new ServiceBusClient(_connectionString);
            _receiver = _client.CreateReceiver(_queueName, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            Console.WriteLine("Iniciando a leitura da fila morta no ServiceBus...");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop principal para ler as mensagens
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Lê a mensagem da fila morta
                    var receivedMessage = await _receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);

                    if (receivedMessage != null)
                    {
                        // Processa a mensagem
                        await ProcessarMensagem(receivedMessage, stoppingToken);
                    }
                    else
                    {
                        // Aguardar um pequeno tempo antes de tentar novamente
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
            // Fechando o receiver e o client ao parar o serviço
            await _receiver.CloseAsync();
            await _client.DisposeAsync();
            Console.WriteLine("Finalizando conexão com o Azure Service Bus.");
        }

        private async Task ProcessarMensagem(ServiceBusReceivedMessage message, CancellationToken token)
        {
            var corpo = Encoding.UTF8.GetString(message.Body);

            Console.WriteLine("[Nova Mensagem Recebida na fila morta] " + corpo);

            // Complete a mensagem após o processamento
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