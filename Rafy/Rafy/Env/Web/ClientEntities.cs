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
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.Web
{
    /// <summary>
    /// 所有实体类在 Web 客户端的查询类、容器
    /// </summary>
    public static class ClientEntities
    {
        private static object _entitiesLock = new object();

        private static Dictionary<string, EntityMeta> _entities;

        /// <summary>
        /// 通过客户端命令来查找对应实体的元数据。
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <returns></returns>
        public static EntityMeta Find(string clientName)
        {
            EntityMeta result = null;
            GetEntities().TryGetValue(clientName, out result);
            return result;
        }

        /// <summary>
        /// 获取某个实体类型在客户端的名称。
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetClientName(Type entityType)
        {
            return entityType.FullName;
        }

        internal static Dictionary<string, EntityMeta> GetEntities()
        {
            if (_entities == null)
            {
                lock (_entitiesLock)
                {
                    if (_entities == null)
                    {
                        var dic = new Dictionary<string, EntityMeta>();

                        CommonModel.Entities.EnsureAllLoaded();

                        foreach (var em in CommonModel.Entities)
                        {
                            dic.Add(GetClientName(em.EntityType), em);
                            //throw new InvalidOperationException(string.Format("存在生成两个同名客户端类型的类型：{0}、{1}。", em.EntityType.FullName, t.FullName));
                        }

                        _entities = dic;
                    }
                }
            }

            return _entities;
        }
    }
}