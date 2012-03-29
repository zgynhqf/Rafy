/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110422
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using System.Collections;

namespace OEA.Library
{
    public static class EntityExtension
    {
        public static IDb CreateDb(this Entity entity)
        {
            return entity.FindRepository().CreateDb();
        }

        public static IDb CreateDb(this EntityList entityList)
        {
            return entityList.FindRepository().CreateDb();
        }

        /// <summary>
        /// 尝试重设 Entity 的 ParentEntity 属性。
        /// 逻辑如下：
        /// 尝试把 Entity.ParentList.Parent 属性中的值，调用 SetParentEntity 方法设置为当前对象的父属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool ResetParentEntity(this Entity entity)
        {
            var parentList = entity.ParentList;
            if (parentList != null)
            {
                var parentEntity = parentList.Parent;
                if (parentEntity != null)
                {
                    //有 ParentList.Parent 属性，则必然有 ParentProperty，所以可以直接调用以下方法进行设置。
                    entity.SetParentEntity(parentEntity);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 创建一个列表。
        /// 列表的数据来自于 srcList 中的所有项。
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="resetParent">此参数表示是否需要把 srcList 中的每一项的父列表对象都设置为返回的新列表。</param>
        /// <returns></returns>
        public static EntityList CreateList(
            this EntityRepository repository,
            IEnumerable srcList, bool resetParent = true
            )
        {
            var list = repository.OldList();

            list.SupressSetItemParent = !resetParent;
            list.RaiseListChangedEvents = false;

            foreach (Entity item in srcList) { list.Add(item); }

            list.RaiseListChangedEvents = true;

            return list;
        }

        /// <summary>
        /// 把这个实体中的所有改动保存到仓库中。
        /// </summary>
        /// <param name="component"></param>
        public static IEntityOrList Save(this Entity component)
        {
            return component.FindRepository().Save(component);
        }

        /// <summary>
        /// 把这个列表中的所有改动保存到仓库中。
        /// </summary>
        /// <param name="component"></param>
        public static IEntityOrList Save(this EntityList component)
        {
            return component.FindRepository().Save(component);
        }
    }
}