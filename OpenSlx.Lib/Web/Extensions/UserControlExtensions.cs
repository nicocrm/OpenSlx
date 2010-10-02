using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sage.Platform.WebPortal;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal.SmartParts;
using System.Reflection;

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
        /// <param name="ctl"></param>
        /// <param name="msg"></param>
        /// <param name="callback">Can specify javascript code to be run when user closes the message box.  
        /// This needs to be specified as a Javascript function, for example "function() { alert('boo') }"</param>
        public static void MessageBox(this UserControl ctl, String msg, String callback = null)
        {
            String script = "Ext.MessageBox.alert('', '" + PortalUtil.JavaScriptEncode(msg) + "'";
            if (!String.IsNullOrEmpty(callback))
                script += ", " + callback;
            script += ");";
            ctl.JavaScript(script);
        }

        /// <summary>
        /// Enable / disable all controls on the form.
        /// Controls with ViewState disabled are ignored.
        /// </summary>
        /// <param name="parent">The control to be disabled (usually called from a form as this.LockForm(true))</param>
        /// <param name="islocked"></param>
        public static void LockForm(this Control parent, bool islocked)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is IButtonControl || c is SmartPartToolsContainer)
                    continue;
                if (!c.EnableViewState)
                    // I am going to assume that if ViewState is disabled on the control, it is a static readonly control and
                    // there is no point in making it go through this logic.
                    // SLX databinding won't work with them anyway since they won't trigger events correctly, so its a pretty safe bet.
                    continue;
                if (c.Controls.Count > 0)
                    LockForm(c, islocked);

                PropertyInfo pr = c.GetType().GetProperty("ReadOnly");
                if (pr != null)
                {
                    pr.SetValue(c, islocked, null);
                }
                else
                {
                    pr = c.GetType().GetProperty("Enabled");
                    if (pr != null)
                    {
                        pr.SetValue(c, !islocked, null);
                    }
                }
            }
        }
    }
}
