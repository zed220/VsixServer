using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib {
    public class VsixSend {
        public VsixSend(ActionType actionType) {
            ActionType = actionType;
        }

        public ActionType ActionType { get; set; }
    }
    public enum ActionType {
        AddFile,
        AddReference,
    }

    public class VsixResponse {
        public VsixResponse(bool success) {
            Success = success;
        }

        public bool Success { get; set; }
    }
}
