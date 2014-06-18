using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.WPF.Editors;
using System.Windows;

namespace JXC.WPF
{
    class ImagePropertyEditor : PropertyEditor
    {
        protected override FrameworkElement CreateEditingElement()
        {
            var selector = new ImageSelectorControl();

            this.ResetBinding(selector);

            this.AddReadOnlyComponent(selector);

            return selector;
        }

        protected override DependencyProperty BindingProperty()
        {
            return ImageSelectorControl.ImageBytesProperty;
        }
    }
}