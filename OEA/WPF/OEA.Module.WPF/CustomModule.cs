/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using OEA.MetaModel;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 自定义模块类型
    /// </summary>
    public abstract class CustomModule : CodeBlocksTemplate
    {
        /// <summary>
        /// 当前模块对应的模块元数据
        /// </summary>
        public ModuleMeta ModuleMeta { get; internal set; }

        /// <summary>
        /// 创建一个实体工作区窗体。
        /// 
        /// 子类可重写此方法以实现自己的实体工作区窗体逻辑。
        /// （
        /// 重写时注意，如何使用了 AutoUI，则应该在生成控件完毕后调用 OnUIGenerated 方法。
        /// 当然，也可以不使用 AutoUI，但是这样的话，界面可能与模板的结构定义并不一致，这会产生一些影响，例如权限系统无法控制。
        /// ）
        /// </summary>
        /// <returns></returns>
        internal protected virtual IEntityWindow CreateWindow()
        {
            var blocks = this.GetBlocks();

            var ui = AutoUI.AggtUIFactory.GenerateControl(blocks);

            this.OnUIGenerated(ui);

            var title = this.ModuleMeta.Label;

            return new EntityModule(ui, title);
        }

        /// <summary>
        /// 子类可以重写此方法来添加当前模块中 UI 的初始化逻辑。
        /// 
        /// 当使用自动生成的 UI 时，此方法会被调用。
        /// </summary>
        /// <param name="ui"></param>
        protected virtual void OnUIGenerated(ControlResult ui) { }
    }
}