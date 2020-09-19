using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib {
    public class VsixSend {
        public VsixSend() { }

        public VsixSend(ActionType actionType) {
            ActionType = actionType;
        }

        public VsixSend(ActionType actionType, List<string> parameters)
        : this(actionType) {
            Parameters = parameters;
        }

        public ActionType ActionType { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
    }
    public enum ActionType {
        ListProjects,
        AddFile,
        AddReference,
    }

    public class VsixResponse {
        public VsixResponse() { }

        public VsixResponse(string result) {
            Result = result;
        }

        public string Result { get; set; }
    }
}
