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
    /// 模板是同一种视图结构的抽象定义
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

            return this.DefineBlocks();
        }

        /// <summary>
        /// 子类实现：获取当前模板的结构定义。
        /// </summary>
        /// <returns></returns>
        protected abstract AggtBlocks DefineBlocks();
    }
}