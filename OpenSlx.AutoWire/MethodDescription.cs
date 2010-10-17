using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSlx.Lib.AutoWire;
using System.Xml.Linq;

namespace OpenSlx.AutoWire
{
    /// <summary>
    /// A business rule or event ready to be written.
    /// </summary>
    public class MethodDescription
    {
        public String Name;
        public String Description;
        public String TargetType;
        public StepType StepType;
        
        public void WriteToProject(String modelRoot)
        {
            XDocument def;
        }


    }
}
