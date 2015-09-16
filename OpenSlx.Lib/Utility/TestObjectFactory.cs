using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSlx.Lib.Security;
using Sage.Entity.Interfaces;
using Sage.Platform;

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Singleton class used for generating test objects.
    /// Generally one would inherit this class, extend it with methods specific to their own entities, 
    /// override the existing methods as needed, and replace the Instance static method with one returning 
    /// an instance of their own class.
    /// </summary>
    public class TestObjectFactory
    {
        public static TestObjectFactory Instance
        {
            get
            {
                return new TestObjectFactory();
            }
        }

        public virtual IAccount CreateAccount(bool save = false)
        {
            IAccount a = EntityFactory.Create<IAccount>();
            a.AccountName = "Test Account";
            a.Owner = SecUtils.CurrentUser.DefaultOwner;
            a.AccountManager = SecUtils.CurrentUser;
            a.Address.Address1 = "Test Address";
            a.Address.City = "Test City";
            a.Address.PostalCode = "34444";
            a.Address.State = "TN";
            if (save)
                a.Save();
            return a;
        }

        public virtual IContact CreateContact(bool save = false, IAccount account = null)
        {
            IContact c = EntityFactory.Create<IContact>();
            if (account == null)
                account = CreateAccount(save);
            c.Owner = account.Owner;
            c.Account = account;
            c.LastName = "Test";
            c.FirstName = "Joe";
            c.AccountManager = SecUtils.CurrentUser;
            if (save)
                c.Save();
            return c;
        }

        public virtual ITicket CreateTicket(bool save = false, IAccount account = null, IContact contact = null)
        {
            if (account == null)
                account = CreateAccount();
            ITicket ticket = EntityFactory.Create<ITicket>();
            ticket.Account = account;
            ticket.Contact = contact ?? CreateContact(save, account);
            ticket.Owner = SecUtils.CurrentUser.DefaultOwner;
            if (save)
                ticket.Save();
            return ticket;
        }

        public virtual IReturn CreateReturn(bool save = false, IAccount account = null)
        {
            if (account == null)
                account = CreateAccount(save);
            IReturn rma = EntityFactory.Create<IReturn>();
            rma.Account = account;
            rma.Ticket = CreateTicket(save, account);
            rma.Owner = SecUtils.CurrentUser.DefaultOwner;
            if (save)
                rma.Save();
            return rma;
        }

        public virtual IProduct CreateProduct(bool save = false)
        {
            IProduct prod = EntityFactory.Create<IProduct>();
            prod.Name = "Test Product";
            prod.Status = "Available";
            if (save)
                prod.Save();
            return prod;
        }

        public virtual IOpportunity CreateOpportunity(bool save = false, IAccount account = null)
        {
            if (account == null)
                account = CreateAccount(save);
            var opp = EntityFactory.Create<IOpportunity>();
            opp.Account = account;
            account.Opportunities.Add(opp);
            opp.Description = "Test Opportunity";
            opp.Owner = account.Owner;
            opp.AccountManager = account.AccountManager;
            opp.Status = "Open";
            if (save)
                opp.Save();
            return opp;
        }

        public virtual IOpportunityProduct CreateOpportunityProduct(bool save = false, IOpportunity opportunity = null, IProduct product = null)
        {
            if (opportunity == null)
                opportunity = CreateOpportunity(save);
            var oppProd = EntityFactory.Create<IOpportunityProduct>();
            oppProd.Opportunity = opportunity;
            oppProd.Product = CreateProduct(save);
            opportunity.Products.Add(oppProd);
            if (save)
                oppProd.Save();
            return oppProd;
        }
    }
}
