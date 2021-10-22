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
using Rafy.ManagedProperty;
using Newtonsoft.Json.Linq;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Web.EntityDataPortal
{
    /// <summary>
    /// 实体属性的设置器
    /// </summary>
    internal class EntityPropertySetter
    {
        private EntityMeta _entityMeta;

        private IList<IRefIdProperty> _refIdProperties;

        public EntityPropertySetter(EntityMeta entityMeta)
        {
            _entityMeta = entityMeta;
            _refIdProperties = _entityMeta.ManagedProperties
                .GetAvailableProperties().OfType<IRefIdProperty>().ToArray();
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
            //有些小写的客户端数据被传输到了服务端，需要被过滤掉。
            if (char.IsLower(pName[0])) { return; }

            var pm = _entityMeta.Property(pName) as PropertyMeta ??
                _entityMeta.ChildrenProperty(pName);
            if (pm != null)
            {
                var mp = pm.ManagedProperty;
                if (mp is IListProperty)
                {
                    //todo: 此处的性能可能需要优化，聚合保存子列表时，重复的查询 Repository
                    var entityType = EntityMatrix.FindByList(pm.PropertyType).EntityType;
                    var repo = RF.Find(entityType);

                    //列表属性的设置不能使用 SetProperty，否则，list.Parent 将会无值。
                    //但是也不能直接使用 LoadProperty，否则会导致调用 list.MarkOld，从而不会保存这个列表。
                    //所以只能先装载一个空列表，然后再把 json 中的数据转换为实体加入到这个列表中。
                    var list = repo.NewList();
                    e.LoadProperty(mp, list);

                    ListReader.JsonToEntity(value as JObject, repo, list);
                }
                else
                {
                    var rawValue = (value as JValue).Value;

                    rawValue = EntityJsonConverter.ToServerValue(pm.PropertyType, rawValue);

                    e.SetProperty(mp, rawValue);
                }
            }
            else
            {
                var rawValue = (value as JValue).Value;

                //如果没有找到一般的属性，则尝试查找外键属性
                for (int i = 0, c = _refIdProperties.Count; i < c; i++)
                {
                    var rip = _refIdProperties[i];
                    if (rip.Name == pName)
                    {
                        e.SetRefId(rip, rawValue);
                        break;
                    }
                }
                //只读属性。
                //if(notFound)
                //{
                //    throw new InvalidOperationException("没有在实体中找到这个属性：" + pName);
                //}
            }
        }
    }
}
