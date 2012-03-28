// -- FILE ------------------------------------------------------------------
// name       : ButtonCommand.cs
// created    : Jani Giannoudis - 2008.04.15
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Automation;
using OEA.WPF.Command;
using OEA.WPF;
using System.Collections.Generic;
using OEA.Module.WPF;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public class ButtonCommand
    {
        // ----------------------------------------------------------------------
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(ButtonCommand),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCommandInvalidated)));

        // ----------------------------------------------------------------------
        public static bool UseCommandImage
        {
            get { return useCommandImage; }
            set { useCommandImage = value; }
        } // UseCommandImage

        // ----------------------------------------------------------------------
        public static bool ShowDisabledState
        {
            get { return showDisabledState; }
            set { showDisabledState = value; }
        } // ShowDisabledState

        // ----------------------------------------------------------------------
        public static CommandToolTipStyle ToolTipStyle
        {
            get { return toolTipStyle; }
            set { toolTipStyle = value; }
        } // ToolTipStyle

        // ----------------------------------------------------------------------
        public static double DisabledOpacity
        {
            get { return disabledOpacity; }
            set { disabledOpacity = value; }
        } // DisabledOpacity

        // ----------------------------------------------------------------------
        public static ICommand GetCommand(DependencyObject sender)
        {
            return sender.GetValue(CommandProperty) as ICommand;
        } // GetCommand

        // ----------------------------------------------------------------------
        public static void SetCommand(DependencyObject sender, ICommand command)
        {
            sender.SetValue(CommandProperty, command);
        } // SetCommand

        // ----------------------------------------------------------------------
        private static void UpdateButton(Button button)
        {
            UIElement uiElement = button.Content as UIElement;
            if (uiElement != null)
            {
                uiElement.Opacity = button.IsEnabled ? 1.0 : disabledOpacity;
            }
        } // UpdateButton

        // ----------------------------------------------------------------------
        private static void OnCommandInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Button button = dependencyObject as Button;
            if (button == null)
            {
                throw new InvalidOperationException("button required");
            }

            ICommand newCommand = e.NewValue as ICommand;
            if (newCommand == null)
            {
                throw new InvalidOperationException("command required");
            }
            button.Command = newCommand;

            RoutedCommand routedCommand = newCommand as RoutedCommand;
            Command command = newCommand as Command;
            //add by zhoujg
            button.Name = "btn" + command.Name.Replace(".", "_");
            button.CommandBindings.Add(CommandRepository.CreateCommandBinding(command));

            var adapter = newCommand as CommandAdapter;
            if (adapter != null)
            {
                //支持UI Test
                button.SetValue(AutomationProperties.NameProperty, adapter.CoreCommand.Label);
                button.SetBinding(ContentControl.ContentProperty, "Label");
                button.DataContext = adapter.CoreCommand;
            }

            // image
            if (useCommandImage && button.Content == null && command != null)
            {
                Uri imageUri = CommandImageService.GetCommandImageUri(command);
                if (imageUri != null)
                {
                    try
                    {
                        ButtonImage image = new ButtonImage();
                        BitmapImage bitmap = new BitmapImage(imageUri);
                        //if (bitmap.CanFreeze) bitmap.Freeze();    //Freeze bitmap it to avoid memory leak
                        image.Source = bitmap;
                        image.Stretch = Stretch.None;  //图形不拉伸
                        StackPanel p = new StackPanel() { Orientation = Orientation.Horizontal };
                        button.Content = p;
                        p.Children.Add(image);
                        p.Children.Add(new Label() { Content = command.Description.Text, Padding = new Thickness(0) });
                    }
                    catch { }
                }
            }

            // tooltip
            if (button.ToolTip == null && command != null)
            {
                button.ToolTip = CommandToolTipService.GetCommandToolTip(command, toolTipStyle);
            }
            if (button.ToolTip is string && routedCommand != null)
            {
                button.ToolTip = CommandToolTipService.FormatToolTip(routedCommand, button.ToolTip as string, toolTipStyle);
            }
            CommandToolTipService.SetShowOnDisabled(button, toolTipStyle);

            if (showDisabledState)
            {
                UpdateButton(button);
                //memory leak, use PropertyChangeNotifier
                //DependencyPropertyDescriptor.FromProperty(
                //    Button.IsEnabledProperty,
                //    typeof(Button)).AddValueChanged(button, ButtonIsEnabledChanged);
                PropertyChangeNotifier notifier = new PropertyChangeNotifier(button, "IsEnabled");
                notifier.ValueChanged += new EventHandler(ButtonIsEnabledChanged);
                notifiers.Add(notifier);
            }
        }

        private static IList<PropertyChangeNotifier> notifiers = new List<PropertyChangeNotifier>();

        // ----------------------------------------------------------------------
        private static void ButtonIsEnabledChanged(object sender, EventArgs e)
        {
            Button button = (sender as PropertyChangeNotifier).PropertySource as Button;
            //Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            UpdateButton(button);
        } // ButtonIsEnabledChanged

        // ----------------------------------------------------------------------
        // members
        private static bool useCommandImage = true;
        private static bool showDisabledState = true;
        private static CommandToolTipStyle toolTipStyle = CommandToolTipStyle.Enabled;
        private static double disabledOpacity = 0.25;

    } // class ButtonCommand

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
