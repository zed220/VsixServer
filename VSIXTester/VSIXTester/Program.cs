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
        const string _FileName = "sample.sam";
        const string _FileContent = @"I am custom content";
        const string _ReferencePath = "Accessibility";

        static void Main(string[] args) {
            MainAsync().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        static async Task MainAsync() {
            await LoadProjectsListAsync();
            await AddFileAndModifyContentAsync(_FileName, _FileContent);
            await AddReferenceAsync(_ReferencePath);
        }

        static async Task LoadProjectsListAsync() {
            await TestHelper.DoWithConsoleLogAsync(
                "Start loading projects:",
                () => new VsixSend(ActionType.ListProjects, new List<string>()),
                r => {
                    var sb = new StringBuilder();
                    sb.AppendLine("Loaded projects:");
                    foreach (var p in r.Result.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        sb.AppendLine(p);
                    return sb.ToString();
                }, "Loading projects failed.");
        }
        static async Task AddFileAndModifyContentAsync(string fileName, string content) {
            await TestHelper.DoWithConsoleLogAsync(
                "Adding file: sample.sam with custom content:",
                () => new VsixSend(ActionType.AddFile, new List<string>() { fileName, content }),
                r => {
                    return Convert.ToBoolean(r.Result) ? $"File {fileName} added." : $"Cannot add file {fileName}.";
                }, $"Cannot add file {fileName} or modify it`s content.");
        }
        static async Task AddReferenceAsync(string referencePath) {
            await TestHelper.DoWithConsoleLogAsync(
                $"Adding dll reference {referencePath}:",
                () => new VsixSend(ActionType.AddReference, new List<string>() { referencePath }),
                r => {
                    return Convert.ToBoolean(r.Result) ? $"Reference {referencePath} added." : $"Cannot add reference {referencePath}.";
                }, $"Cannot add reference {referencePath}.");
        }
    }
}
