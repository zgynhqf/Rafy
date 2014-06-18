/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 17:10
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 17:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Rafy.WPF.Controls;

namespace Rafy.WPF
{
    internal static class WPFHelper
    {
        /// <summary>
        /// 在后台不断调用某个方法，直到其成功执行。
        /// 
        /// 注意！此方法会阻断当前的执行逻辑，但是会继续执行界面的渲染。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool InvokeUntil(Func<bool> action)
        {
            int i = 0;

            bool res = false;

            while (!res && i++ < 1000)//1000 进入防止死循环
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new DispatcherOperationCallback(unused =>
                {
                    res = action();
                    return null;
                }), null);
            }

            return res;
        }
    }
}
