// -- FILE ------------------------------------------------------------------
// name       : CommandToolTipService.cs
// created    : Jani Giannoudis - 2008.04.15
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public static class CommandToolTipService
    {

        // ----------------------------------------------------------------------
        public static string ToolTipKeyGestureFormat
        {
            get { return toolTipKeyGestureFormat; }
            set { toolTipKeyGestureFormat = value; }
        } // ToolTipKeyGestureFormat

        // ----------------------------------------------------------------------
        public static object GetCommandToolTip(Command command, CommandToolTipStyle toolTipStyle)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (toolTipStyle == CommandToolTipStyle.None)
            {
                return null;
            }

            string toolTip = null;

            if (!string.IsNullOrEmpty(command.Description.ToolTip))
            {
                toolTip = command.Description.ToolTip;
            }
            else if (!string.IsNullOrEmpty(command.Description.Description))
            {
                toolTip = command.Description.Description;
            }

            return toolTip;
        } // GetCommandToolTip

        // ----------------------------------------------------------------------
        public static object FormatToolTip(RoutedCommand command, string toolTip, CommandToolTipStyle toolTipStyle)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (toolTipStyle == CommandToolTipStyle.None || string.IsNullOrEmpty(toolTip))
            {
                return null;
            }

            if (toolTipStyle != CommandToolTipStyle.AlwaysWithKeyGesture &&
                    toolTipStyle != CommandToolTipStyle.EnabledWithKeyGesture)
            {
                return toolTip;
            }

            if (command.InputGestures.Count > 0)
            {
                KeyGesture keyGesture = command.InputGestures[0] as KeyGesture;
                if (keyGesture != null && keyGesture.DisplayString.Length > 0)
                {
                    toolTip = string.Format(
                        toolTipKeyGestureFormat,
                        toolTip,
                        keyGesture.DisplayString);
                }
            }

            return toolTip;
        } // FormatToolTip

        // ----------------------------------------------------------------------
        public static void SetShowOnDisabled(DependencyObject dependencyObject, CommandToolTipStyle toolTipStyle)
        {
            if (toolTipStyle == CommandToolTipStyle.Always || toolTipStyle == CommandToolTipStyle.AlwaysWithKeyGesture)
            {
                ToolTipService.SetShowOnDisabled(dependencyObject, true);
            }
        } // SetShowOnDisabled

        // ----------------------------------------------------------------------
        private static string toolTipKeyGestureFormat = "{0} ({1})";

    } // class CommandToolTipService

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
