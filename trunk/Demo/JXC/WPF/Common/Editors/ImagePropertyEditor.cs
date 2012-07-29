using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF.Editors;
using System.Windows;

namespace JXC.WPF
{
    class ImagePropertyEditor : WPFPropertyEditor
    {
        protected override FrameworkElement CreateEditingElement()
        {
            var selector = new ImageSelectorControl();

            this.ResetBinding(selector);

            this.BindElementReadOnly(selector);

            return selector;
        }

        protected override DependencyProperty BindingProperty()
        {
            return ImageSelectorControl.ImageBytesProperty;
        }
    }
}