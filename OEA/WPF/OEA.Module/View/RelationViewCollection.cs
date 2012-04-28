/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110328
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OEA
{
    /// <summary>
    /// 关系视图集合
    /// </summary>
    public class RelationViewCollection : Collection<RelationView>
    {
        private ObjectView _owner;

        internal RelationViewCollection(ObjectView owner)
        {
            if (owner == null) throw new ArgumentNullException("owner");

            this._owner = owner;
        }

        public ObjectView Owner
        {
            get { return this._owner; }
        }

        protected override void InsertItem(int index, RelationView item)
        {
            base.InsertItem(index, item);

            item.Owner = this._owner;
        }

        protected override void SetItem(int index, RelationView item)
        {
            base.SetItem(index, item);

            item.Owner = this._owner;
        }

        /// <summary>
        /// 获取某一个关系视图
        /// </summary>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public ObjectView this[string relationType]
        {
            get
            {
                var res = this.Find(relationType);
                if (res == null) throw new InvalidOperationException("没有找到指定的关系：" + relationType);
                return res;
            }
        }

        /// <summary>
        /// 尝试找到某一个关系视图
        /// </summary>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public ObjectView Find(string relationType)
        {
            if (this.Count > 0)
            {
                var surrounder = this.FirstOrDefault(s => s.SurrounderType == relationType);
                if (surrounder != null) { return surrounder.View; }
            }

            return null;
        }

        /// <summary>
        /// 找到所有的指定关系视图
        /// </summary>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IEnumerable<ObjectView> FindAll(string relationType)
        {
            return this.Where(s => s.SurrounderType == relationType).Select(r => r.View);
        }
    }
}