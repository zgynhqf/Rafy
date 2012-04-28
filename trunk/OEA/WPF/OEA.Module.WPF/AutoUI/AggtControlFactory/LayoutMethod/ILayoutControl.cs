/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using System.Windows.Controls;
using Itenso.Windows.Input;
using OEA.WPF.Command;
using System.Windows.Automation;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 传统布局控件
    /// 
    /// 布局是一个 Control，同时，在它里面可以安排以下内容：
    /// Main, Toolbar, Navigate, Condition, Result, Children 以及其它自定义块。
    /// </summary>
    public interface ILayoutControl
    {
        /// <summary>
        /// 摆放这些组件
        /// </summary>
        /// <param name="components"></param>
        void Arrange(UIComponents components);
    }
}