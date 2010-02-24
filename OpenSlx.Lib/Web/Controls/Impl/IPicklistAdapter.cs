using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Utility interfaces used to access the picklist's value.
    /// </summary>
    public interface IPicklistAdapter
    {
        /// <summary>
        /// Returns the underlying value (or values, for a multi-select list)
        /// </summary>
        /// <returns></returns>
        String GetValue();

        /// <summary>
        /// Sets the underlying value on the control (or values, for a multi-select list)
        /// </summary>
        /// <param name="value"></param>
        void SetValue(String value);

        /// <summary>
        /// Creates controls and attaches them to the designated parent.
        /// </summary>
        /// <param name="parentControl"></param>
        void CreateChildControls(SimplePicklist parentControl);

        /// <summary>
        /// Events should get triggered when the value may have changed.
        /// </summary>
        event EventHandler TextChanged;
    }
}
