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
using OEA.ManagedProperty;
using Newtonsoft.Json.Linq;

namespace OEA.Web.EntityDataPortal
{
    /// <summary>
    /// 实体属性的设置器
    /// </summary>
    internal class EntityPropertySetter
    {
        private EntityRepository Repository;

        private IList<IManagedProperty> _allProperties;

        private IList<IRefProperty> _refProperties;

        public EntityPropertySetter(EntityRepository repo)
        {
            this.Repository = repo;

            this._allProperties = this.Repository.GetAvailableIndicators();
            this._refProperties = this._allProperties.Where(m => m is IRefProperty).Cast<IRefProperty>().ToArray();
        }

        internal void SetEntity(Entity e, JObject json)
        {
            foreach (var property in json.Properties())
            {
                var pName = property.Name;
                var value = property.Value;

                this.TrySetProperty(e, pName, value);
            }
        }

        private void TrySetProperty(Entity e, string pName, JToken value)
        {
            //有些小些的客户端数据被传输到了服务端，需要被过滤掉。
            if (char.IsLower(pName[0])) { return; }

            var mp = this._allProperties.FirstOrDefault(p => p.Name == pName);
            if (mp != null)
            {
                if (e.IsNew && mp == Entity.IdProperty) { return; }

                var isList = mp.PropertyType.IsSubclassOf(typeof(EntityList));
                if (isList)
                {
                    //todo: 此处的性能可能需要优化，聚合保存子列表时，重复的查询 Repository
                    var entityType = EntityConvention.EntityType(mp.PropertyType);
                    var repo = RF.Create(entityType);
                    var list = ListReader.JsonToEntity(value as JObject, repo);

                    e.SetProperty(mp, list, ManagedPropertyChangedSource.FromUIOperating);
                }
                else
                {
                    var rawValue = (value as JValue).Value;

                    rawValue = EntityJsonConverter.ToServerValue(mp.PropertyType, rawValue);

                    e.SetProperty(mp, rawValue, ManagedPropertyChangedSource.FromUIOperating);
                }
            }
            else
            {
                var rawValue = (value as JValue).Value;

                //如果没有找到一般的属性，则尝试查找外键属性
                var refP = this._refProperties.FirstOrDefault(r => r.GetMeta(e).IdProperty == pName);
                if (refP != null)
                {
                    e.GetLazyRef(refP).NullableId = Convert.ToInt32(rawValue);
                }
                else
                {
                    //只读属性。
                    //throw new InvalidOperationException("没有在实体中找到这个属性：" + pName);
                }
            }
        }
    }
}
