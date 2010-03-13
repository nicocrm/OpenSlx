using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NHibernate.Persister.Entity;
using NHibernate.Impl;
using Sage.Platform.Orm;
using OpenSlx.Lib.Web.Controls;

namespace OpenSlx.Lib.UnitTest
{
    [TestFixture]
    public class TestSimpleLookup
    {
        [Test]
        public void TestDecomposePath()
        {
            
            String entityName = "Sage.SalesLogix.Entities.Contact";
            using (var sess = new SessionScopeWrapper())
            {
                SessionFactoryImpl sf = (SessionFactoryImpl)sess.SessionFactory;
                AbstractEntityPersister persister = (AbstractEntityPersister)(sf.GetEntityPersister(entityName));                
                Assert.AreEqual("LastName", SimpleLookup.DecomposePath(sf, persister, "LASTNAME"));
                Assert.AreEqual("Address.PostalCode", SimpleLookup.DecomposePath(sf, persister, "ADDRESSID=ADDRESSID.ADDRESS!POSTALCODE"));
                Assert.AreEqual("AccountManager.UserInfo.Name", SimpleLookup.DecomposePath(sf, persister, "ACCOUNTMANAGERID>USERID.USERINFO!USERNAME"));
            }
        }
    }
}
