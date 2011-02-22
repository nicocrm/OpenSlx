using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSlx.Lib.AutoWire
{    
    public abstract class SlxMethodAttribute : Attribute
    {
        public TargetStepType StepType { get; set; }
        public String Description { get; set; }
    }
}
