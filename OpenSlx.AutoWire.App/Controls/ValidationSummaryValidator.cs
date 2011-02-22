using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenSlx.AutoWire.App.Controls
{
    /// <summary>
    /// Helper class used to relate a validated element to the validation summary
    /// Courtesy of Ed Foh - http://codeblitz.wordpress.com/2009/05/12/wpf-validation-summary-control/
    /// </summary>
    public partial class ValidationSummaryValidator
    {

        public static DependencyProperty AdornerSiteProperty =
    DependencyProperty.RegisterAttached("AdornerSite", typeof(DependencyObject), typeof(ValidationSummaryValidator),
    new PropertyMetadata(null, OnAdornerSiteChanged));

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static DependencyObject GetAdornerSite(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (element.GetValue(AdornerSiteProperty) as DependencyObject);
        }

        public static void SetAdornerSite(DependencyObject element, DependencyObject value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(AdornerSiteProperty, value);
        }

        private static void OnAdornerSiteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IErrorViewer errorViewer = e.NewValue as IErrorViewer;
            if (errorViewer != null)
            {
                errorViewer.SetElement(d);
            }
        }

    }
}
