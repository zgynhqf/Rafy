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

namespace VSTemplates.Wizards.Items.DomainEntity
{
    public partial class DomainEntityWizardWindow : Window
    {
        public DomainEntityWizardWindow()
        {
            InitializeComponent();

            this.Loaded += DomainEntityWizardWindow_Loaded;

            this.AddHandler(UIElement.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(UIElement_GotKeyboardFocus));
        }

        void DomainEntityWizardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEntityKeyRowHeight();
            txtClassName.Focus();
        }

        void UIElement_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = e.Source as TextBox;
            if (tb != null)
            {
                tb.SelectAll();
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }

            base.OnKeyDown(e);
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void txtParentEntityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            InitEntityKeyRowHeight();
        }

        private void InitEntityKeyRowHeight()
        {
            entityKeyRow.Height = txtParentEntityName.Text.Length > 0 ?
                GridLength.Auto : new GridLength(0);
        }
    }
}
