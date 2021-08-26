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
using Rafy.Utils;
using Rafy.MetaModel.View;
using Rafy.Web.Json;
using System.Diagnostics;
using Rafy.Web.ClientMetaModel;
using Rafy.Reflection;

namespace Rafy.Web.EntityDataPortal
{
    /// <summary>
    /// Entity 与 Json 相互转换的类
    /// </summary>
    internal class EntityJsonConverter
    {
        /// <summary>
        /// 把实体列表对应的 json 转换为 EntityList 对象。
        /// </summary>
        /// <param name="jEntityList"></param>
        /// <param name="repository">如果没有显式指定 Repository，则会根据 json 中的 _model 属性来查找对应的实体仓库。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        internal static EntityList JsonToEntityList(JObject jEntityList, EntityRepository repository = null)
        {
            return ListReader.JsonToEntity(jEntityList, repository);
        }

        internal static void EntityToJson(EntityViewMeta evm, IEnumerable<Entity> entities, IList<EntityJson> list)
        {
            foreach (var entity in entities)
            {
                var entityJson = new EntityJson();

                EntityToJson(evm, entity, entityJson);

                list.Add(entityJson);
            }
        }

        internal static void EntityToJson(EntityViewMeta evm, Entity entity, EntityJson entityJson)
        {
            var isTree = evm.EntityMeta.IsTreeEntity;

            foreach (var propertyVM in evm.EntityProperties)
            {
                var property = propertyVM.PropertyMeta;
                var mp = property.ManagedProperty;
                if (mp != null)
                {
                    //如果非树型实体，则需要排除树型实体的两个属性。
                    if (!isTree && (mp == Entity.TreeIndexProperty || mp == Entity.TreePIdProperty)) { continue; }

                    //引用属性
                    if (propertyVM.IsReferenceEntity)
                    {
                        var refMp = mp as IRefProperty;
                        object value = string.Empty;
                        var id = entity.GetRefNullableId(refMp.RefIdProperty);
                        if (id != null) { value = id; }

                        var idName = refMp.RefIdProperty.Name;
                        entityJson.SetProperty(idName, value);

                        //同时写入引用属性的视图属性，如 BookCategoryId_Display
                        if (id != null && propertyVM.CanShowIn(ShowInWhere.List))
                        {
                            var titleProperty = propertyVM.SelectionViewMeta.RefTypeDefaultView.TitleProperty;
                            if (titleProperty != null)
                            {
                                var lazyRefEntity = entity.GetRefEntity(refMp.RefEntityProperty);
                                var titleMp = titleProperty.PropertyMeta.ManagedProperty;
                                if (titleMp != null)
                                {
                                    value = lazyRefEntity.GetProperty(titleMp);
                                }
                                else
                                {
                                    value = ObjectHelper.GetPropertyValue(lazyRefEntity, titleProperty.Name);
                                }

                                var name = EntityModelGenerator.LabeledRefProperty(idName);
                                entityJson.SetProperty(name, value);
                            }
                        }
                    }
                    //一般托管属性
                    else
                    {
                        var pRuntimeType = property.PropertyType;
                        var serverType = ServerTypeHelper.GetServerType(pRuntimeType);
                        if (serverType.Name != SupportedServerType.Unknown)
                        {
                            var value = entity.GetProperty(mp);
                            value = ToClientValue(pRuntimeType, value);
                            entityJson.SetProperty(mp.Name, value);
                        }
                    }
                }
                //一般 CLR 属性
                else
                {
                    var pRuntimeType = property.PropertyType;
                    var serverType = ServerTypeHelper.GetServerType(pRuntimeType);
                    if (serverType.Name != SupportedServerType.Unknown)
                    {
                        var value = ObjectHelper.GetPropertyValue(entity, property.Name);
                        value = ToClientValue(pRuntimeType, value);
                        entityJson.SetProperty(property.Name, value);
                    }
                }
            }
        }

        internal static object ToClientValue(Type serverType, object value)
        {
            if (serverType.IsEnum) { value = EnumViewModel.EnumToLabel(value as Enum); }
            else if (serverType == typeof(Nullable<int>))
            {
                if (value == null) value = 0;
            }
            else if (serverType == typeof(Nullable<Guid>))
            {
                if (value == null) value = Guid.Empty;
                value = value.ToString();
            }
            else if (serverType == typeof(Guid))
            {
                value = value.ToString();
            }

            return value;
        }

        internal static object ToServerValue(Type serverType, object value)
        {
            if (serverType.IsEnum) { value = EnumViewModel.Parse(value as string, serverType); }
            else if (serverType == typeof(Nullable<int>))
            {
                var intValue = Convert.ToInt32(value);
                if (intValue == 0) value = null;
                else if (!(value is int)) value = intValue;
            }
            else if (serverType == typeof(Nullable<Guid>))
            {
                if (value != null)
                {
                    var guidValue = Guid.Parse(value.ToString());
                    if (guidValue == Guid.Empty) value = null;
                }
            }
            else if (serverType == typeof(Guid))
            {
                if (value != null)
                {
                    value = Guid.Parse(value.ToString());
                }
                else
                {
                    value = Guid.Empty;
                }
            }

            return value;
        }
    }
}