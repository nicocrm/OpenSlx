# OpenSlx

## Overview

A collection of utilities and controls for the InforCRM platform.

## Installation

The easiest way to install OpenSlx in your project is from nuget using the OpenSlx.Lib package:

    Install-Package OpenSlx.Lib
	
Version 1.0 is compatible with Saleslogix 8.0 and InforCRM 8.1 and 8.2. 

## Compilation

To compile the project, you will need to add references to the SalesLogix assemblies
(as they are not redistributable they are not included with the solution files).

You can pick them up from a local web project.

## Contributing

Pull requests are welcome!

## OpenSlx Library

### Db

 * SlxCustomDialect: left as an example of a custom SQL dialect, this is no longer needed with recent versions of InforCRM.
 * DbHelper: utility methods for direct SQL access.

### QuickForms

Custom controls to be used in your quickforms in Application Architect.
At this time the only control is a combo box which has been superseded by the stock combobox control.

### Security

 * SecUtils: utility methods to determine whether a user is in a team, and shortcuts to get the current user.
 
### SlxDataHelpers

Helper methods designed to make the Slx data easier to access and/or format.

 * TimeZoneConvert: convert datetime to and from the web user's timezone.
 
### Utility

Miscellaneous utilities - mostly helpers for unit testing (TODO: move to another namespace?)

### Web.Controls

Custom ASP.NET controls.

### Web.Extensions

Useful extension methods for existing web controls.

### Utility

Miscellaneous utilities, web related.  

 * TypedWebEntityBindingGenerator: helper class for creating strongly typed bindings
 * SimpleSmartPart: boiler plate code for creating custom smart parts
 
## Unit testing with OpenSlx

OpenSlx includes several utilities meant to facilitate unit testing in your custom library.
These include:

 * SlxAppSetup: a test harness meant to be extended and used as your TestSuiteSetup
 * TestObjectFactory: an extensible factory class for generating objects 
 
Example test suite setup class: 
 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using OpenSlx.Lib.Utility;
    using NUnit.Framework;
    
    namespace SSSWorld.ISCO.BusinessRules.UnitTest
    {
        /// <summary>
        /// Setup fixture for the assembly
        /// </summary>
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
                _setup = new SlxAppSetup(@"\inetpub\WWWRoot\ISCO\SlxClient", "");
    
                try
                {
                    _setup.Open();
                    _setup.SetTimezone(TIMEZONE);
                }
                catch (Exception)
                {
                    // if you get an error in SetUpFixture put a breakpoint here to get the details
                    throw;
                }
    
            }
    
    
            [TearDown]
            public void Teardown()
            {
                _setup.Close();
            }
        }
    }
    
Be sure to build your test assembly in 32-bit mode, and you may also have to set up your test runner to 
avoid copying the test assemblies to an alternate location (e.g. in Resharper, disable shadow copying)
