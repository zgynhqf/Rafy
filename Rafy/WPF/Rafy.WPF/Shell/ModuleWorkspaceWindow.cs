/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121204 13:56
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121204 13:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF
{
    /// <summary>
    /// 由模块生成的 WorkspaceWindow
    /// </summary>
    public class ModuleWorkspaceWindow : WorkspaceWindow, IDisposable
    {
        internal ModuleWorkspaceWindow() { }

        /// <summary>
        /// 对应的模块元数据
        /// </summary>
        public ModuleMeta ModuleMeta { get; internal set; }

        /// <summary>
        /// 本模块如果是实体对应的相关模块，则这个属性表示本实体模块对应的主视图。
        /// </summary>
        public LogicalView MainView { get; internal set; }

        /// <summary>
        /// 如果本窗体是用一个模板生成的，那么这个属性表示该模板对象。
        /// 否则，返回 null。
        /// </summary>
        public UITemplate Template { get; internal set; }

        /// <summary>
        /// 本窗口内部聚合控件对应的聚合块定义
        /// </summary>
        public AggtBlocks Blocks { get; internal set; }

        /// <summary>
        /// 通过 View 找到外层控件中最近的一个 WorkspaceWindow
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static WorkspaceWindow GetOuterWorkspaceWindow(LogicalView view)
        {
            //这里直接通过控件树关系，而不能只通过视图的父子关系查询，这是因为环绕坏并不是主块的子块。
            return GetOuterWorkspaceWindow(view.GetRootView().Control as DependencyObject);
        }

        /// <summary>
        /// 内存泄漏，尽量断开连接。
        /// </summary>
        public override void Dispose()
        {
            if (this.MainView != null)
            {
                this.MainView.Dispose();
                this.MainView = null;
            }

            base.Dispose();
        }
    }
}