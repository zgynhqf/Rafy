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

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 聚合块模板。
    /// 
    /// 模板是同一种视图结构的抽象定义。
    /// 注意：此类及其子类不是线程安全的。
    /// </summary>
    public abstract class BlocksTemplate
    {
        /// <summary>
        /// 当前模板显示的实体类型
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 获取当前模板的结构定义。
        /// </summary>
        /// <returns></returns>
        public AggtBlocks GetBlocks()
        {
            if (this.EntityType == null) throw new ArgumentNullException("this.MainType");

            var blocks = this.DefineBlocks();

            this.OnBlocksDefined(blocks);

            return blocks;
        }

        /// <summary>
        /// 子类实现：获取当前模板的结构定义。
        /// </summary>
        /// <returns></returns>
        protected abstract AggtBlocks DefineBlocks();

        /// <summary>
        /// 定义完成后的事件。
        /// </summary>
        public event EventHandler<BlocksDefinedEventArgs> BlocksDefined;

        protected virtual void OnBlocksDefined(AggtBlocks blocks)
        {
            var handler = this.BlocksDefined;
            if (handler != null) handler(this, new BlocksDefinedEventArgs(blocks));
        }

        /// <summary>
        /// 由于模块并不是线程安全的，所以提供 Clone 方法，方便复杂模块
        /// </summary>
        /// <returns></returns>
        public BlocksTemplate Clone()
        {
            var template = Activator.CreateInstance(this.EntityType) as BlocksTemplate;
            template.EntityType = this.EntityType;
            return template;
        }
    }

    public class BlocksDefinedEventArgs : EventArgs
    {
        public BlocksDefinedEventArgs(AggtBlocks blocks)
        {
            this.Blocks = blocks;
        }

        /// <summary>
        /// 定义好的聚合块
        /// </summary>
        public AggtBlocks Blocks { get; private set; }
    }
}