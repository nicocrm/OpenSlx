using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.Web.Controls.Lookup;
using System.Web.UI;


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

namespace OpenSlx.Lib.Web.Controls.Workarounds
{
    /// <summary>
    /// Bug fixes for the SLX lookupcontrol.
    /// This defaults the sort to the first column in the lookup when the lookup is set to Initialize.
    /// </summary>
    public class FixSlxLookup : LookupControl
    {
        /// <summary>
        /// Add the sorting hack.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (InitializeLookup && LookupProperties.Count > 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(),
                    @"$(document).ready(function() { setTimeout(function() {
                        " + this.ClientID + @"_luobj.initGrid = function(seedValue, reload) {
                            LookupControl.prototype.initGrid.apply(this, [seedValue, reload]);
                            this.getGrid().getNativeGrid().getStore().setDefaultSort('" + this.LookupProperties[0].PropertyName + @"');
                        };
                    }, 500) });", true);
            }
        }

        /// <summary>
        /// Initialize the lookup image if necessary
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Fix the image - by default the SLX controls tries to load it from the current type's assembly.
            // since this is a subclass of LookupControl it is not in the "correct" assembly anymore.
            // this fix ensures that the image is loaded from the original assembly
            if (this.ViewState["LookupImageURL"] == null)
                LookupImageURL = this.Page.ClientScript.GetWebResourceUrl(typeof(LookupControl), "Sage.SalesLogix.Web.Controls.Resources.Find_16x16.gif");
        }
    }
}
