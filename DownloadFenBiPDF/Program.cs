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
        /// <summary>
        /// 获取一个智能组卷的id
        /// </summary>
        /// <param name="client">复用httpclient</param>
        /// <returns>智能组卷id</returns>
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

        /// <summary>
        /// 下载pdf到程序根目录
        /// </summary>
        /// <param name="client">复用httpclient</param>
        /// <param name="id">智能组卷的id</param>
        /// <param name="fileCount">当前循环次数，用于给下载的pdf编号</param>
        /// <returns></returns>
        private static async Task DownloadPDFAsync(HttpClient client,string id,int fileCount)
        {
            string downloadUrl = $"https://urlimg.fenbi.com/api/pdf/tiku/sydw/exercise/{id}?app=web&kav=12&version=3.0.0.0";
            //因为是pdf文件，所以不能用GetStream而应该用GetByte
            var bytes = (await client.GetByteArrayAsync(downloadUrl));
            var fileName = $"试卷{DateTime.Now:MM-dd}-{fileCount}.pdf";
            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                file.Delete();
            }
            using var stream = file.Open(FileMode.Create);
            //using var stream = new FileStream(fileName, FileMode.Create);
            //写入内存中的byte[]建议用这种方法，byte[]直接转成ReadOnlyMemory<byte>
            await stream.WriteAsync(bytes);
        }
    }
}
