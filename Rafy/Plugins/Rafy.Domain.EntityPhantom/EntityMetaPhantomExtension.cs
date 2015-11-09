/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 10:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    public static class EntityMetaPhantomExtension
    {
        /// <summary>
        /// 启用实体的幽灵（假删除）功能。
        /// </summary>
        /// <param name="meta"></param>
        public static void EnablePhantoms(this EntityMeta meta)
        {
            meta.IsPhantomEnabled = true;

            meta.Property(EntityPhantomExtension.IsPhantomProperty).MapColumn();

            //使用假删除插件后，需要把整个聚合中的所有数据都标识为‘幽灵’状态，所有的聚合子都需要在内存中也进行假删除。
            //如果在插件打开此功能后，这个功能不应该被关闭，否则会影响插件的一些功能。
            meta.DeletingChildrenInMemory = true;
        }

        /// <summary>
        /// 禁用实体的幽灵（假删除）功能。
        /// </summary>
        /// <param name="meta"></param>
        public static void DisablePhantoms(this EntityMeta meta)
        {
            meta.IsPhantomEnabled = false;

            meta.Property(EntityPhantomExtension.IsPhantomProperty).DontMapColumn();

            meta.DeletingChildrenInMemory = false;
        }
    }
}
