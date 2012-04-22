using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Globalization;

namespace OEA.Module.WPF
{
    public static class ThemeManager
    {
        #region 可以直接用于绑定的 DP 附加属性

        public static readonly DependencyProperty CurrentThemeProperty = DependencyProperty.RegisterAttached(
            "CurrentTheme", typeof(string), typeof(ThemeManager),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnCurrentThemeChanged))
            );

        public static string GetCurrentTheme(DependencyObject d)
        {
            return (string)d.GetValue(CurrentThemeProperty);
        }

        public static void SetCurrentTheme(DependencyObject d, string value)
        {
            d.SetValue(CurrentThemeProperty, value);
        }

        private static void OnCurrentThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string theme = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(theme)) { return; }

            //为整个应用程序应用主题
            ApplyTheme(theme);
        }

        #endregion

        public static string[] GetThemes()
        {
            return new string[]
            { 
                "White","Blue",
                //"ExpressionDark", "ExpressionLight", "RainierOrange", "RainierPurple", "RainierRadialBlue", 
                //"ShinyBlue", "ShinyRed", "ShinyDarkTeal",  "ShinyDarkGreen", "ShinyDarkPurple",
                //"DavesGlossyControls", "WhistlerBlue", "BureauBlack", "BureauBlue", "BubbleCreme", 
                //"UXMusingsRed", "UXMusingsGreen", "UXMusingsRoughRed", "UXMusingsRoughGreen", "UXMusingsBubblyBlue"
            };
        }

        /// <summary>
        /// 当前加载的主题名
        /// </summary>
        private static IList<ResourceDictionary> _currentThemes;

        private static void ApplyTheme(string theme)
        {
            var app = Application.Current;
            if (app == null) return;

            var resourceList = app.Resources.MergedDictionaries;

            if (_currentThemes != null)
            {
                foreach (var themeResources in _currentThemes) { resourceList.Remove(themeResources); }
            }

            var themes = ThemeManager.GetThemeResourceDictionary(theme);
            foreach (var themeResources in themes) { resourceList.Add(themeResources); }

            _currentThemes = themes;
        }

        private static IList<ResourceDictionary> GetThemeResourceDictionary(string theme)
        {
            var list = new List<ResourceDictionary>();

            foreach (var modulePlugin in OEAEnvironment.GetAllPlugins())
            {
                var name = modulePlugin.Assembly.GetName().Name;
                string packUri = string.Format(@"/{0};component/Themes/{1}/Theme.xaml", name, theme);
                try
                {
                    var rd = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
                    list.Add(rd);
                }
                catch (IOException)
                {
                    //如果该 Module 中没有皮肤文件时，则定位不到皮肤，会发生此 IO 异常。
                }
            }

            return list;
        }
    }
}
