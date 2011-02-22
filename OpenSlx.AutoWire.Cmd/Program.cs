using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Application;
using Sage.Platform.Data;
using Sage.Platform.Projects;
using Sage.Platform.Projects.Interfaces;

namespace OpenSlx.AutoWire.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationContext.Initialize(Guid.NewGuid().ToString());
            ApplicationContext.Current.Services.Add<IDataService>(new ConnectionStringDataService(SelectedConnection.BuildConnectionString(Password)));
            IProject project = ProjectUtility.InitProject(ModelPath);
            ApplicationContext.Current.Services.Add<IProjectContextService>(new SimpleProjectContextService(project));         
        }
    }
}
