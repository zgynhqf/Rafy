﻿#pragma checksum "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "AE4E7BF1CD71B1AC0D4EC5055C9097DA"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Rafy.WPF.Editors {
    
    
    /// <summary>
    /// DateRangePropertyEditorControl
    /// </summary>
    public partial class DateRangePropertyEditorControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 15 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblStart;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker dpStart;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblEnd;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker dpEnd;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Rafy.WPF;component/runtime/editor/propertyeditor/daterangepropertyeditorcontrol." +
                    "xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.lblStart = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.dpStart = ((System.Windows.Controls.DatePicker)(target));
            
            #line 16 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
            this.dpStart.SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.date_SelectedDateChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.lblEnd = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.dpEnd = ((System.Windows.Controls.DatePicker)(target));
            
            #line 18 "..\..\..\..\..\Runtime\Editor\PropertyEditor\DateRangePropertyEditorControl.xaml"
            this.dpEnd.SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.date_SelectedDateChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
