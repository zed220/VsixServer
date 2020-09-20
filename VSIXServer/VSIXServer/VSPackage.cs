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

        public VSPackage() {
            new HttpServerLite(serverAddress, Responce).Start();
        }

        async Task<VsixResponse> Responce(VsixSend message) {
            switch (message.ActionType) {
                case ActionType.ListProjects: return new VsixResponse(string.Join(",", await GetAllProjectsAsync()));
                case ActionType.AddFile: return new VsixResponse((await AddFileAsync(message.Parameters[0], message.Parameters[1])).ToString());
                case ActionType.AddReference: return new VsixResponse((await AddReferenceAsync(message.Parameters[0])).ToString());
                default: throw new ArgumentOutOfRangeException();
            }
        }

        async Task<EnvDTE.DTE> PrepareEnvironment() {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            return await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }


        async Task<List<string>> GetAllProjectsAsync() {
            var dte = await PrepareEnvironment();
            var result = new List<string>();
            for (var p = 1; p <= dte.Solution.Projects.Count; p++) {
                var project = dte.Solution.Projects.Item(p);
                result.Add(project.Name);
            }
            return result;
        }
        async Task<bool> AddFileAsync(string fileName, string content) {
            var dte = await PrepareEnvironment();
            var project = dte.Solution.Projects.Item(1);
            for (var f = 1; f <= project.ProjectItems.Count; f++) {
                var file = project.ProjectItems.Item(f);
                if (file.Name == fileName)
                    return false;
            }
            var solution2 = (EnvDTE80.Solution2)dte.Solution;
            var projectItemTemplate = solution2.GetProjectItemTemplate("Text File", "CSharp");
            try {
                project.ProjectItems.AddFromTemplate(projectItemTemplate, fileName);
            }
            catch {
                return false;
            }
            dte.ActiveDocument.Close(EnvDTE.vsSaveChanges.vsSaveChangesYes);
            project.Save();
            for (var i = 1; i <= project.ProjectItems.Count; i++) {
                var item = project.ProjectItems.Item(i);
                if (item.Name == fileName) {
                    for (var p = 1; p <= item.Properties.Count; p++) {
                        var property = item.Properties.Item(p);
                        if (property.Name == "FullPath") {
                            System.IO.File.WriteAllText(property.Value.ToString(), content);
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }
        async Task<bool> AddReferenceAsync(string referencePath) {
            var dte = await PrepareEnvironment();
            var project = dte.Solution.Projects.Item(1);
            var p = (VSProject)project.Object;
            for (var r = 1; r <= p.References.Count; r++) {
                var reference = p.References.Item(r);
                if (reference.Path == referencePath)
                    return false;
            }
            p.References.Add(referencePath);
            project.Save();
            return true;
        }
    }
}
