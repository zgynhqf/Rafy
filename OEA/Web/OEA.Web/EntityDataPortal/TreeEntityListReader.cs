/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace OEA.Web.EntityDataPortal
{
    /// <summary>
    /// 树型列表的读取器
    /// </summary>
    internal class TreeEntityListReader : ListReader
    {
        protected override void ReadCore()
        {
            this.ResultEntityList = this.Repository.NewList();

            //读取 Create、Update 的列表。
            this.ReadRootList(this.ChangeSet, this.ResultEntityList);

            this.ReadDeleteList();
        }

        protected void ReadRootList(JObject changeSet, IList<Entity> list)
        {
            var p = changeSet.Property("roots");
            if (p != null)
            {
                var roots = p.Value as JArray;
                foreach (JObject root in roots)
                {
                    this.ReadTreeEntityRecur(list, root, null);
                }
            }
        }

        protected void ReadTreeEntityRecur(IList<Entity> list, JObject item, Entity treeParent)
        {
            var treeChildrenProperty = item.Property("TreeChildren");
            if (treeChildrenProperty != null) { treeChildrenProperty.Remove(); }

            var newEntityMark = item.Property("isNew");
            if (newEntityMark != null) { newEntityMark.Remove(); }

            //先把当前对象加入集合中。
            var e = this.Repository.New();
            if (newEntityMark == null) { e.Status = PersistenceStatus.Unchanged; }
            this._setter.SetEntity(e, item);
            if (treeParent != null)
            {
                e.TreeParent = treeParent;
            }
            else
            {
                e.TreePId = null;
                list.Add(e);
            }

            if (treeChildrenProperty != null)
            {
                var treeChildren = treeChildrenProperty.Value as JArray;
                foreach (JObject treeChild in treeChildren)
                {
                    this.ReadTreeEntityRecur(list, treeChild, e);
                }
            }
        }

        private void ReadDeleteList()
        {
            var toDelete = new List<Entity>();
            this.ReadList(this.ChangeSet, "toDelete", toDelete);
            if (toDelete.Count > 0)
            {
                this.ResultEntityList.AutoTreeCodeEnabled = false;
                foreach (var root in toDelete)
                {
                    //先加入，最后会再移除
                    //加入到 Remove 列表中，这样就可以在保存时删除了。
                    this.ResultEntityList.Add(root);
                    this.ResultEntityList.Remove(root);
                }
            }
        }
    }
}