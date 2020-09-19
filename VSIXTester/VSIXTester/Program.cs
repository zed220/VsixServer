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
        const string serverAddress = "http://localhost:8080/vsix/";

        static void Main(string[] args) {
            var result = PostCallAPI(new VsixSend(ActionType.ListProjects, new List<string>())).GetAwaiter().GetResult();
            Console.WriteLine("Loaded projects:");
            foreach (var p in result.Result.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
                Console.WriteLine(p);
            }
            Console.WriteLine("Adding file: sample.sam with custom content");
            result = PostCallAPI(new VsixSend(ActionType.AddFile, new List<string>() { "sample.sam", "I am custom content" })).GetAwaiter().GetResult();
            if(Convert.ToBoolean(result.Result))
                Console.WriteLine("File added");
            else
                Console.WriteLine("Cannot add file");
            Console.WriteLine("Adding dll reference: Accessibility");
            result = PostCallAPI(new VsixSend(ActionType.AddReference, new List<string>() { "Accessibility" })).GetAwaiter().GetResult();
            if (Convert.ToBoolean(result.Result))
                Console.WriteLine("Reference added");
            else
                Console.WriteLine("Cannot add reference");
        }

        static async Task<VsixResponse> PostCallAPI(VsixSend jsonObject) {
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
