using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal;
using Sage.Platform.Application;
using System.Reflection;
using System.Collections;
using System.Linq;
using Sage.Platform.ComponentModel;
using Sage.Platform.WebPortal.SmartParts;
using System.Diagnostics;
using Sage.Platform.Orm.Interfaces;
using Sage.Platform.Application.UI.Web;
using Sage.Platform.WebPortal.Services;
using OpenSlx.Lib.Properties;
using OpenSlx.Lib.Utility;
using OpenSlx.Lib.Web.Utility;

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
    /// Adaptation of the EntityDataSource for an entity bound to a Saleslogix property.
    /// Suitable for editable datagrids on Saleslogix tabs.
    /// </summary>
    [ToolboxData("<{0}:SlxEntityDataSource runat=server></{0}:SlxEntityDataSource>")]
    public class SlxEntityDataSource : EntityDataSource
    {
        private String _entityDataSourceProperty;
        [Description("Name of the property to access in the form's entity to retrieve the collection to bind to.  To bind to the entity itself, use a period."),
        DefaultValue(".")]
        public String EntityDataSourceProperty
        {
            get { return _entityDataSourceProperty; }
            set { _entityDataSourceProperty = value; }
        }

        private IPersistentEntity _sourceEntity = null;
        /// <summary>
        /// Specify the entity that the data will be pulled from.
        /// By default, SlxEntityDataSource will attempt to retrieve this from the smart part's data binding.
        /// </summary>
        public IPersistentEntity SourceEntity
        {
            get
            {
                if (_sourceEntity != null)
                    return _sourceEntity;
                return (IPersistentEntity)FindParentSmartPart().BindingSource.Current;
            }
            set
            {
                _sourceEntity = value;
            }
        }

        /// <summary>
        /// List of fields to be included in the retrieved data, separated by commas.
        /// This can be used to include sub-properties, e.g. Product.Price.
        /// If non-null a dictionary object containing all the specified properties will be returned,
        /// otherwise the original object will be returned.
        /// </summary>
        //public String SelectFields { get; set; }

        /// <summary>
        /// Attempt to find the smart part containing this control.
        /// </summary>
        /// <returns></returns>
        private EntityBoundSmartPart FindParentSmartPart()
        {
            Control iteration = this.Parent;
            while (iteration != null)
            {
                if (iteration is EntityBoundSmartPart)
                    return (EntityBoundSmartPart)iteration;
                iteration = iteration.Parent;
            }
            throw new InvalidOperationException(Resources.SlxEntityDataSourceCanOnlyBeUsedOnAnEntity);
        }

        /// <summary>
        /// Populate the parent's data source collection based on the provided data source property name.
        /// </summary>
        private void PopulateDataSource()
        {
            object entity = SourceEntity;
            if (entity == null)
                return;
            if (_entityDataSourceProperty == ".")
                DataSource = new object[] { entity };
            else
            {
                try
                {
                    DataSource = ((IEnumerable)ReflectionHelper.GetPropertyValue(entity, _entityDataSourceProperty, new WebCacheService()))
                        .Cast<object>();
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException(String.Format(Resources.PropertyX0MustBeCastableToICollection, _entityDataSourceProperty));
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }


        /// <summary>
        /// Reload the entity collection and signal connected objects that the data may have changed.
        /// </summary>
        public override void Refresh()
        {
            PopulateDataSource();
            base.Refresh();
        }

        protected override DataSourceView GetView(string viewName)
        {
            if (!String.IsNullOrEmpty(EntityDataSourceProperty))
                PopulateDataSource();
            EntityDataSourceView view = (EntityDataSourceView)base.GetView(viewName);
            // we use that for SalesLogix in order to be able to match by InstanceId or Id
            view.MatchByAnyKey = true;
            return view;
        }

        /// <summary>
        /// Remove entities.
        /// If the DeleteMethodName is specified then this will be called, otherwise we'll try and remove them 
        /// from the parent collection.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public override int DeleteEntities(IEnumerable<object> entities)
        {
            // either way we delete them, most likely we'll need a refresh
            var panelRefresh = ((ApplicationPage)Page).PageWorkItem.Services.Get<IPanelRefreshService>();
            if (panelRefresh != null)
                panelRefresh.RefreshAll();

            if (!String.IsNullOrEmpty(this.DeleteMethodName))
                return base.DeleteEntities(entities);
            // fallback - delete entities by removing them from the collection

            // remove reference from the parent's collection (if there is one)
            object parentEntity = SourceEntity;
            Debug.Assert(parentEntity != null, "parentEntity != null");
            object parentCollection = ReflectionHelper.GetPropertyValue(parentEntity,
                this.EntityDataSourceProperty, new WebCacheService());
            Debug.Assert(parentCollection != null, "parentCollection != null");
            MethodInfo funRemove = parentCollection.GetType().GetMethod("Remove");
            Debug.Assert(funRemove != null, "funRemove != null");
            int count = 0;
            foreach (IPersistentEntity child in entities)
            {
                funRemove.Invoke(parentCollection, new object[] { child });
                // NOW we can remove it
                if (child.PersistentState == PersistentState.Modified || child.PersistentState == PersistentState.Unmodified)
                    child.Delete();
                count++;
            }
            return count;
        }

        /*
         * an experiment to try and pull "linked" entities  (e.g. Contact.Account.AccountName)
         * Not working yet
        public override IEnumerable DecorateList(IEnumerable pagedList)
        {
            if (SelectFields == null)
                return base.DecorateList(pagedList);
            var cache = IoC.Resolve<ICacheService>();
            String[] fields = SelectFields.Split(new char[] { ',' });
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim();                
            }
            var results = new List<object>();
            foreach (object o in pagedList)
            {
                ComponentView componentView = new ComponentView(fields, o);
                results.Add(componentView);
            }
            return results;
        }
        */
        /*
         * Following 2 are work arounds that no longer appear to be required
        /// <summary>
        /// Work around for tabs in Saleslogix.
        /// We have to force a reload of the data after the postback data has been processed, otherwise the 
        /// values from one tab will persist on the next one!
        /// </summary>
        /// <param name="e"></param>
        void parent_FormBound(object sender, EventArgs e)
        {
            var entity = (IComponentReference)EntityContext.GetEntity();

            if (!object.Equals(ViewState["PrevEntityId"], entity.Id))
            {
                if (ViewState["PrevEntityId"] != null)
                {
                    // force a reload, if they navigate to a different entity
                    Refresh();
                }

                ViewState["PrevEntityId"] = entity.Id;
            }
        }


        /// <summary>
        /// Work around for tabs in Saleslogix.
        /// We have to cancel the update if they have navigated to a different entity.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="values"></param>
        /// <param name="oldValues"></param>
        /// <returns></returns>
        public override int UpdateEntities(IEnumerable<object> entities, IDictionary values, IDictionary oldValues)
        {
            var entity = (IComponentReference)EntityContext.GetEntity();

            if (object.Equals(ViewState["PrevEntityId"], entity.Id))
                return base.UpdateEntities(entities, values, oldValues);
            else
                return 0;
        }
         */
    }

}
