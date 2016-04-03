/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130514
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130514 12:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// Web 界面中的实体视图元数据。
    /// </summary>
    public class WebEntityViewMeta : EntityViewMeta
    {
        private int _PageSize = 25;
        /// <summary>
        /// 超过 10000 就不分页了。
        /// </summary>
        public int PageSize
        {
            get { return this._PageSize; }
            set { this.SetValue(ref this._PageSize, value); }
        }

        private WebCommandCollection _Commands = new WebCommandCollection();
        /// <summary>
        /// 这个界面块中可用的 Web 命令。
        /// </summary>
        public WebCommandCollection Commands
        {
            get { return _Commands; }
        }

        private IList<EntityPropertyViewMeta> _LockedProperty = new List<EntityPropertyViewMeta>();
        /// <summary>
        /// Web 中需要锁定的列对应的属性列表。
        /// </summary>
        public IList<EntityPropertyViewMeta> LockedProperties
        {
            get { return this._LockedProperty; }
        }

        internal override EntityPropertyViewMeta CreatePropertyViewMeta()
        {
            return new WebEntityPropertyViewMeta();
        }

        /// <summary>
        /// 根据名字查询实体属性
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public new WebEntityPropertyViewMeta Property(IManagedProperty property)
        {
            return base.Property(property) as WebEntityPropertyViewMeta;
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new WebEntityPropertyViewMeta Property(string name)
        {
            return base.Property(name) as WebEntityPropertyViewMeta;
        }
    }
}