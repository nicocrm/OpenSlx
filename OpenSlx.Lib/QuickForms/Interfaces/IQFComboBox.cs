using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Controls;


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


namespace OpenSlx.Lib.QuickForms.Interfaces
{
    /// <summary>
    /// Interface used to reference to the combo box from code snippet actions.
    /// This is not working at the moment - the project Sage.Form.Interfaces is not getting a reference
    /// to OpenSlx.Lib.dll so it does not build.
    /// </summary>
    public interface IQFComboBox : IControl
    {
        String Text { get; set; }
    }
}
