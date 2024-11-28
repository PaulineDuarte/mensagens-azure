using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Livedio
{
    public class ConsumidorFila : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger<ConsumidorFila> _logger;

        public ConsumidorFila(ILogger<ConsumidorFila> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Inicialize o cliente e o processador
            _client = new ServiceBusClient("connectionString");  // Substitua pela sua string de conexão do Azure Service Bus
            _processor = _client.CreateProcessor("fila1", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            _logger.LogInformation("Consumidor de fila inicializado.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando a leitura da fila no Azure Service Bus...");

            // Configure os handlers de mensagens e erros
            _processor.ProcessMessageAsync += ProcessarMensagem;
            _processor.ProcessErrorAsync += ProcessarErro;

            // Inicie o processador
            await _processor.StartProcessingAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Finalizando conexão com o Azure Service Bus...");
            await _processor.StopProcessingAsync(stoppingToken);
            await _client.DisposeAsync();
            _logger.LogInformation("Conexão finalizada.");
        }

        private async Task ProcessarMensagem(ProcessMessageEventArgs args)
        {
            var corpo = args.Message.Body.ToString();
            _logger.LogInformation("[Nova Mensagem Recebida] {Mensagem}", corpo);

            try
            {
                // Confirma o processamento da mensagem
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Mensagem processada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem.");
            }
        }

        private Task ProcessarErro(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "[Erro] Source: {Source} | Mensagem: {Mensagem}",
                args.ErrorSource,
                args.Exception.Message);

            return Task.CompletedTask;
        }
    }
}
