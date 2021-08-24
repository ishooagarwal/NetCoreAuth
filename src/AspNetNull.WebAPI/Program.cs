using AspNetNull.Persistance.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text;

namespace AspNetNull.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureAppConfiguration(builder =>
            {
                string path = Directory.GetCurrentDirectory() + "../../../tools";

                string[] files = System.IO.Directory.GetFiles(path, "appsettings." + "*.json");
                foreach (var item in files)
                {
                    builder.AddJsonFile(item);
                }

                files = System.IO.Directory.GetFiles(path, "secure.appsettings" + "*.json");
                foreach (var item in files)
                {
                    string stream = File.ReadAllText(item);
                    string encryptedFile = Cryptography.Encrypt(stream);
                    string decryptedJson = Cryptography.Decrypt(stream);
                    builder.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(decryptedJson)));
                }
                builder.AddEnvironmentVariables();
            });
    }
}
