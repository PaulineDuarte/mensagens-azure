using Azure.Messaging.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Livedio
{
    public class ProdutorTopico
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly string _topicName = "top1";
        private readonly string _connectionString = "connectionString";

        public ProdutorTopico()
        {
            // Inicializando o cliente e sender para enviar mensagens ao tópico
            _client = new ServiceBusClient(_connectionString);
            _sender = _client.CreateSender(_topicName);

            Console.WriteLine("Conectado ao Azure Service Bus.");
        }
        public async Task ProduzirMensagem()
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    Console.WriteLine($"Enviando mensagem: {i}");

                    // Criando a mensagem para enviar
                    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes($"Número {i}"));

                    // Enviando a mensagem para o tópico
                    await _sender.SendMessageAsync(message);
                }

                Console.WriteLine("Concluído o envio das mensagens.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.GetType().FullName} | Mensagem: {ex.Message}");
            }
            finally
            {
                // Fechando o sender e o cliente
                await _sender.CloseAsync();
                await _client.DisposeAsync();
                Console.WriteLine("Finalizando conexão com o Azure Service Bus.");
            }
        }
    }
}