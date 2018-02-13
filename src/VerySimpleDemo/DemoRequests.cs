using System;
using System.Windows;
using System.Windows.Controls;
using WpfAsyncRequest;

namespace VerySimpleDemo
{
    public static class DemoRequests
    {
        #region [AR] AskUserRequest

        // Register an attached request for Control
        public static readonly DependencyProperty AskUserRequestProperty
            = AsyncRequest.RegisterAttached((Func<Control, string, bool?>)AskUser, typeof(DemoRequests));

        // Standard Get and Set methods for an AttachedProperty
        public static void SetAskUserRequest(Control element, IResponsive<string, bool> value)
        {
            element.SetValue(AskUserRequestProperty, value);
        }

        public static IResponsive<string, bool> GetAskUserRequest(Control element)
        {
            return (IResponsive<string, bool>)element.GetValue(AskUserRequestProperty);
        }

        // Action we gonna perform on request received.
        private static bool? AskUser(Control control, string question)
        {
            switch (MessageBox.Show(question, "Hmm?", MessageBoxButton.YesNoCancel))
            {
                case MessageBoxResult.Cancel:
                    return null;

                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
