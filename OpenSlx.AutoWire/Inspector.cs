using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Sage.Platform.ComponentModel;
using OpenSlx.Lib.AutoWire;

namespace OpenSlx.AutoWire
{
    public class Inspector : MarshalByRefObject
    {
        private String _deploymentFolder = null;

        public void InitializeDomain(String deploymentFolder)
        {
            _deploymentFolder = deploymentFolder;
            Assembly.ReflectionOnlyLoad("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
        }

        Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            String asmPath = Path.Combine(_deploymentFolder, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(asmPath))
                return Assembly.ReflectionOnlyLoadFrom(asmPath);
            // if it fails, try to load from current folder
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        public List<MethodDescription> CollectMethodDescriptionsForApp(String basePath)
        {
            List<MethodDescription> result = new List<MethodDescription>();
            foreach (String file in Directory.GetFiles(Path.Combine(basePath, "Bin")))
            {
                Console.WriteLine(file);
                String fileName = Path.GetFileName(file);
                if (!fileName.StartsWith("Sage"))
                    CollectMethodDescriptionsForAssembly(file, result);
            }
            return result;
        }

        /// <summary>
        /// Check for custom "AutoWire" attributes added to any (static) method in the assembly, and populate corresponding 
        /// descriptions.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="result"></param>
        private static void CollectMethodDescriptionsForAssembly(string file, List<MethodDescription> result)
        {
            Assembly asm;
            try
            {
                asm = Assembly.ReflectionOnlyLoadFrom(file);
            }
            catch (Exception x)
            {
                //    LOG.Warn("Failed to load assembly from file " + file, x);
                return;
            }
            if (CustomAttributeData.GetCustomAttributes(asm).Count(x => x.Constructor.DeclaringType.FullName == typeof(AutoWireAttribute).FullName) == 0)
                return;
            Type componentRefType = Type.ReflectionOnlyGetType("Sage.Platform.ComponentModel.IComponentReference, Sage.Platform", false, false);
            foreach (Type targetType in asm.GetTypes())
            {
                foreach (MethodInfo method in targetType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    ParameterInfo[] methodParams = method.GetParameters();
                    if (methodParams.Length == 0 || !componentRefType.IsAssignableFrom(methodParams[0].ParameterType))
                        continue;
                    String entityType = methodParams[0].ParameterType.Name.Substring(1);  // chop initial "I" from interface name

                    foreach (CustomAttributeData attr in CustomAttributeData.GetCustomAttributes(method))
                    {
                        Type attrType = attr.Constructor.DeclaringType;
                        if (!attrType.FullName.StartsWith("OpenSlx.Lib.AutoWire"))
                            continue;
                        MethodDescription newMethod = new MethodDescription
                                {
                                    Name = method.Name,
                                    TargetEntity = entityType,
                                    DeclaringType = FormatType(targetType)
                                };
                        // set method type and default step type according to the type of rule we have
                        switch (attrType.Name)
                        {
                            case "BusinessRuleAttribute":
                                newMethod.StepType = TargetStepType.Primary;
                                break;
                            case "OnBeforeInsertAttribute":
                                newMethod.MethodType = Sage.Platform.Orm.Entities.MethodType.CrudEvent;
                                newMethod.StepType = TargetStepType.PostExecute;
                                newMethod.EventType = EventType.BeforeInsert;
                                break;
                            case "OnBeforeUpdateAttribute":
                                newMethod.MethodType = Sage.Platform.Orm.Entities.MethodType.CrudEvent;
                                newMethod.StepType = TargetStepType.PostExecute;
                                newMethod.EventType = EventType.BeforeUpdate;
                                break;
                            case "OnBeforeDeleteAttribute":
                                newMethod.MethodType = Sage.Platform.Orm.Entities.MethodType.CrudEvent;
                                newMethod.StepType = TargetStepType.PostExecute;
                                newMethod.EventType = EventType.BeforeDelete;
                                break;
                            case "OnAfterUpdateAttribute":
                                newMethod.MethodType = Sage.Platform.Orm.Entities.MethodType.CrudEvent;
                                newMethod.StepType = TargetStepType.PostExecute;
                                newMethod.EventType = EventType.AfterUpdate;
                                break;
                            case "OnAfterInsertAttribute":
                                newMethod.MethodType = Sage.Platform.Orm.Entities.MethodType.CrudEvent;
                                newMethod.StepType = TargetStepType.PostExecute;
                                newMethod.EventType = EventType.AfterInsert;
                                break;
                        }
                        foreach (CustomAttributeNamedArgument arg in attr.NamedArguments)
                        {
                            switch (arg.MemberInfo.Name)
                            {
                                case "StepType":
                                    newMethod.StepType = (TargetStepType)arg.TypedValue.Value;
                                    break;
                                case "Description":
                                    newMethod.Description = (String)arg.TypedValue.Value;
                                    break;
                            }
                        }
                        List<ParamDescription> newParams = new List<ParamDescription>();
                        for (int i = 1; i < methodParams.Length; i++)
                        {
                            if (methodParams[i].IsOut)
                            {
                                newMethod.ReturnType = methodParams[i].ParameterType.FullName;
                            }
                            else
                            {
                                newParams.Add(new ParamDescription
                                {
                                    ParamType = methodParams[i].ParameterType.FullName,
                                    Name = methodParams[i].Name
                                });
                            }
                        }
                        newMethod.ParameterTypes = newParams.ToArray();


                        result.Add(newMethod);
                    }
                }
            }
        }

        private static string FormatType(Type targetType)
        {
            return String.Format("{0}, {1}", targetType.FullName, targetType.Assembly.GetName().Name);
        }
    }
}
