using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Sage.Platform.Application;
using Sage.Platform.Projects.Interfaces;
using Sage.Platform.Orm.Entities;
using Sage.Platform.WebPortal.Design;
using System.IO;
using OpenSlx.Lib.AutoWire;
using Sage.Platform.ComponentModel;
using log4net;
using System.Runtime.Remoting;

namespace OpenSlx.AutoWire
{
    public static class Process
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Process));
        private const String AUTOWIRE_HEADER = " ** Method added by OpenSlx.AutoWire ** \n\n";

        public static void ProcessCurrentProject()
        {
            IProject project = ApplicationContext.Current.Services.Get<IProjectContextService>().ActiveProject;
            List<MethodDescription> methodsToCreate = CollectMethodDescriptions(project.Models.Get<PortalModel>());
            if (methodsToCreate.Count == 0)
            {
                LOG.Info("No auto-wire method found");
            }

            OrmModel model = project.Models.Get<OrmModel>();
            foreach (OrmPackage package in model.Packages)
            {
                foreach (OrmEntity entity in package.Entities)
                {
                    List<MethodDescription> methodsForEntity = methodsToCreate.Where(x => x.TargetEntity == entity.Name).ToList();
                    bool modified = false;

                    modified = CleanAutowireMethods(entity, methodsForEntity);
                    foreach (MethodDescription method in methodsForEntity)
                    {
                        AddMethodToEntity(entity, method);
                        modified = true;
                    }
                    if (modified)
                        entity.Save();
                }
            }
        }

        /// <summary>
        /// Add a method to the definition for the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="method"></param>
        private static void AddMethodToEntity(OrmEntity entity, MethodDescription method)
        {
            OrmEntityMethod targetMethod = null;
            targetMethod = method.CreateMethod(entity);
            OrmMethodTarget targetStep = method.CreateTarget(AUTOWIRE_HEADER);
            TargetMethodCollection targetColl = null;
            switch (method.StepType)
            {
                case TargetStepType.PreExecute:
                    targetColl = targetMethod.PreExecuteTargets;
                    break;
                case TargetStepType.PostFlush:
                    targetColl = targetMethod.PostFlushTargets;
                    break;
                case TargetStepType.PostExecute:
                    targetColl = targetMethod.PostExecuteTargets;
                    break;
                case TargetStepType.Primary:
                default:
                    targetColl = targetMethod.MethodTargets;
                    break;
            }
            targetColl.Add(targetStep);
            entity.Methods.Add(targetMethod);
        }

        /// <summary>
        /// Any method that is on the entity and is NOT in the methodsToCreate should be removed 
        /// (this also considers the step type, eg, if the method is specified as Post, and the current one is a Pre, it will be removed)
        /// Any method that is on the entity is IS in the methodsToCreate should be removed from methodsToCreate.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>true if entity was modified as a result</returns>
        private static bool CleanAutowireMethods(OrmEntity entity, List<MethodDescription> methodsToCreate)
        {
            List<OrmEntityMethod> methodsToRemove = new List<OrmEntityMethod>();
            bool modified = false;
            foreach (OrmEntityMethod method in entity.Methods.Union(new OrmEntityMethod[] { 
                entity.OnAfterInsertMethod, entity.OnAfterDeleteMethod, entity.OnBeforeDeleteMethod, 
                entity.OnBeforeInsertMethod, entity.OnBeforeUpdateMethod, entity.OnAfterUpdateMethod }))
            {
                if (method == null)
                    // happens for the "event" methods - some of them are null
                    continue;
                modified = CleanAutowireMethodsTarget(TargetStepType.Primary, method.MethodTargets, methodsToCreate) || modified;
                modified = CleanAutowireMethodsTarget(TargetStepType.PreExecute, method.PreExecuteTargets, methodsToCreate) || modified;
                modified = CleanAutowireMethodsTarget(TargetStepType.PostExecute, method.PostExecuteTargets, methodsToCreate) || modified;
                modified = CleanAutowireMethodsTarget(TargetStepType.PostFlush, method.PostFlushTargets, methodsToCreate) || modified;

                if (method.AllTargets.Count() == 0)
                    methodsToRemove.Add(method);
            }
            foreach (OrmEntityMethod method in methodsToRemove)
            {
                if (entity.Methods.Contains(method))
                {
                    modified = true;
                    method.Delete();
                    entity.Methods.Remove(method);
                }
            }
            return modified;
        }

        private static bool CleanAutowireMethodsTarget(TargetStepType targetStepType, TargetMethodCollection targetColl, List<MethodDescription> methodsToCreate)
        {
            List<OrmMethodTarget> targetsToRemove = new List<OrmMethodTarget>();
            foreach (OrmMethodTarget target in targetColl)
            {
                if (IsOpenSlxTarget(target))
                {
                    MethodDescription targetMatch = methodsToCreate.FirstOrDefault(x => x.MatchTarget(target, targetStepType, AUTOWIRE_HEADER));
                    if (targetMatch != null)
                    {
                        methodsToCreate.Remove(targetMatch);
                    }
                    else
                    {
                        targetsToRemove.Add(target);
                    }
                }
            }
            foreach (OrmMethodTarget target in targetsToRemove)
            {
                targetColl.Remove(target);
            }

            return targetsToRemove.Count > 0;
        }

        private static bool IsOpenSlxTarget(OrmMethodTarget target)
        {
            return target.Description != null && target.Description.Contains(AUTOWIRE_HEADER);
        }


        private static List<MethodDescription> CollectMethodDescriptions(PortalModel portalModel)
        {
            AppDomain reflectionDomain = AppDomain.CreateDomain("OpenSlx.AutoWire.Inspector Domain", null, null);

            Inspector inspectorProxy = (Inspector)reflectionDomain.CreateInstanceAndUnwrap("OpenSlx.AutoWire", "OpenSlx.AutoWire.Inspector");
            inspectorProxy.InitializeDomain(portalModel.Project.Drive.RootDirectory + "\\deployment\\common\\bin");
            List<MethodDescription> result = new List<MethodDescription>();
            foreach (PortalApplication app in portalModel.PortalApplications)
            {
                List<MethodDescription> appMethods = inspectorProxy.CollectMethodDescriptionsForApp(app.SupportFilesDefinition.ResolvedProjectPath);
                foreach (MethodDescription method in appMethods)
                {
                    if (result.Count(m => m.Equals(method)) == 0)
                        result.Add(method);
                }
            }
            AppDomain.Unload(reflectionDomain);
            return result;
        }

    }
}
