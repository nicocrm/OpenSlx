using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenSlx.Lib.Utility;
using Sage.Entity.Interfaces;
using Sage.SalesLogix.Entities;

namespace OpenSlx.Lib.UnitTest
{
    [TestFixture]
    public class TestReflectionHelper
    {
        [Test]
        public void TestFindComplexProperty()
        {
            Ticket t = new Ticket();
            t.Account = new Account();
            t.Account.AccountName = "XXX";
            //t.Account.Id = "XXX"; 
            Assert.AreEqual("XXX", ReflectionHelper.GetPropertyValue(t, "Account.AccountName", null));
            Assert.IsNull(ReflectionHelper.GetPropertyValue(t, "Account.Id", null));
            //Assert.IsNotNull(ReflectionHelper.FindPropertyOnEntity(typeof(ITicket), "Account.Id", null));
        }
    }
}
