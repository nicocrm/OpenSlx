using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.WebPortal.Workspaces.Tab;
using OpenSlx.Lib.Services;
using OpenSlx.Lib.Web.Utility;
using OpenSlx.Lib.Utility;
using System.Web.UI;

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

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// A tweak for the TabWorkspace that renders the middle pane on the bottom, 
    /// instead of the middle.
    /// This is handy for forms like Complete Activity.
    /// 
    /// To use this instead of the regular workspace the master page must be edited and the workspace control replaced
    /// with this version.
    /// 
    /// Uses reflection so may not be upgrade safe.  If the fields cannot be found on the parent
    /// then it will fall back to the stock behavior.
    /// </summary>
    public class TabWorkspaceWithMiddleOnBottom : TabWorkspace
    {
        protected override void RenderChildren(HtmlTextWriter writer)
        {
            ICacheService cache = new WebCacheService();

            try
            {
                Control scriptManager = ((Control)ReflectionHelper.GetFieldValue(this, "m_scriptManagerProxy", cache));
                Control elementDrag = ((Control)ReflectionHelper.GetFieldValue(this, "m_elementDragHelper", cache));
                Control mainSection = ((Control)ReflectionHelper.GetFieldValue(this, "m_mainSection", cache));
                Control middleSection = ((Control)ReflectionHelper.GetFieldValue(this, "m_middleSection", cache));

                scriptManager.RenderControl(writer);
                elementDrag.RenderControl(writer);
                this.StateProxy.RenderControl(writer);
                mainSection.RenderControl(writer);
                middleSection.RenderControl(writer);
            }
            catch (KeyNotFoundException)
            {
                base.RenderChildren(writer);
            }
        }
    }
}
