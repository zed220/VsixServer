using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VSIXTester {
    static class TestHelper {
        const string serverAddress = "http://localhost:8080/vsix/";

        public static async Task DoWithConsoleLogAsync(string messageStart, Func<VsixSend> func, Func<VsixResponse, string> getMessageComplete, string messageError) {
            Console.WriteLine(messageStart);
            try {
                var result = await DoWithServerAsync(func());
                Console.WriteLine(getMessageComplete(result));
            }
            catch (Exception e) {
                Console.WriteLine(messageError);
                Console.WriteLine(e.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        static async Task<VsixResponse> DoWithServerAsync(VsixSend jsonObject) {
            using (HttpClient client = new HttpClient()) {
                var content = new StringContent(JsonConvert.SerializeObject(jsonObject), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(serverAddress, content);
                if (response != null) {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<VsixResponse>(jsonString);
                }
            }
            throw new HttpRequestException("Empty response");
        }
    }
}
