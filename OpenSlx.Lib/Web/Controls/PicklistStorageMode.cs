using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Indicates how a picklist is stored or displayed
    /// (note - this is used, rather than the default Sage StoreModeEnum, to allow for more than 1 items to be selected,
    /// if the user wants to see both code and text)
    /// </summary>
    [Flags]
    public enum PicklistStorageMode
    {
        Id = 1,
        Code = 2,
        Text = 4
    }
}
