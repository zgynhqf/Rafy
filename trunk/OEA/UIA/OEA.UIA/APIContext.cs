/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111201
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using OEA.UIA.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.Windows.Input;
using System.Threading;
using System.IO;

namespace OEA.UIA
{
    /// <summary>
    /// 继承此类：可以直接使用许多定义好的静态方法。
    /// </summary>
    public abstract class APIContext
    {
        public static WpfWindow 主窗口
        {
            get { return TestContext.Current.MainWindow; }
        }

        public static WpfWindow 当前窗口
        {
            get { return TestContext.Current.CurrentWindow; }
        }

        #region 当前测试对象

        public static void 设置当前测试对象(WpfControl item)
        {
            TestContext.Current.ControlContexts.Push(item);
            LogLine("→进入→： " + item.FriendlyName);
        }

        public static void 取消当前测试对象()
        {
            var control = TestContext.Current.ControlContexts.Pop();
            LogLine("←回到←： " + 当前测试对象.FriendlyName);
        }

        public static WpfControl 当前测试对象
        {
            get
            {
                var controls = TestContext.Current.ControlContexts;
                return controls.Count > 0 ? controls.Peek() : 主窗口;
            }
        }

        public static IDisposable 进入(WpfControl item)
        {
            设置当前测试对象(item);
            return new CurrentTestWrapper()
            {
                _item = item
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">当前测试对象</param>
        /// <param name="action">以 item 为环境运行的代码</param>
        /// <returns></returns>
        public static WpfControl 进入(WpfControl item, Action<WpfControl> action)
        {
            using (进入(item)) { action(item); }

            return item;
        }

        private class CurrentTestWrapper : IDisposable
        {
            internal WpfControl _item;

            public void Dispose()
            {
                if (当前测试对象 == this._item) 取消当前测试对象();
            }
        }

        #endregion

        #region 窗口、模块

        public static WpfWindow 弹出窗口()
        {
            var curWin = 当前窗口;
            var popupWin = curWin.Find<WpfWindow>();
            popupWin.SearchProperties[WpfControl.PropertyNames.ClassName] = "Popup";
            if (popupWin.Exists) return popupWin;

            return curWin;
        }

        public static WpfWindow 窗口(string title)
        {
            return WpfControlFactory.Create<WpfWindow>(title);
        }

        public static WpfTabPage OpenModule(string title)
        {
            主窗口.菜单(title).单击();

            var lastTitle = title.SplitBy(".").Last();

            return 主窗口.页签(lastTitle);
        }

        #endregion

        #region 代理到当前测试对象的静态方法

        public static WpfButton 点击按钮(string title)
        {
            return 当前测试对象.点击按钮(title);
        }

        public static WpfButton 点击按钮(WpfButton btn)
        {
            Logger.LogLine("点击按钮：" + btn.Name);

            btn.单击();

            return btn;
        }

        public static WpfMenuItem 菜单(string title = null)
        {
            return 当前测试对象.菜单(title);
        }

        public static WpfTabPage 页签(string title = null)
        {
            return 当前测试对象.页签(title);
        }

        public static WpfTree 列表(string pathes = null)
        {
            return 当前测试对象.列表(pathes);
        }

        public static WpfTree 树形列表(string pathes = null)
        {
            return 当前测试对象.树形列表(pathes);
        }

        public static WpfButton 按钮(string title = null)
        {
            return 当前测试对象.按钮(title);
        }

        public static WpfEdit 属性编辑器(string title = null)
        {
            return 当前测试对象.属性编辑器(title);
        }

        public static WpfDatePicker 属性编辑器_日期(string title = null)
        {
            return 当前测试对象.属性编辑器_日期(title);
        }

        public static WpfComboBox 属性编辑器_枚举(string title = null)
        {
            return 当前测试对象.属性编辑器_枚举(title);
        }

        public static WpfComboListControl 属性编辑器_下拉列表(string title = null)
        {
            return 当前测试对象.属性编辑器_下拉列表(title);
        }

        public static WpfCheckBox 属性编辑器_勾选框(string title = null)
        {
            return 当前测试对象.属性编辑器_勾选框(title);
        }

        #endregion

        #region 方便的方法

        public static void 按住Ctrl()
        {
            Keyboard.PressModifierKeys(ModifierKeys.Control);
        }

        public static void 释放Ctrl()
        {
            Keyboard.ReleaseModifierKeys(ModifierKeys.Control);
        }

        public static void 按住Shift()
        {
            Keyboard.PressModifierKeys(ModifierKeys.Shift);
        }

        public static void 释放Shift()
        {
            Keyboard.ReleaseModifierKeys(ModifierKeys.Shift);
        }

        public static void 回车()
        {
            Keyboard.SendKeys("{ENTER}");
        }

        public static void 等待(double time)
        {
            Playback.Wait((int)(time * 1000));
            //Thread.Sleep((int)(time * 1000));
        }

        public static void LogLine(string message)
        {
            Logger.LogLine(message);
        }

        #endregion
    }
}