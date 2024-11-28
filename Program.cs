using livedio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Livedio
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Live Dio - Service Bus");
            await StartMenuAsync();
        }

        private static async Task StartMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Escolha uma opção:");
                Console.WriteLine("1: Produzir mensagens na fila");
                Console.WriteLine("2: Produzir mensagens no tópico");
                Console.WriteLine("3: Ler mensagens (Inicia consumidores)");
                Console.WriteLine("0: Sair");

                var resposta = Console.ReadLine();

                switch (resposta)
                {
                    case "1":
                        await new ProdutorFila().ProduzirMensagem();
                        break;
                    case "2":
                        await new ProdutorTopico().ProduzirMensagem();
                        break;
                    case "3":
                        CreateHostBuilder().Build().Run();
                        return; // Para evitar o loop após iniciar os consumidores
                    case "0":
                        Console.WriteLine("Encerrando...");
                        return;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }

                Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
                Console.ReadKey();
            }
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, config) =>
                {
                    // Adiciona o arquivo appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumidorFilaMorta>();
                    services.AddHostedService<ConsumidorTopico>();
                });
    }
}

    
