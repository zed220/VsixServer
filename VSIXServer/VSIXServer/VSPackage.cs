using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            dte.GetProjects().Select(p => p.Name).ToList().ForEach(result.Add);
            return result;
        }
        async Task<bool> AddFileAsync(string fileName, string content) {
            var dte = await PrepareEnvironment();
            var project = dte.GetProjects().FirstOrDefault();
            if (project == null)
                return false;
            var items = project.ProjectItems;
            var existFile = items.AsGeneric<ProjectItems, ProjectItem>(() => items.Count, items.Item).FirstOrDefault(f => f.Name == fileName);
            if (existFile != null)
                return false;
            if (!TryCreateAndAddFile(dte, project, fileName, out existFile))
                return false;
            return WriteFileContent(existFile, content);
        }
        bool TryCreateAndAddFile(EnvDTE.DTE dte, Project project, string fileName, out ProjectItem existFile) {
            existFile = null;
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
            var items = project.ProjectItems;
            existFile = items.AsGeneric<ProjectItems, ProjectItem>(() => items.Count, items.Item).FirstOrDefault(f => f.Name == fileName);
            return existFile != null;
        }
        bool WriteFileContent(ProjectItem existFile, string content) {
            var properties = existFile.Properties;
            var property = properties.AsGeneric<Properties, Property>(() => properties.Count, properties.Item).FirstOrDefault(p => p.Name == "FullPath");
            if (property == null)
                return false;
            try {
                System.IO.File.WriteAllText(property.Value.ToString(), content);
            }
            catch {
                return false;
            }
            return true;
        }

        async Task<bool> AddReferenceAsync(string referencePath) {
            var dte = await PrepareEnvironment();
            var project = dte.GetProjects().FirstOrDefault();
            if (project == null)
                return false;
            var vsProject = (VSProject)project.Object;
            var references = vsProject.References;
            var reference = references.AsGeneric<References, Reference>(() => references.Count, references.Item).FirstOrDefault(r => r.Path == referencePath);
            if (reference != null)
                return false;
            references.Add(referencePath);
            project.Save();
            return true;
        }
    }
}
