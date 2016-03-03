using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenSlx.Lib.Utility;
using Sage.SalesLogix.Entities;

namespace OpenSlx.Lib.UnitTest
{
    [TestFixture]
    public class TestSlxEntityUtility
    {
        [Test]
        public void TestCloneEntity()
        {
            Opportunity opp = new Opportunity();
            opp.Account = new Account() {AccountName = "Testing"};
            var target = SlxEntityUtility.CloneEntity(opp);
            Assert.AreEqual(target.Account, opp.Account);            
        }
    }
}
