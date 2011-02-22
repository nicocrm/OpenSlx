using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSlx.Lib.AutoWire
{
    /// <summary>
    /// Used to identify an assembly that has AutoWire attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AutoWireAttribute : Attribute
    {
    }
}
