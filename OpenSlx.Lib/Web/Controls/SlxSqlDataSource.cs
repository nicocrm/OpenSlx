using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sage.SalesLogix.API;

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
