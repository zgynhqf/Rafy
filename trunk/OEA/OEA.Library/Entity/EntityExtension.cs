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
            return entity.GetRepository().CreateDb();
        }

        public static IDb CreateDb(this EntityList entityList)
        {
            return entityList.GetRepository().CreateDb();
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
            var list = repository.NewList();

            list.SupressSetItemParent = !resetParent;
            list.RaiseListChangedEvents = false;

            foreach (Entity item in srcList) { list.Add(item); }

            list.RaiseListChangedEvents = true;

            return list;
        }
    }
}