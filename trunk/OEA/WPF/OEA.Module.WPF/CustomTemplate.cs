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
    /// 自定义模板
    /// 
    /// 本类主要是为了给上层使用者提供一个统一的扩展接口：
    /// 通过 CreateUICore 方法提供了生成期扩展性、
    /// 通过 OnUIGenerated 方法提供了生成期扩展性。
    /// </summary>
    public abstract class CustomTemplate : CodeBlocksTemplate
    {
        /// <summary>
        /// 通过本模板生成指定实体类型的 UI 控件。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ControlResult CreateUI<TEntity>()
        {
            this.EntityType = typeof(TEntity);

            return this.CreateUI();
        }

        /// <summary>
        /// 通过本模板生成指定实体类型的 UI 控件。
        /// </summary>
        /// <returns></returns>
        public ControlResult CreateUI(Type entityType)
        {
            this.EntityType = entityType;

            return this.CreateUI();
        }

        /// <summary>
        /// 通过本模板生成实体 UI 控件。
        /// </summary>
        /// <returns></returns>
        public ControlResult CreateUI()
        {
            var blocks = this.GetBlocks();

            var ui = this.CreateUICore(blocks);

            this.OnUIGenerated(ui);

            return ui;
        }

        /// <summary>
        /// 创建一个实体控件。
        /// 
        /// 本方法提供了生成期扩展性。
        /// 
        /// （
        /// 重写时注意，可以不使用 AutoUI，但是这样的话，
        /// 界面可能与模板的结构定义并不一致，这会产生一些影响，例如权限系统无法控制。
        /// ）
        /// </summary>
        /// <returns></returns>
        protected virtual ControlResult CreateUICore(AggtBlocks blocks)
        {
            return AutoUI.AggtUIFactory.GenerateControl(blocks);
        }

        /// <summary>
        /// 子类可以重写此方法来添加当前模块中 UI 的初始化逻辑。
        /// 
        /// 当使用自动生成的 UI 时，此方法会被调用。
        /// </summary>
        /// <param name="ui"></param>
        internal protected virtual void OnUIGenerated(ControlResult ui) { }
    }
}