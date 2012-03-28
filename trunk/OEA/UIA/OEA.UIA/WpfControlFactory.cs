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
using Microsoft.VisualStudio.TestTools.UITesting;

namespace OEA.UIA
{
    public static class WpfControlFactory
    {
        public static TControl Create<TControl>(string title, WpfControl context = null)
            where TControl : WpfControl, new()
        {
            //if (TestContext.Current.NeedCancel) Playback.Cancel();
            if (TestContext.Current.NeedCancel) throw new StopUIAException("停止自动化测试！");

            var control = new TControl();
            if (context != null)
            {
                control.Container = context;
            }

            if (!string.IsNullOrEmpty(title))
            {
                control.SearchProperties[WpfControl.PropertyNames.Name] = title;
            }

            return control;
        }

        public static TControl CreateById<TControl>(string id, WpfControl context = null)
            where TControl : WpfControl, new()
        {
            var control = new TControl();
            if (context != null)
            {
                control.Container = context;
            }

            if (!string.IsNullOrEmpty(id))
            {
                control.SearchProperties[WpfControl.PropertyNames.AutomationId] = id;
            }

            return control;
        }
    }
}