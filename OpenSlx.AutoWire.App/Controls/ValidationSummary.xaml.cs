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
using System.Collections.ObjectModel;

namespace OpenSlx.AutoWire.App.Controls
{

    public interface IErrorViewer
    {
        void SetElement(DependencyObject element);
    }


    /// <summary>
    /// Courtesy of Ed Foh - http://codeblitz.wordpress.com/2009/05/12/wpf-validation-summary-control/
    /// </summary>
    public partial class ValidationSummary : UserControl, IErrorViewer
    {
        private List<DependencyObject> _elements = new List<DependencyObject>();
        private ObservableCollection<string> _errorMessages = new ObservableCollection<string>();


        public ValidationSummary()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ErrorViewer_Loaded);
        }

        void ErrorViewer_Loaded(object sender, RoutedEventArgs e)
        {
            itemsControl.ItemsSource = _errorMessages;
        }

        private void Element_ValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added && !_errorMessages.Contains(e.Error.ErrorContent.ToString()))
            {
                _errorMessages.Add(e.Error.ErrorContent.ToString());
            }
            else if (e.Action == ValidationErrorEventAction.Removed && _errorMessages.Contains(e.Error.ErrorContent.ToString()))
            {
                _errorMessages.Remove(e.Error.ErrorContent.ToString());
            }
        }

        #region IErrorViewer Members

        public void SetElement(DependencyObject element)
        {
            if (!_elements.Contains(element))
            {
                _elements.Add(element);
                Validation.AddErrorHandler(element, Element_ValidationError);
            }
        }

        #endregion
    }
}
