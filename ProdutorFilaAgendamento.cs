using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace livedio
{
    public class ProdutorFilaAgendamento : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ProdutorFilaAgendamento> _logger;

        public ProdutorFilaAgendamento(ILogger<ProdutorFilaAgendamento> logger)
        {
            // Definindo a conexão com o Service Bus
            var connectionString = "ConnectionString"; // Substitua pela sua connection string
            var queueName = "fila1"; // Substitua pelo nome da sua fila

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Chamando o método para enviar as mensagens
            await ProduzirMensagem(cancellationToken);
        }

        public async Task ProduzirMensagem(CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancelamento solicitado, interrompendo o envio das mensagens.");
                        break;
                    }

                    _logger.LogInformation($"Enviando mensagem: {i}");
                    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes("Número " + i))
                    {
                        ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(1)
                    };

                    await _sender.SendMessageAsync(message, cancellationToken);
                }

                _logger.LogInformation("Concluído o envio das mensagens.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro: {ex.GetType().FullName} | Mensagem: {ex.Message}");
            }
            finally
            {
                await _sender.DisposeAsync();
                _logger.LogInformation("Conexão com o Service Bus finalizada.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _client.DisposeAsync();
            _logger.LogInformation("Conexão com o Service Bus finalizada.");
        }
    }
}
