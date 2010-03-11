using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sage.SalesLogix.API;

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
