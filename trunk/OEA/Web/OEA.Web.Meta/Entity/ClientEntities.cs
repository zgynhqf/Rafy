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
using OEA.MetaModel;
using OEA.Web.Json;
using OEA.Web.ClientMetaModel;
using OEA.MetaModel.View;

namespace OEA.Web
{
    /// <summary>
    /// 所有客户端类的查询类、容器
    /// </summary>
    public static class ClientEntities
    {
        private static object _entitiesLock = new object();

        private static Dictionary<string, EntityMeta> _entities;

        public static EntityMeta Find(string clientName)
        {
            EntityMeta result = null;
            GetEntities().TryGetValue(clientName, out result);
            return result;
        }

        public static string GetClientName(Type entityType)
        {
            return entityType.FullName;
        }

        public static Dictionary<string, EntityMeta> GetEntities()
        {
            if (_entities == null)
            {
                lock (_entitiesLock)
                {
                    if (_entities == null)
                    {
                        _entities = new Dictionary<string, EntityMeta>();

                        foreach (var em in CommonModel.Entities)
                        {
                            _entities.Add(GetClientName(em.EntityType), em);
                            //throw new InvalidOperationException(string.Format("存在生成两个同名客户端类型的类型：{0}、{1}。", em.EntityType.FullName, t.FullName));
                        }
                    }
                }
            }

            return _entities;
        }
    }
}