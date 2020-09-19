using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommonLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VSIXServer {
    [Guid("4324BA11-6422-4323-B8FC-390DD0A7B0BF")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("VSIX Server", "VSIX Server from Petr Zinovyev", "1.0")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSPackage : AsyncPackage {
        const string serverAddress = "http://localhost:8080/vsix/";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
            //await JoinableTaskFactory.SwitchToMainThreadAsync();
            var server = new WebServer(serverAddress, Responce);
            server.Run();
            //if(await IsSolutionLoadedAsync())
            //    HandleOpenSolution();

            // Listen for subsequent solution events
            //SolutionEvents.OnAfterBackgroundSolutionLoadComplete += HandleOpenSolution;
        }

        VsixResponse Responce(VsixSend message) {
            return new VsixResponse(true);
        }

        //private async Task<bool> IsSolutionLoadedAsync() {
        //    await JoinableTaskFactory.SwitchToMainThreadAsync();
        //    var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

        //    ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

        //    return value is bool isSolOpen && isSolOpen;
        //}

        //async void HandleOpenSolution(object sender = null, EventArgs e = null) {
        //    await JoinableTaskFactory.SwitchToMainThreadAsync();
        //    var solService = await GetServiceAsync(typeof(SVsSolution)) as SVsSolution;
        //    var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        //    for (int p = 1; p <= dte.Solution.Projects.Count; p++) {
        //        var project = dte.Solution.Projects.Item(p);
        //        Debug.WriteLine(project.Name);
        //        var solution2 = (EnvDTE80.Solution2)dte.Solution;
        //        var projectItemTemplate = solution2.GetProjectItemTemplate("Text File", "CSharp");
        //        project.ProjectItems.AddFromTemplate(projectItemTemplate, "SampleFilte.sample");
        //        dte.ActiveDocument.Close(EnvDTE.vsSaveChanges.vsSaveChangesYes);
        //        for (int f = 1; f <= project.ProjectItems.Count; f++) {
        //            var file = project.ProjectItems.Item(f);
        //            if (file.Name == "SampleFilte.sample") {
        //                System.IO.File.WriteAllText(file.Properties.Item(18).Value.ToString(), "Some text");
        //            }
        //        }
        //    }
        //}
    }
}
