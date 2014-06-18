/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120904 13:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120904 13:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF
{
    /// <summary>
    /// WPFLogicalView 工厂
    /// </summary>
    public interface ICustomViewFactory
    {
        /// <summary>
        /// 通过视图元数据构造一个视图控制器。
        /// </summary>
        /// <param name="evm">块内视图元数据</param>
        /// <returns></returns>
        LogicalView CreateView(EntityViewMeta evm);
    }
}