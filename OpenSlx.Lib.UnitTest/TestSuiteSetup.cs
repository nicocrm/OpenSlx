using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sage.Platform.Application;
using OpenSlx.Lib.Utility;

namespace OpenSlx.Lib.UnitTest
{    
    
    [SetUpFixture]
    public class TestSuiteSetup
    {
        SlxAppSetup _setup;

        /// <summary>
        /// Time zone used for testing (make sure this is different from the local time zone)
        /// </summary>
        public const String TIMEZONE = "Pacific Standard Time (Mexico)";


        [SetUp]
        public void Setup()
        {
            // app root - this needs to be updated if the project is deployed somewhere else
            _setup = new SlxAppSetup(@"\inetpub\WWWRoot\SlxEval\SlxClient", "");

            try
            {
                _setup.Open();
            }
            catch (Exception)
            {
                throw;
            }

            // setup a default time zone - this is used in some business rules
            // normally passed in from the client browser.
            ApplicationContext.Current.Services.Get<Sage.Platform.Application.IContextService>().SetContext("TimeZone",
                new Sage.Platform.TimeZones().FindTimeZone(TIMEZONE));
        }


        [TearDown]
        public void Teardown()
        {
            _setup.Close();
        }
    }

}
