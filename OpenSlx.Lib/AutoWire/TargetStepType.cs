using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSlx.Lib.AutoWire
{
    /// <summary>
    /// Indicates whether the method executes as a pre, post, or main step.
    /// </summary>
    public enum TargetStepType
    {
        Primary,
        PreExecute,        
        PostExecute,
        PostFlush
    }
}
