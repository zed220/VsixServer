using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommonLib;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using Task = System.Threading.Tasks.Task;

namespace VSIXServer {
    [Guid("4324BA11-6422-4323-B8FC-390DD0A7B0BF")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("VSIX Server", "VSIX Server from Petr Zinovyev", "1.0")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSPackage : AsyncPackage {
        const string serverAddress = "http://localhost:8080/vsix/";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
            var server = new WebServer(serverAddress, Responce);
            server.Run();
        }

        VsixResponse Responce(VsixSend message) {
            switch (message.ActionType) {
                case ActionType.ListProjects: return new VsixResponse(string.Join(",", GetAllProjects().GetAwaiter().GetResult()));
                case ActionType.AddFile: return new VsixResponse(AddFile(message.Parameters[0], message.Parameters[1]).GetAwaiter().GetResult().ToString());
                case ActionType.AddReference: return new VsixResponse(AddReference(message.Parameters[0]).GetAwaiter().GetResult().ToString());
            }
            return new VsixResponse("true");
        }
        async Task<List<string>> GetAllProjects() {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var result = new List<string>();
            for (int p = 1; p <= dte.Solution.Projects.Count; p++) {
                var project = dte.Solution.Projects.Item(p);
                result.Add(project.Name);
            }
            return result;
        }
        async Task<bool> AddFile(string fileName, string content) {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var project = dte.Solution.Projects.Item(1);
            for (int f = 1; f <= project.ProjectItems.Count; f++) {
                var file = project.ProjectItems.Item(f);
                if (file.Name == fileName)
                    return false;
            }
            var solution2 = (EnvDTE80.Solution2)dte.Solution;
            var projectItemTemplate = solution2.GetProjectItemTemplate("Text File", "CSharp");
            project.ProjectItems.AddFromTemplate(projectItemTemplate, fileName);
            dte.ActiveDocument.Close(EnvDTE.vsSaveChanges.vsSaveChangesYes);
            for (int f = 1; f <= project.ProjectItems.Count; f++) {
                var file = project.ProjectItems.Item(f);
                if (file.Name == fileName) {
                    System.IO.File.WriteAllText(file.Properties.Item(18).Value.ToString(), "Some text");
                    break;
                }
            }
            return true;
        }
        async Task<bool> AddReference(string referencePath) {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var project = dte.Solution.Projects.Item(1);
            var p = (VSProject)project.Object;
            p.References.Add(referencePath);
            return true;
        }
    }
}
