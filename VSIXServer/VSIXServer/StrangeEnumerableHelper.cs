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
    static class StrangeEnumerableHelper {
        public static IEnumerable<T> AsGeneric<TEnumerable, T>(this TEnumerable source, Func<int> getCount, Func<object, T> getElement) where TEnumerable : IEnumerable {
            var count = getCount();
            for (var i = 1; i <= count; i++)
                yield return getElement(i);
        }
        public static IEnumerable<Project> GetProjects(this EnvDTE.DTE dte) {
            var projects = dte.Solution.Projects;
            return projects.AsGeneric<Projects, Project>(() => projects.Count, projects.Item);
        }
    }
}
