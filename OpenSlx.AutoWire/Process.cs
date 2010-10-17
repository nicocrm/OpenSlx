using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OpenSlx.AutoWire
{
    public static class Process
    {
        public static void ProcessAssembly(String assemblyPath, String projectPath)
        {
            Assembly asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            //asm.GetCustomAttributes();
        }
    }
}
