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
using Rafy.Domain;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Rafy.Web.EntityDataPortal
{
    /// <summary>
    /// 树型列表的读取器
    /// </summary>
    internal class TreeEntityListReader : ListReader
    {
        protected override void ReadCore()
        {
            //读取 Create、Update 的列表。
            this.ReadRootList(this.ChangeSet, this.ResultEntityList);

            this.ReadDeleteList();
        }

        protected void ReadRootList(JObject changeSet, IEntityList list)
        {
            var p = changeSet.Property("roots");
            if (p != null)
            {
                var roots = p.Value as JArray;

                var oldValue = list.AutoTreeIndexEnabled;
                try
                {
                    list.AutoTreeIndexEnabled = false;

                    foreach (JObject root in roots)
                    {
                        this.ReadTreeEntityRecur(list, root, null);
                    }
                }
                finally
                {
                    list.AutoTreeIndexEnabled = oldValue;
                }

                list.ResetTreeIndex();
            }
        }

        protected void ReadTreeEntityRecur(IList<Entity> list, JObject item, Entity treeParent)
        {
            //先把 TreeChildren、isNew 两个属性从集合中删除，这样后面拷贝所有属性时就不用拷贝这两个属性。
            var treeChildrenProperty = item.Property("TreeChildren");
            if (treeChildrenProperty != null) { treeChildrenProperty.Remove(); }
            var newEntityMark = item.Property("isNew");
            var isNew = newEntityMark != null;
            if (isNew) { newEntityMark.Remove(); }

            var e = this.Repository.New();

            if (treeParent != null)
            {
                //先设置 TreeParent，再拷贝所有属性。
                //（可以保证在后面设置 TreePId 时，不会引发数据查询。）
                e.TreeParent = treeParent;
            }
            else
            {
                list.Add(e);
            }

            //拷贝所有属性
            _setter.SetEntity(e, item);

            //如果有子节点，则转换加载所有子节点。
            if (treeChildrenProperty != null)
            {
                var treeChildren = treeChildrenProperty.Value as JArray;
                foreach (JObject treeChild in treeChildren)
                {
                    this.ReadTreeEntityRecur(list, treeChild, e);
                }
            }
            e.TreeChildren.MarkLoaded();

            //最后再清空节点的 IsNew 状态，使得节点在添加它的子节点时，TreeChildren 集合不会执行懒加载。
            if (!isNew)
            {
                e.PersistenceStatus = PersistenceStatus.Modified;
            }
        }

        private void ReadDeleteList()
        {
            var toDelete = new List<Entity>();
            this.ReadList(this.ChangeSet, "d", toDelete);
            if (toDelete.Count > 0)
            {
                this.ResultEntityList.AutoTreeIndexEnabled = false;
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