using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DownloadFenBiPDF
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("application.json");

#if DEBUG
            Assembly assembly = typeof(Program).Assembly;
            if (assembly != null)
                configurationBuilder.AddUserSecrets(assembly, true);
#endif
            var conf = configurationBuilder.Build();
            var cookie = conf["FenBiCookie"];
            if (string.IsNullOrEmpty(cookie))
            {
                throw new Exception("cookie不可为空");
            }
            var success = int.TryParse(conf["DownloadCount"],out var count);
            if (!success)
            {
                throw new Exception("count不正确");
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", cookie);

            if (!await CheckCookieAsync(client))
            {
                throw new Exception("cookie无效");
            }

            var taskList = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                taskList.Add(DownloadRandomPDF(client, i));
            }

            await Task.WhenAll(taskList);

            Console.WriteLine("按任何键退出");
            Console.ReadKey();

        }

        private static async Task DownloadRandomPDF(HttpClient client, int count)
        {
            var id = await GetPageIdAsync(client);
            await DownloadPDFAsync(client, id, count);
        }

        /// <summary>
        /// 获取一个智能组卷的id
        /// </summary>
        /// <param name="client">复用httpclient</param>
        /// <returns>智能组卷id</returns>
        private static async Task<int> GetPageIdAsync(HttpClient client)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent("2"), "type" },
                { new StringContent("2"), "exerciseTimeMode" }
            };

            var response = await client.PostAsync("https://tiku.fenbi.com/api/sydw/exercises?app=web&kav=12&version=3.0.0.0",content);
            var result = await response.Content.ReadAsStringAsync();



            using JsonDocument jsonDocument = JsonDocument.Parse(result);
            var id = jsonDocument.RootElement.GetProperty("id");

            //System.Text.Json很严格，是数字类型就不能获取为字符串
            return id.GetInt32();

        }

        /// <summary>
        /// 下载pdf到程序根目录
        /// </summary>
        /// <param name="client">复用httpclient</param>
        /// <param name="id">智能组卷的id</param>
        /// <param name="fileCount">当前循环次数，用于给下载的pdf编号</param>
        /// <returns></returns>
        private static async Task DownloadPDFAsync(HttpClient client,int id,int fileCount)
        {
            string downloadUrl = $"https://urlimg.fenbi.com/api/pdf/tiku/sydw/exercise/{id}?app=web&kav=12&version=3.0.0.0";
            //因为是pdf文件，所以不能用GetStream而应该用GetByte
            var bytes = (await client.GetByteArrayAsync(downloadUrl));
            var fileName = $"试卷{DateTime.Now:MM-dd}-{fileCount}.pdf";
            using var stream = new FileStream(fileName, FileMode.Create);
            //写入内存中的byte[]建议用这种方法，byte[]直接转成ReadOnlyMemory<byte>
            await stream.WriteAsync(bytes);
        }

        private static async Task<bool> CheckCookieAsync(HttpClient client)
        {

            var response = await client.GetAsync(@"https://tiku.fenbi.com/api/users/info?app=web&kav=12&version=3.0.0.0");
            return response.StatusCode==HttpStatusCode.OK;

        }
    }
}
