using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.Web.Controls;
using System.Collections.Specialized;
using System.Collections;
using System.Web.UI.WebControls;

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
    /// This fix corrects a NullReferenceException that occurs when the ExpandableRows property
    /// is enabled, and the grid uses standard ASP.NET command buttons.
    /// </summary>
    public class FixSlxGridView : SlxGridView
    {
        /// <summary>
        /// Used in databinding.
        /// There is a bug in the original SlxGridView which causes this method to fail.
        /// </summary>
        /// <param name="fieldValues"></param>
        /// <param name="row"></param>
        /// <param name="includeReadOnlyFields"></param>
        /// <param name="includePrimaryKey"></param>
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
