using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AvalonDock;

namespace OEA.Module.WPF
{
    public static class ResizingPanelExt
    {
        public static readonly DependencyProperty StarGridLengthProperty = DependencyProperty.RegisterAttached(
            "StarGridLength", typeof(double), typeof(ResizingPanelExt), new PropertyMetadata(StarGridLengthPropertyChangedCallback)
            );

        public static double GetStarGridLength(DependencyObject d)
        {
            return (double)d.GetValue(StarGridLengthProperty);
        }

        public static void SetStarGridLength(DependencyObject d, double value)
        {
            d.SetValue(StarGridLengthProperty, value);
        }

        private static void StarGridLengthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = new GridLength((double)e.NewValue, GridUnitType.Star);
            ResizingPanel.SetResizeHeight(d, value);
            ResizingPanel.SetResizeWidth(d, value);
        }
    }
}
