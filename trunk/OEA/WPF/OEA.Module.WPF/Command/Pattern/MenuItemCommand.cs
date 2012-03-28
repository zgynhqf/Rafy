// -- FILE ------------------------------------------------------------------
// name       : MenuItemCommand.cs
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
using OEA.WPF.Command;
using System.Windows.Automation;
using OEA.Module.WPF;

namespace Itenso.Windows.Input
{

    // ------------------------------------------------------------------------
    public class MenuItemCommand
    {

        // ----------------------------------------------------------------------
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(MenuItemCommand),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCommandInvalidated)));

        // ----------------------------------------------------------------------
        public static bool UseCommandImage
        {
            get { return useCommandImage; }
            set { useCommandImage = value; }
        } // UseCommandImage

        // ----------------------------------------------------------------------
        public static CommandToolTipStyle ToolTipStyle
        {
            get { return toolTipStyle; }
            set { toolTipStyle = value; }
        } // ToolTipStyle

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
        private static void OnCommandInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MenuItem menuItem = dependencyObject as MenuItem;
            if (menuItem == null)
            {
                throw new InvalidOperationException("menu item required");
            }

            ICommand newCommand = e.NewValue as ICommand;
            if (newCommand == null)
            {
                throw new InvalidOperationException("command required");
            }
            menuItem.Command = newCommand;

            RoutedCommand routedCommand = newCommand as RoutedCommand;
            Command command = newCommand as Command;
            //add by zhoujg
            menuItem.Name = "mi" + command.Name.Replace(".", "_");
            menuItem.CommandBindings.Add(CommandRepository.CreateCommandBinding(command));
            //支持UI Test
            var adapter = newCommand as CommandAdapter;
            if (adapter != null)
            {
                //支持UI Test
                menuItem.SetValue(AutomationProperties.NameProperty, adapter.CoreCommand.Label);
                menuItem.SetBinding(ContentControl.ContentProperty, "Label");
                menuItem.DataContext = adapter.CoreCommand;
            }

            // image
            if (useCommandImage && menuItem.Icon == null && command != null)
            {
                Uri imageUri = CommandImageService.GetCommandImageUri(command);
                if (imageUri != null)
                {
                    try
                    {
                        MenuItemImage image = new MenuItemImage();
                        BitmapImage bitmap = new BitmapImage(imageUri);
                        // if (bitmap.CanFreeze) bitmap.Freeze();    //Freeze image it to avoid memory leak
                        image.Stretch = Stretch.None;  //图形不拉伸
                        image.Source = bitmap;
                        menuItem.Icon = image;
                    }
                    catch { }
                }
            }

            // tooltip
            if (menuItem.ToolTip == null && command != null)
            {
                menuItem.ToolTip = CommandToolTipService.GetCommandToolTip(command, toolTipStyle);
            }
            if (menuItem.ToolTip is string && routedCommand != null)
            {
                menuItem.ToolTip = CommandToolTipService.FormatToolTip(routedCommand, menuItem.ToolTip as string, toolTipStyle);
            }
            CommandToolTipService.SetShowOnDisabled(menuItem, toolTipStyle);
        } // OnCommandInvalidated

        // ----------------------------------------------------------------------
        // members
        private static bool useCommandImage = true;
        private static CommandToolTipStyle toolTipStyle = CommandToolTipStyle.None;

    } // class MenuItemCommand

} // namespace Itenso.Windows.Input
// -- EOF -------------------------------------------------------------------
