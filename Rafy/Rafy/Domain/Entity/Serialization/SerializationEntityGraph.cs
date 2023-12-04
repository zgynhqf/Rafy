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
using System.Runtime.Serialization.Json;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Serialization
{
    internal class SerializationEntityGraph
    {
        /// <summary>
        /// 为指定的实体创建一个 DataContractSerializer。
        /// 此过程会通过引用属性、列表属性，递归搜索实体类中所涉及到的其它所有实体类型，
        /// 并传递给 DataContractSerializer 作为已知类型，否则，将无法序列化。
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static XmlObjectSerializer CreateSerializer(EntityMeta entityMeta, bool json = true)
        {
            var graph = new SerializationEntityGraph();
            graph.DeepSearch(entityMeta);

            graph._knownTypes.Add(typeof(MPFV));

            if (json)
            {
                var jsonSerializer = new DataContractJsonSerializer(entityMeta.EntityType, graph._knownTypes);
                return jsonSerializer;
            }

            var serializer = new DataContractSerializer(entityMeta.EntityType, graph._knownTypes);
            return serializer;
        }

        private List<Type> _knownTypes;

        /// <summary>
        /// 通过引用属性、列表属性，递归搜索实体类中所涉及到的所有实体类型。
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        private void DeepSearch(EntityMeta entityMeta)
        {
            _knownTypes = new List<Type>() { entityMeta.EntityType };

            DeepSearchRecur(entityMeta);
        }

        private void DeepSearchRecur(EntityMeta entityMeta)
        {
            var properties = entityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var mp = properties[i] as IProperty;
                Type relativeType = null;
                switch (mp.Category)
                {
                    case PropertyCategory.ReferenceEntity:
                        if (mp.GetMeta(entityMeta.EntityType).Serializable)
                        {
                            relativeType = (mp as IRefProperty).RefEntityType;
                        }
                        break;
                    case PropertyCategory.List:
                        if (mp.GetMeta(entityMeta.EntityType).Serializable)
                        {
                            var p = mp as IListProperty;
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

                        var relativeTypeMeta = CommonModel.Entities.Find(relativeType);
                        DeepSearchRecur(relativeTypeMeta);
                    }
                }
            }
        }
    }
}