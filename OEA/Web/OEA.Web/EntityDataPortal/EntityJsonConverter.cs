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
using OEA.MetaModel;
using OEA.Utils;
using OEA.MetaModel.View;
using OEA.Web.Json;
using System.Diagnostics;

namespace OEA.Web.EntityDataPortal
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
            foreach (var propertyVM in evm.EntityProperties)
            {
                var property = propertyVM.PropertyMeta;
                var mp = property.ManagedProperty;
                if (mp != null)
                {
                    //引用属性
                    if (propertyVM.IsReference)
                    {
                        var refMp = mp as IRefProperty;
                        var lazyRef = entity.GetLazyRef(refMp);
                        object value = string.Empty;
                        var id = lazyRef.NullableId;
                        if (id.HasValue) { value = id.Value; }

                        var idName = refMp.GetMeta(entity).IdProperty;
                        entityJson.SetProperty(idName, value);

                        //同时写入引用属性的视图属性，如 BookCategoryId_Label
                        if (id.HasValue && propertyVM.CanShowIn(ShowInWhere.List))
                        {
                            var titleProperty = propertyVM.ReferenceViewInfo.RefTypeDefaultView.TitleProperty;
                            if (titleProperty != null)
                            {
                                var lazyRefEntity = lazyRef.Entity;
                                var titleMp = titleProperty.PropertyMeta.ManagedProperty;
                                if (titleMp != null)
                                {
                                    value = lazyRefEntity.GetProperty(titleMp);
                                }
                                else
                                {
                                    value = lazyRefEntity.GetPropertyValue(titleProperty.Name);
                                }

                                var name = EntityModelGenerator.LabeledRefProperty(idName);
                                entityJson.SetProperty(name, value);
                            }
                        }
                    }
                    //一般托管属性
                    else
                    {
                        var value = entity.GetProperty(mp);
                        value = ToClientValue(property.Runtime.PropertyType, value);
                        entityJson.SetProperty(mp.Name, value);
                    }
                }
                //一般属性
                else
                {
                    var value = entity.GetPropertyValue(property.Name);
                    value = ToClientValue(property.Runtime.PropertyType, value);
                    entityJson.SetProperty(property.Name, value);
                }
            }
        }

        internal static object ToClientValue(Type serverType, object value)
        {
            if (serverType.IsEnum) { value = EnumViewModel.EnumToLabel(value as Enum); }
            if (serverType == typeof(Nullable<int>))
            {
                if (value == null) value = 0;
            }

            return value;
        }

        internal static object ToServerValue(Type serverType, object value)
        {
            if (serverType.IsEnum) { value = EnumViewModel.LabelToEnum(value as string, serverType); }
            if (serverType == typeof(Nullable<int>))
            {
                var intValue = Convert.ToInt32(value);
                if (intValue == 0) value = null;
            }

            return value;
        }
    }
}