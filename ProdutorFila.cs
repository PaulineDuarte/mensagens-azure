using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace livedio
{
    public class ProdutorFila
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

       
        public ProdutorFila()


        {
            // Inicialize o cliente e o remetente com a connection string 
            _client = new ServiceBusClient("connectionString"); // Substitua pela sua string de conexão do Azure Service Bus
            _sender = _client.CreateSender("fila1");
        }

        public async Task ProduzirMensagem()
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    string mensagem = $"Número {i}";
                    Console.WriteLine($"Enviando mensagem: {mensagem}");

                    // Cria uma mensagem e envia
                    ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(mensagem));
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
                // Libere os recursos
                await _sender.DisposeAsync();
                await _client.DisposeAsync();
                Console.WriteLine("Finalizando conexão com Service Bus.");
            }
        }
    }
}
