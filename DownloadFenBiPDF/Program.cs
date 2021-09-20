using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DownloadFenBiPDF
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("application.json");

            Assembly assembly = typeof(Program).Assembly;
            if (assembly != null)
                configurationBuilder.AddUserSecrets(assembly, true);

            var conf = configurationBuilder.Build();
            var cookie = conf.GetSection("FenBiCookie").Value;
            var count = int.Parse(conf.GetSection("DownloadCount").Value);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", cookie);


            for (int i = 0; i < count; i++)
            {
                var id = await GetPageIdAsync(client);
                await DownloadPDFAsync(client, id, i);
            }

        }
        static readonly string cookie = "sid=-259474955386604970; persistent=o/YRwZKRcwvNKpuKT1Nmv4vQeLARdomaDu1fJpD4YDwBlW84cw4vy1rLVoBCLzGtIHhCJePwWK749OF/GACiGw==; userid=60759081; sess=wan9KUQGRQ7ijJ/B7S7KT5ngwCZAfpABZgv6iPmyL1dIcg1lWtXeCeI7aReo6Nf6TEXXtl+zXIP1Q1oJTizudgJlzyQ9yzmZziY/DKm5nsc=";

        private static async Task<string> GetPageIdAsync(HttpClient client)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://tiku.fenbi.com/api/sydw/exercises?app=web&kav=12&version=3.0.0.0"),
                Method = HttpMethod.Post
            };


            var content = new MultipartFormDataContent
            {
                { new StringContent("2"), "type" },
                { new StringContent("2"), "exerciseTimeMode" }
            };
            request.Content = content;

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            using JsonDocument jsonDocument = JsonDocument.Parse(result);
            var id = jsonDocument.RootElement.GetProperty("id");

            return id.GetInt32().ToString();

        }

        private static async Task DownloadPDFAsync(HttpClient client,string id,int fileCount)
        {
            string downloadUrl = $"https://urlimg.fenbi.com/api/pdf/tiku/sydw/exercise/{id}?app=web&kav=12&version=3.0.0.0";
            var bytes = (await client.GetByteArrayAsync(downloadUrl));
            var fileName = $"试卷{DateTime.Now:MM-dd}-{fileCount}.pdf";
            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                file.Delete();
            }
            using var stream = file.Open(FileMode.Create);
            //using var stream = new FileStream(fileName, FileMode.Create);
            await stream.WriteAsync(bytes);
        }
    }
}
