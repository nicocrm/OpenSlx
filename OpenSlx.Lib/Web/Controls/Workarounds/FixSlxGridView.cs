using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.Web.Controls;
using System.Collections.Specialized;
using System.Collections;
using System.Web.UI.WebControls;

namespace OpenSlx.Lib.Web.Controls.Workarounds
{
    /// <summary>
    /// This fix corrects a NullReferenceException that occurs when the ExpandableRows property
    /// is enabled, and the grid uses standard ASP.NET command buttons.
    /// </summary>
    public class FixSlxGridView : SlxGridView
    {
        protected override void ExtractRowValues(IOrderedDictionary fieldValues, GridViewRow row, bool includeReadOnlyFields, bool includePrimaryKey)
        {
            if (fieldValues != null)
            {
                ICollection is2 = this.CreateColumns(null, false);
                int count = is2.Count;
                object[] array = new object[count];
                string[] dataKeyNamesInternal = this.DataKeyNames;
                is2.CopyTo(array, 0);
                for (int i = 0; (i < count) && (i < row.Cells.Count); i++)
                {
                    if (((DataControlField)array[i]).Visible)
                    {
                        OrderedDictionary dictionary = new OrderedDictionary();
                        try
                        {
                            ((DataControlField)array[i]).ExtractValuesFromCell(dictionary, row.Cells[i] as DataControlFieldCell, row.RowState, includeReadOnlyFields);
                            foreach (DictionaryEntry entry in dictionary)
                            {
                                if (includePrimaryKey || (Array.IndexOf(dataKeyNamesInternal, entry.Key) == -1))
                                {
                                    fieldValues[entry.Key] = entry.Value;
                                }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            if (!ExpandableRows)
                                throw;
                        }
                    }
                }
            }
        }
    }
}
