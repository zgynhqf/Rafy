using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Globalization;

namespace Rafy.WPF
{
    /// <summary>
    /// 皮肤管理器
    /// </summary>
    public static class SkinManager
    {
        /// <summary>
        /// 当前加载的主题名
        /// </summary>
        private static IList<ResourceDictionary> _currentDics;

        /// <summary>
        /// 当前正在使用的皮肤。
        /// </summary>
        public static string Current { get; private set; }

        /// <summary>
        /// 获取当前可用的所有皮肤
        /// </summary>
        /// <returns></returns>
        public static string[] GetSkins()
        {
            return new string[]
            { 
                "Blue","Gray", "Yellow"
                //"ExpressionDark", "ExpressionLight", "RainierOrange", "RainierPurple", "RainierRadialBlue", 
                //"ShinyBlue", "ShinyRed", "ShinyDarkTeal",  "ShinyDarkGreen", "ShinyDarkPurple",
                //"DavesGlossyControls", "WhistlerBlue", "BureauBlack", "BureauBlue", "BubbleCreme", 
                //"UXMusingsRed", "UXMusingsGreen", "UXMusingsRoughRed", "UXMusingsRoughGreen", "UXMusingsBubblyBlue"
            };
        }

        /// <summary>
        /// 应用指定的主题.
        /// </summary>
        /// <param name="skin"></param>
        public static void Apply(string skin)
        {
            var app = Application.Current;
            if (app == null) return;

            var resourceList = app.Resources.MergedDictionaries;

            if (_currentDics != null)
            {
                foreach (var skinResources in _currentDics) { resourceList.Remove(skinResources); }
            }

            var skins = GetSkinResourceDictionary(skin);
            foreach (var skinResource in skins) { resourceList.Add(skinResource); }

            _currentDics = skins;
            Current = skin;
        }

        private static IList<ResourceDictionary> GetSkinResourceDictionary(string skin)
        {
            var list = new List<ResourceDictionary>();

            foreach (var modulePlugin in RafyEnvironment.AllPlugins)
            {
                string packUri = string.Format(@"Resources/Colors/{0}.xaml", skin);
                var uri = Helper.GetPackUri(modulePlugin.Assembly, packUri);
                try
                {
                    var rd = Application.LoadComponent(uri) as ResourceDictionary;
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
