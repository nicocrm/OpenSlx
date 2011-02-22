using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSlx.Lib.AutoWire;
using Sage.Entity.Interfaces;

namespace OpenSlx.AutoWire.Demo
{
    public class Class1
    {
        [BusinessRule(StepType = TargetStepType.Primary)]
        public static void Test(IAccount account)
        {
        }

        [BusinessRule(StepType = TargetStepType.Primary)]
        public static void TestWithOneParam(IAccount account, String param1)
        {
        }

        [BusinessRule(StepType = TargetStepType.Primary)]
        public static void TestWithReturnType(IAccount account, String param1, out String returnValue)
        {
            returnValue = "";
        }

        [OnBeforeInsert(StepType = TargetStepType.PostExecute)]
        public static void TestBeforeUpdate(IAccount account, object session)
        {
        }
    }
}
