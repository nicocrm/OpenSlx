using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sage.Platform.WebPortal;

namespace OpenSlx.Lib.Web.Extensions
{
    /// <summary>
    /// Useful extension methods for web user controls that are used as smart parts.
    /// </summary>
    public static class UserControlExtensions
    {
        /// <summary>
        /// Send a script to the page.
        /// A shortcut for ScriptManager.RegisterStartupScript.
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="script"></param>
        public static void JavaScript(this UserControl ctl, String script)
        {
            ScriptManager.RegisterStartupScript(ctl, ctl.GetType(), Guid.NewGuid().ToString(),
                script, true);
        }

        /// <summary>
        /// Convenience function to show an alert message to the user, without using
        /// the javascript (modal) function.
        /// Note that you can generally only show 1 per page.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="callback">Can specify javascript code to be run when user closes the message box.  
        /// This needs to be specified as a Javascript function, for example "function() { alert('boo') }"</param>
        public static void MessageBox(this UserControl ctl, String msg, String callback=null)
        {
            String script = "Ext.MessageBox.alert('', '" + PortalUtil.JavaScriptEncode(msg) + "'";
            if (!String.IsNullOrEmpty(callback))
                script += ", " + callback;
            script += ");";
            ctl.JavaScript(script);            
        }
    }
}
