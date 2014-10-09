using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sage.Platform.WebPortal;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal.SmartParts;
using System.Linq.Expressions;
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
            String script;
            if (callback != null)
            {
                script = "Sage.UI.Dialogs.raiseQueryDialogExt({ callbackFn: " + callback + ", title: 'Sage SalesLogix', query: '" + PortalUtil.JavaScriptEncode(msg) + "' });";
            }
            else
            {
                script = "Sage.UI.Dialogs.alert('" + PortalUtil.JavaScriptEncode(msg) + "');";
            }
            ctl.JavaScript(script);
        }

        /// <summary>
        /// Enable / disable all controls on the form.
        /// Controls with ViewState disabled are ignored.
        /// </summary>
        /// <param name="parent">The control to be disabled (usually called from a form as this.LockForm(true))</param>
        /// <param name="islocked"></param>
        /// <returns>A list of ids that were touched (enabled or disabled).  This can be stored in the viewstate to automatically undo the LockForm method by calling UnlockForm.</returns>
        public static List<String> LockForm(this Control parent, bool islocked)
        {
            List<String> result = new List<string>();
            foreach (Control c in parent.Controls)
            {
                if (c is IButtonControl || c is SmartPartToolsContainer)
                    continue;
                if (!c.EnableViewState)
                    // I am going to assume that if ViewState is disabled on the control, it is a static readonly control and
                    // there is no point in making it go through this logic.
                    // SLX databinding won't work with them anyway since they won't trigger events correctly, so its a pretty safe bet.
                    continue;
                if (c.Controls.Count > 0 && !(c is CompositeControl))
                    result.AddRange(LockForm(c, islocked));

                bool touched = false;
                PropertyInfo prRO = c.GetType().GetProperty("ReadOnly");
                PropertyInfo prEnabled = c.GetType().GetProperty("Enabled");
                bool isRo = false;
                if (prRO != null && (bool)prRO.GetValue(c, null))
                    isRo = true;
                if (prEnabled != null && !(bool)prEnabled.GetValue(c, null))
                    isRo = true;
                if (isRo == islocked)
                {
                    // already set to right state
                    continue;
                }
                if (prRO != null)
                {
                    touched = true;
                    prRO.SetValue(c, islocked, null);
                }
                else if (prEnabled != null)
                {
                    touched = true;
                    prEnabled.SetValue(c, !islocked, null);
                    if (c is WebControl && !((WebControl)c).SupportsDisabledAttribute)
                    // extra for dropdowns because ASP.NET marks them as SupportsDisabledAttribute = false, 
                    // which means the "disabled" attribute does not get output by default
                    {
                        ((WebControl)c).Attributes["disabled"] = "disabled";
                    }
                }
                if (touched)
                    result.Add(c.UniqueID);
            }
            return result;
        }

        /// <summary>
        /// Undo the "LockForm" using the list of ids returned by LockForm
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="controlIds"></param>
        public static void UnlockForm(this Control parent, IEnumerable<String> controlIds)
        {
            foreach (String id in controlIds)
            {
                Control c = FindControlRecursive(parent, id);
                if (c != null)
                {
                    PropertyInfo pr = c.GetType().GetProperty("ReadOnly");
                    if (pr != null)
                    {
                        pr.SetValue(c, false, null);
                    }
                    pr = c.GetType().GetProperty("Enabled");
                    if (pr != null)
                    {
                        pr.SetValue(c, true, null);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to find a control nested in a hierarchy of naming containers
        /// </summary>
        /// <param name="rootControl"></param>
        /// <param name="controlID">UniqueID of the control to locate</param>
        /// <returns></returns>
        private static Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.UniqueID == controlID) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn =
                    FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }

        /// Convenience method to show an "Add" dialog. (TODO)
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="control"></param>
        /// <param name="childsParentRelationship">How to get the parent from the child</param>
        /// <param name="parentsChildCollection">How to get the child collection from the parent</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="smartPartId"></param>
        // not working 
        //public static void ShowAddDialog<TParent, TChild>(this SmartPart control,
        //    Expression<Func<TChild, TParent>> childsParentRelationship,
        //    Expression<Func<TParent, ICollection<TChild>>> parentsChildCollection,
        //    int height, int width, String smartPartId)
        //{
        //    PropertyInfo childsParentRelationshipProp = (PropertyInfo)((MemberExpression)childsParentRelationship.Body).Member; ;
        //    PropertyInfo parentsChildCollectionProp = (PropertyInfo)((MemberExpression)parentsChildCollection.Body).Member;
        //    control.DialogService.SetChildIsertInfo(typeof(TChild), typeof(TParent),
        //        childsParentRelationshipProp, parentsChildCollectionProp);
        //    control.DialogService.SetSpecs(height, width, smartPartId);
        //    control.DialogService.ShowDialog();
        //}
    }
}
