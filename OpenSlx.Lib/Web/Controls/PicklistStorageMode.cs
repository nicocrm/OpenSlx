#if SIMPLE_PICKLIST 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

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
        /// <summary>
        /// Store as item id
        /// </summary>
        Id = 1,
        /// <summary>
        /// Store as code (shorttext)
        /// </summary>
        Code = 2,
        /// <summary>
        /// Store as actual text
        /// </summary>
        Text = 4
    }
}

#endif