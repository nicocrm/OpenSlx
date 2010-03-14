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

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Strategic Sales Systems

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        /// <param name="p"></param>
        /// <param name="adminPassword"></param>
        private String LoadSaleslogixConnectionString(string connectionFile, String username, String adminPassword)
        {
            XDocument doc = XDocument.Load(connectionFile);
            return doc.Element("ConString").Value + ";User ID=" + username + ";Password=" + adminPassword;
        }


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
                _workItem.Services.AddNew(typeof(WebUserOptionsService), typeof(IUserOptionsService));
                _workItem.Services.AddNew(typeof(EntityFactoryContextService), typeof(IEntityContextService));
                _workItem.Services.Add<IFieldLevelSecurityService>(new FieldLevelSecurityService());
            }
            catch (Exception x)
            {
                LOG.Warn("Test setup failed", x);
                throw;
            }
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
        /// <param name="workItem">Parent work item</param>
        /// <param name="appName">Application name (arbitrary name that the app was built with)</param>
        /// <param name="configType">Type of configuration (eg HibernateConfiguration).  May be null if not applicable.</param>
        /// <param name="configFile">Absolute path to configuration file.  It will be copied under the local configuration directory.</param>
        /// <param name="p">Name of the configuration file</param>
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
