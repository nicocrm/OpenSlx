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
   Copyright 2010 nicocrm (http://github.com/nicocrm/OpenSlx)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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
