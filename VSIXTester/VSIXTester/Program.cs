using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VSIXTester {
    class Program {
        const string serverAddress = "http://localhost:8080/vsix";

        static void Main(string[] args) {
            var result = PostCallAPI(new VsixSend(ActionType.AddFile)).GetAwaiter().GetResult();
        }

        static async Task<object> PostCallAPI(VsixSend jsonObject) {
            try {
                using (HttpClient client = new HttpClient()) {
                    var content = new StringContent(JsonConvert.SerializeObject(jsonObject), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(serverAddress, content);
                    if (response != null) {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<VsixResponse>(jsonString);
                    }
                }
            }
            catch (Exception ex) {
            }
            return null;
        }
    }
}
