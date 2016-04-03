/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140107
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140107 14:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain.Serialization
{
    internal class SerializationEntityGraph
    {
        /// <summary>
        /// 为指定的实体创建一个 DataContractSerializer。
        /// 此过程会通过引用属性、列表属性，递归搜索实体类中所涉及到的其它所有实体类型，
        /// 并传递给 DataContractSerializer 作为已知类型，否则，将无法序列化。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static DataContractSerializer CreateSerializer(Entity entity)
        {
            var graph = new SerializationEntityGraph();
            graph.DeepSearch(entity);

            return new DataContractSerializer(entity.GetType(), graph._knownTypes);
        }

        private List<Type> _knownTypes;

        /// <summary>
        /// 通过引用属性、列表属性，递归搜索实体类中所涉及到的所有实体类型。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private void DeepSearch(Entity entity)
        {
            _knownTypes = new List<Type>() { entity.GetType() };

            DeepSearchRecur(entity.GetRepository());
        }

        private void DeepSearchRecur(IRepository repo)
        {
            var properties = repo.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var mp = properties[i] as IProperty;
                Type relativeType = null;
                switch (mp.Category)
                {
                    case PropertyCategory.ReferenceEntity:
                        if (mp.GetMeta(repo.EntityType).Serializable)
                        {
                            relativeType = (mp as IRefEntityProperty).RefEntityType;
                        }
                        break;
                    case PropertyCategory.List:
                        if (mp.GetMeta(repo.EntityType).Serializable)
                        {
                            var  p = mp as IListProperty;
                            if (!_knownTypes.Contains(p.PropertyType))
                            {
                                //列表类型同样需要加入到序列化类型列表中。
                                _knownTypes.Add(p.PropertyType);

                                relativeType = p.ListEntityType;
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (relativeType != null)
                {
                    if (!_knownTypes.Contains(relativeType))
                    {
                        _knownTypes.Add(relativeType);

                        var repo2 = RepositoryFactoryHost.Factory.FindByEntity(relativeType);
                        DeepSearchRecur(repo2);
                    }
                }
            }
        }

        private void DeepSearchRecur_Instance(Entity entity)
        {
            var values = entity.GetNonDefaultPropertyValues();
            foreach (var value in values)
            {
                var mp = value.Property as IProperty;
                switch (mp.Category)
                {
                    case PropertyCategory.ReferenceEntity:
                        if (mp.GetMeta(entity).Serializable)
                        {
                            var refEntity = entity.GetRefEntity(mp as IRefEntityProperty);
                            AddEntityType(refEntity.GetType());
                            DeepSearchRecur_Instance(refEntity);
                        }
                        break;
                    case PropertyCategory.List:
                        if (mp.GetMeta(entity).Serializable)
                        {
                            var list = entity.GetLazyList(mp as IListProperty);
                            for (int i = 0, c = list.Count; i < c; i++)
                            {
                                var child = list[i];
                                if (i == 0) { AddEntityType(child.GetType()); }
                                DeepSearchRecur_Instance(child);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void AddEntityType(Type entityType)
        {
            if (!_knownTypes.Contains(entityType))
            {
                _knownTypes.Add(entityType);
            }
        }
    }
}