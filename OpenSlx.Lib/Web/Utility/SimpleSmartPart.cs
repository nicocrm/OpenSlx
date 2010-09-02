using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.WebPortal.SmartParts;

namespace OpenSlx.Lib.Web.Utility
{
    /// <summary>
    /// Helper class to reduce the amount of boilerplate code needed for custom, bound 
    /// smart parts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleSmartPart<T> : EntityBoundSmartPartInfoProvider
        where T: class
    {
        #region Helpers

        /// <summary>
        /// Reference to the current entity.
        /// This can be null if the entity has not been loaded yet.
        /// </summary>
        protected T CurrentEntity
        {
            get
            {
                return BindingSource.Current as T;
            }
        }

        #endregion


        #region EntityBoundSmartPartInfoProvider Implementation

        public override Type EntityType
        {
            get { return typeof(T); }
        }

        protected override void OnAddEntityBindings()
        {

        }

        #endregion
    }
}
