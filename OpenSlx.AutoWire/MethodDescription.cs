using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSlx.Lib.AutoWire;
using System.Xml.Linq;
using Sage.Platform.Orm.Entities;

namespace OpenSlx.AutoWire
{
    /// <summary>
    /// A business rule or event ready to be written.
    /// </summary>
    [Serializable]
    public class MethodDescription
    {
        /// <summary>
        /// Full name for the declaring type
        /// </summary>
        public String DeclaringType;
        /// <summary>
        /// Method name (for a business rule)
        /// </summary>
        public String Name;
        /// <summary>
        /// Description (to be used as comment in AA)
        /// </summary>
        public String Description;
        /// <summary>
        /// Entity type this applies to.
        /// This is only the last part of the name eg IContact.
        /// </summary>
        public String TargetEntity;
        /// <summary>
        /// Whether this is a pre, post, primary step.
        /// If this is specified as a primary step 
        /// </summary>
        public TargetStepType StepType;
        /// <summary>
        /// Type returned by a method - this is always the last paramter, and must be defined as an output parameter
        /// </summary>
        public String ReturnType;
        /// <summary>
        /// Other input parameters
        /// </summary>
        public ParamDescription[] ParameterTypes;

        public MethodType MethodType = MethodType.Rule;

        public EventType EventType;

        public override bool Equals(object obj)
        {
            MethodDescription right = obj as MethodDescription;
            if (right == null)
                return false;
            return Name == right.Name &&
                TargetEntity == right.TargetEntity &&
                StepType == right.StepType &&
                DeclaringType == right.DeclaringType;
        }

        public bool MatchTarget(OrmMethodTarget target, TargetStepType targetStepType, String descriptionPrefix)
        {
            return targetStepType == StepType &&
                descriptionPrefix + Description == target.Description &&
                target.TargetMethod == Name &&
                target.TargetType == DeclaringType;
        }

        /// <summary>
        /// Create a new method or return existing method if appropriate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public OrmEntityMethod CreateMethod(OrmEntity entity)
        {
            if (MethodType == Sage.Platform.Orm.Entities.MethodType.Rule)
            {
                OrmEntityMethod method = new OrmEntityMethod
                {
                    ActionType = ActionType.None,
                    ReturnType = ReturnType,
                    MethodName = Name,
                    MethodType = MethodType
                };
                if (ParameterTypes != null && ParameterTypes.Length > 0)
                {
                    //MethodParameterCollection paramColl = new MethodParameterCollection(method);
                    foreach (ParamDescription p in ParameterTypes)
                    {
                        method.MethodParameters.Add(new OrmEntityMethodParam { ParamName = p.Name, ParamType = p.ParamType });
                    }
                }
                return method;
            }
            else if (MethodType == Sage.Platform.Orm.Entities.MethodType.CrudEvent)
            {
                switch (EventType)
                {
                    case AutoWire.EventType.BeforeUpdate:
                        return entity.OnBeforeUpdateMethod;
                    case AutoWire.EventType.AfterUpdate:
                        return entity.OnAfterUpdateMethod;
                    case AutoWire.EventType.BeforeInsert:
                        return entity.OnBeforeInsertMethod;
                    case AutoWire.EventType.AfterInsert:
                        return entity.OnAfterInsertMethod;
                    case AutoWire.EventType.BeforeDelete:
                        return entity.OnBeforeDeleteMethod;
                    default:
                        throw new Exception("Invalid event type " + EventType + " for AutoWire method " + DeclaringType + "." + Name);
                }
            }
            else
            {
                throw new Exception("Unsupported method type " + MethodType + " for AutoWire method " + DeclaringType + "." + Name);
            }
        }

        public OrmMethodTarget CreateTarget(String descriptionPrefix)
        {
            return new OrmMethodTarget
            {
                Description = descriptionPrefix + Description,
                Active = true,
                TargetType = DeclaringType,
                TargetMethod = Name
            };
        }
    }
}
