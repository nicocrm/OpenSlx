using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sage.Platform.Application;
using Sage.Platform.DynamicMethod;
using Sage.Platform.Configuration;
using Sage.Platform.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using Sage.SalesLogix.Web;
using Sage.Platform.Services;
using Sage.Platform.Security;
using Sage.Platform.Application.Services;
using log4net;
using System.Xml.Linq;
using System.Xml.XPath;
using Sage.Platform.Orm;
using Sage.Platform;
using Sage.Entity.Interfaces;
using Sage.SalesLogix.Security;
using Sage.Platform.Application.UI.Web;
using Sage.Platform.Orm.Services;
using Sage.SalesLogix;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Helper object to set up the Slx Application support object.
    /// Can be used as test harness or to host desktop applications.
    /// The basic principle - you point it to the path of a deployed site (or portal) and it copies the config from there into
    /// the current folder.
    /// </summary>
    public class SlxAppSetup : IDisposable
    {
        private WorkItem _workItem;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SlxAppSetup));

        /// <summary>
        /// Application identifier (for configuration).
        /// Saleslogix will look for configuration file in Configuration\Application\ApplicationName.
        /// </summary>
        public String ApplicationName { get; set; }

        /// <summary>
        /// Root of an existing Saleslogix deployment.
        /// If this is provided, then files will be copied from this location to the 
        /// application configuration folder.
        /// If this is not provided, files will be expected to be present under the 
        /// configuration path already!
        /// </summary>
        public String SourceFolder { get; set; }

        /// <summary>
        /// Saleslogix user name - this should be set before calling Open.
        /// Defaults to Admin.
        /// </summary>
        public String Username { get; set; }

        /// <summary>
        /// Saleslogix password - this should be set before calling Open.
        /// The rest of the connection string will be loaded from connection.config.
        /// If there is a connection string called "Saleslogix" defined in the 
        /// configuration this value will not be used.
        /// </summary>
        public String Password { get; set; }

        /// <summary>
        /// Create an empty setup.
        /// ApplicationName defaults to Test.
        /// </summary>
        public SlxAppSetup()
        {
            this.ApplicationName = "Test";
            this.Username = "ADMIN";
            this.Password = "";
        }

        /// <summary>
        /// Create a setup that will copy all files and assemblies from the 
        /// current web deployment (default to ADMIN user)
        /// </summary>
        public SlxAppSetup(String webDeploymentFolder, String adminPassword)
            : this()
        {
            SourceFolder = webDeploymentFolder;
            Password = adminPassword;
        }

        /// <summary>
        /// Loads the connection string from the connection.config file.
        /// </summary>
        private String LoadSaleslogixConnectionString(string connectionFile, String username, String adminPassword)
        {
            XDocument doc = XDocument.Load(connectionFile);
            return doc.Element("ConString").Value + ";User ID=" + username + ";Password=" + adminPassword;
        }


        /// <summary>
        /// 
        /// </summary>
        ~SlxAppSetup()
        {
            Dispose(false);
        }

        /// <summary>
        /// Start up the Saleslogix application engine.
        /// The parameters set up at construction time will be used.
        /// </summary>        
        public void Open()
        {
            //Globals.Initialize();
            System.Environment.CurrentDirectory = Path.GetDirectoryName(typeof(SlxAppSetup).Assembly.Location);
            try
            {
                _workItem = ApplicationContext.Initialize(ApplicationName);

                PrepareConfigurationFile(null, "log4net.config");
                FileInfo logConfigInfo = new FileInfo(GetConfigurationFile("log4net.config"));
                if (logConfigInfo.Exists)
                {
                    log4net.Config.XmlConfigurator.Configure(logConfigInfo);
                }
                else
                {
                    log4net.Config.XmlConfigurator.Configure();
                }
                PrepareConfigurationFile(typeof(HibernateConfiguration), "hibernate.xml");
                PrepareConfigurationFile(typeof(DynamicMethodConfiguration), "dynamicmethods.xml");
                PrepareConfigurationFile(typeof(DynamicInterceptorConfiguration), "dynamicInterceptors.xml");
                PrepareConfigurationFile(null, "connection.config");

                // allow users to specify connection via the App.config file.
                // if not specified, we'll try and read it from the deployed connection.config.
                var conConfig = System.Configuration.ConfigurationManager.ConnectionStrings["Saleslogix"];
                String connectionString = conConfig == null ? null : conConfig.ConnectionString;
                if (connectionString == null)
                {
                    connectionString = LoadSaleslogixConnectionString(GetConfigurationFile("connection.config"), Username, Password);
                }

                if (SourceFolder != null)
                    CopySaleslogixAssemblies(Path.Combine(SourceFolder, "bin"),
                        Path.GetDirectoryName(
                            GetType().Assembly.GetModules()[0].FullyQualifiedName));
                else
                    LOG.Debug("SourceFolder is null - not copying Saleslogix Assemblies (they must already be present under the execution path!)");


                _workItem.Services.Add<IDataService>(new ConnectionStringDataService(connectionString));
                _workItem.Services.Add<IUserService>(new MockUserService(Username));
                _workItem.Services.Add<IUserOptionsService>(new UserOptionsService(_workItem.Services.Get<IUserService>(), 
                    _workItem.Services.Get<IDataService>()));

                // XXX do we need the "EntityContextService"?
                // it requires a "Parent" work item... so it won't work with the root work item (_workItem)
                WorkItem childWorkItem = new WorkItem();
                childWorkItem.Parent = _workItem;
                _workItem.BuildTransientItem(childWorkItem);
                childWorkItem.Services.AddNew(typeof(EntityFactoryContextService), typeof(IEntityContextService));

                _workItem.Services.AddNew(typeof(SessionFactoryEntityMappingInfoService), typeof(IEntityMappingInfoService));
                _workItem.Services.Add<IFieldLevelSecurityService>(new FieldLevelSecurityService());

            }
            catch (Exception x)
            {
                LOG.Warn("Test setup failed", x);
                throw;
            }
        }

        /// <summary>
        /// Setup a default time zone - this is used in some business rules.
        /// Normally passed in from the client browser.
        /// </summary>
        /// <param name="timezone"></param>
        public void SetTimezone(String timezone)
        {
            var tz = new Sage.Platform.TimeZones().FindTimeZone(timezone);
            if(tz == null)
                throw new Exception("Invalid timezone " + timezone);
            ApplicationContext.Current.Services.Get<Sage.Platform.Application.IContextService>().SetContext("TimeZone", tz);
        }

        /// <summary>
        /// Shut down the application context
        /// </summary>
        public void Close()
        {
            if (_workItem != null)
            {
                try
                {
                    _workItem.Dispose();
                }
                catch (Exception x)
                {
                    LOG.Warn("Error trying to dispose work item", x);
                }
                try
                {
                    ApplicationContext.Shutdown();
                }
                catch (Exception x)
                {
                    LOG.Warn("Error shutting down app context", x);
                }
                _workItem = null;
            }
        }

        /// <summary>
        /// Register configuration file.
        /// This copies the given file under the fixed path for the application so it can be retrieved, and registers it.
        /// If SourceFolder is not specified then the file will be expected to already be there, and the copy step will be skipped.
        /// </summary>
        /// <param name="configType">Type of configuration (eg HibernateConfiguration).  May be null if not applicable.</param>
        /// <param name="configFile">Absolute path to configuration file.  It will be copied under the local configuration directory.</param>        
        private void PrepareConfigurationFile(Type configType, string configFile)
        {
            Directory.CreateDirectory(@"Configuration\Application\" + ApplicationName);
            if (SourceFolder != null && File.Exists(Path.Combine(this.SourceFolder, configFile)))
            {
                File.Copy(Path.Combine(this.SourceFolder, configFile),
                    GetConfigurationFile(configFile), true);
            }
            if (configType != null)
            {
                ConfigurationManager configManager = _workItem.Services.Get<ConfigurationManager>();
                ReflectionConfigurationTypeInfo typeInfo = new ReflectionConfigurationTypeInfo(configType);
                typeInfo.ConfigurationSourceType = typeof(LocalFileConfigurationSource);
                configManager.RegisterConfigurationType(typeInfo);
            }
        }

        /// <summary>
        /// Retrieve path to configuration file of the specified type.
        /// </summary>
        /// <param name="fileName">File name (eg, hibernate.xml)</param>
        /// <returns>Full path</returns>
        private String GetConfigurationFile(String fileName)
        {
            return String.Format(@"Configuration\Application\{0}\{1}", ApplicationName, fileName);
        }


        /// <summary>
        /// Copy all Saleslogix assemblies from the specified folder.
        /// The name of the assemblies to copy are obtained from the dynamicmethods.xml 
        /// and the hibernate.xml files.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void CopySaleslogixAssemblies(string source, string target)
        {
            foreach (String file in GetHibernateAssemblies().Select(x => x + ".dll")
                .Union(GetDynamicMethodsAssemblies().Select(x => x + ".dll")))
            {
                String targetFile = Path.Combine(target, file);
                String sourceFile = Path.Combine(source, file);
                if (!(File.Exists(targetFile) &&
                    File.GetLastWriteTime(sourceFile).CompareTo(File.GetLastWriteTime(targetFile)) <= 0))
                    try
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }
                    catch (IOException x)
                    {
                        LOG.Warn("Unable to copy file " + sourceFile, x);
                    }
            }
            foreach (String file in Directory.GetFiles(source, "Castle.*.dll")
                .Union(Directory.GetFiles(source, "NHibernate.*.dll"))
                .Union(Directory.GetFiles(source, "Iesi.*.dll"))
                .Union(Directory.GetFiles(source, "LinqBridge.dll"))
                .Union(Directory.GetFiles(source, "Interop.*.dll"))
                .Union(Directory.GetFiles(source, "Sage.Entity*.dll"))
                .Union(Directory.GetFiles(source, "Sage.SalesLogix*.dll"))
                .Union(Directory.GetFiles(source, "Sage.Platform*.dll")))
            {
                String targetFile = Path.Combine(target, Path.GetFileName(file));
                String sourceFile = file;
                if (!(File.Exists(targetFile) &&
                    File.GetLastWriteTime(sourceFile).CompareTo(File.GetLastWriteTime(targetFile)) <= 0))
                    try
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }
                    catch (IOException x)
                    {
                        LOG.Warn("Could not copy assembly " + sourceFile + " to " + targetFile, x);
                    }
            }
        }

        private IEnumerable<String> GetHibernateAssemblies()
        {
            XDocument doc = XDocument.Load(GetConfigurationFile("hibernate.xml"));
            return from element in doc.Element("NHibernate")
                .Element("MappingAssemblies")
                .Elements("AssemblyName")
                   select element.Value;
        }


        private IEnumerable<String> GetDynamicMethodsAssemblies()
        {
            Regex removeTypeName = new Regex(".*, *(.*)");
            List<String> assemblies = new List<string>();
            foreach (var typeName in GetDynamicMethodsTypes())
            {
                String assemblyName = removeTypeName.Replace(typeName, "$1");
                if (!assemblies.Contains(assemblyName))
                    assemblies.Add(assemblyName);
            }
            return assemblies;
        }

        private IEnumerable<String> GetDynamicMethodsTypes()
        {
            XPathDocument doc = new XPathDocument(GetConfigurationFile("dynamicmethods.xml"));
            var navigator = doc.CreateNavigator();
            var iterator = navigator.Select("//dynamicMethod//target/@targetType");
            while (iterator.MoveNext())
                yield return iterator.Current.Value;
            iterator = navigator.Select("//dynamicMethod//primaryTarget/@targetType");
            while (iterator.MoveNext())
                yield return iterator.Current.Value;
        }

        /// <summary>
        /// Helper class to get the configuration to load from the current directory
        /// </summary>
        private class LocalFileConfigurationSource : FileConfigurationSource
        {
            protected override string TransformConfigurationPath(IConfigurationTypeInfo typeContext, IConfigurationProvider provider, string configPath)
            {
                return Path.Combine(System.Environment.CurrentDirectory, configPath.Replace("/", @"\"));
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Shut down the application context (if it is still open).
        /// Exceptions will be silently ignored.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            try
            {
                Close();
            }
            catch (Exception) { }

            if (disposing)
                GC.SuppressFinalize(this);
        }

        #endregion
    }
}
