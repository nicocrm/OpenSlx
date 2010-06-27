using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sage.SalesLogix.API;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/nicocrm/OpenSlx)

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

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Extension for the standard SqlDataSource - this will set the connection string based on the ApplicationContext value.
    /// </summary>
    /// <see cref="BindingSourceParameter"/>
    public class SlxSqlDataSource : SqlDataSource
    {
        /// <summary>
        /// Add Connection string from SalesLogix.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            ProviderName = "System.Data.OleDb";
            ConnectionString = MySlx.Data.CurrentConnection.GetConnectionString();
            base.OnInit(e);
        }
    }
}
