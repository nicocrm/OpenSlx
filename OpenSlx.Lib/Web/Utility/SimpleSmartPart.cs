using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.WebPortal.SmartParts;
using System.Web.UI;
using Sage.Platform.Orm.Interfaces;
using Sage.Platform.WebPortal.Binding;
using System.Linq.Expressions;
using Sage.Platform.WebPortal;
using System.Reflection;

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

namespace OpenSlx.Lib.Web.Utility
{
    /// <summary>
    /// Helper class to reduce the amount of boilerplate code needed for custom, bound 
    /// smart parts.
    /// The methods under "Configuration" can be overridden by subclasses.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity - e.g. IAccount</typeparam>
    public class SimpleSmartPart<TEntity> : EntityBoundSmartPartInfoProvider
        where TEntity : class, IPersistentEntity
    {
        #region Helpers

        /// <summary>
        /// Reference to the current entity.
        /// This can be null if the entity has not been loaded yet.
        /// </summary>
        protected TEntity CurrentEntity
        {
            get
            {
                return BindingSource.Current as TEntity;
            }
        }

        private TypedWebEntityBindingGenerator<TEntity> _bf = null;        
        /// <summary>
        /// Create a new binding, add it to the binding source, and return it.
        /// </summary>
        protected WebEntityBinding AddBinding<TComponent, TProperty, TProperty2>(Expression<Func<TEntity, TProperty>> entityProperty,
            TComponent component,
            Expression<Func<TComponent, TProperty2>> componentProperty)
        {
            if (_bf == null)
                _bf = new TypedWebEntityBindingGenerator<TEntity>();
            WebEntityBinding bdg = _bf.CreateBinding(entityProperty, component, componentProperty);
            BindingSource.Bindings.Add(bdg);
            return bdg;
        }

        #endregion


        #region EntityBoundSmartPartInfoProvider Implementation

        public override Type EntityType
        {
            get { return typeof(TEntity); }
        }

        protected override void OnAddEntityBindings()
        {

        }

        public override Sage.Platform.Application.UI.ISmartPartInfo GetSmartPartInfo(Type smartPartInfoType)
        {
            var tinfo = new ToolsSmartPartInfo();
            var container = ToolbarContainer;
            if (container != null)
            {
                List<Control> controlContainer = null;
                switch (container.ToolbarLocation)
                {
                    case SmartPartToolsLocation.Right:
                        controlContainer = tinfo.RightTools;
                        break;
                    case SmartPartToolsLocation.Left:
                        controlContainer = tinfo.LeftTools;
                        break;
                    case SmartPartToolsLocation.Center:
                        controlContainer = tinfo.CenterTools;
                        break;
                }
                if (controlContainer != null)
                {
                    foreach (Control c in container.Controls)
                    {
                        controlContainer.Add(c);
                    }
                }
            }
            if (!String.IsNullOrEmpty(SmartPartTitle))
            {
                tinfo.Title = SmartPartTitle;
                tinfo.Description = SmartPartTitle;
            }
            return tinfo;
        }
        

        #endregion

        #region Configuration

        /// <summary>
        /// Smart part title - the default is to read the "Title" local resource.
        /// </summary>
        protected virtual String SmartPartTitle
        {
            get
            {
                return (String)GetLocalResourceObject("Title") ?? "";
            }
        }


        /// <summary>
        /// Controls from this container will be added to the Smartpart Toolbar.
        /// Return null if toolbar not desired.
        /// </summary>
        protected virtual SmartPartToolsContainer ToolbarContainer
        {
            get
            {
                return null;
            }
        }


        #endregion
    }
}
