/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 10:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.Domain.Serialization.Json
{
    /// <summary>
    /// 聚合实体的序列化器。
    /// 序列化后的数据只是暴露了实体的数据，而忽略了实体的状态。
    /// </summary>
    public class AggtSerializer
    {
        private const string TotalCountProperty = "TotalCount";
        private const string EntityListProperty ="Data";
        private JsonTextWriter _writer;

        public AggtSerializer()
        {
            this.SerializeAggt = true;
            this.UseCamelProperty = true;
        }

        /// <summary>
        /// 是否需要同时序列化所有子对象。
        /// 默认为 true。
        /// </summary>
        public bool SerializeAggt { get; set; }

        /// <summary>
        /// 是否使用舵峰式。
        /// 默认为 true。
        /// </summary>
        public bool UseCamelProperty { get; set; }

        /// <summary>
        /// 是否需要在序列化时忽略默认值的属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreDefault { get; set; }

        /// <summary>
        /// 是否采用缩进的格式。
        /// 默认为 false。
        /// </summary>
        public bool Indent { get; set; }

        /// <summary>
        /// 序列化指定的实体元素，并返回对应的 JSON。
        /// </summary>
        /// <param name="entityOrList"></param>
        /// <returns></returns>
        public string Serialize(IDomainComponent entityOrList)
        {
            using (var writer = new StringWriter())
            {
                this.Serialize(entityOrList, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// 序列化指定的实体元素到指定的 TextWriter 中。
        /// </summary>
        /// <param name="entityOrList"></param>
        /// <param name="textWriter"></param>
        public void Serialize(IDomainComponent entityOrList, TextWriter textWriter)
        {
            using (_writer = new JsonTextWriter(textWriter))
            {
                _writer.CloseOutput = false;
                if (this.Indent)
                {
                    _writer.Formatting = Formatting.Indented;
                }

                if (entityOrList is Entity)
                {
                    this.SerializeEntity(entityOrList as Entity);
                }
                else
                {
                    this.SerializeList(entityOrList as EntityList);
                }

                _writer.Flush();
            }
        }

        private void SerializeEntity(Entity entity)
        {
            var isTree = entity.SupportTree;

            _writer.WriteStartObject();

            foreach (var field in entity.GetCompiledPropertyValues())
            {
                var property = field.Property as IProperty;
                if (!isTree && (property == Entity.TreePIdProperty || property == Entity.TreeIndexProperty))
                {
                    continue;
                }
                if (this.IgnoreDefault)
                {
                    var defaultValue = property.GetMeta(entity).DefaultValue;
                    if (object.Equals(defaultValue, field.Value))
                    {
                        continue;
                    }
                }

                this.SerializeProperty(property, field.Value);
            }

            _writer.WriteEndObject();
        }

        private void SerializeProperty(IProperty property, object value)
        {
            switch (property.Category)
            {
                case PropertyCategory.List:
                    if (this.SerializeAggt && value != null)
                    {
                        this.WritePropertyName(property.Name);
                        this.SerializeList(value as EntityList);
                    }
                    break;
                case PropertyCategory.LOB:
                case PropertyCategory.Normal:
                case PropertyCategory.Readonly:
                case PropertyCategory.Redundancy:
                    this.WritePropertyName(property.Name);
                    _writer.WriteValue(value);
                    break;
                case PropertyCategory.ReferenceId:
                    var refProperty = property as IRefProperty;
                    switch (refProperty.ReferenceType)
                    {
                        case ReferenceType.Child:
                        case ReferenceType.Normal:
                            this.WritePropertyName(property.Name);
                            _writer.WriteValue(value);
                            break;
                        case ReferenceType.Parent:
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case PropertyCategory.ReferenceEntity:
                    //所有引用属性需要被忽略。
                    //var refProperty = property as IRefProperty;
                    //if (refProperty.ReferenceType == ReferenceType.Child)
                    //{
                    //    throw new NotSupportedException("不支持引用类型为聚合子的引用属性的序列化。");
                    //}
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void SerializeList(EntityList entityList)
        {
            var totalCount = entityList.TotalCount;
            if (totalCount > 0)
            {
                _writer.WriteStartObject();
                this.WritePropertyName(TotalCountProperty);
                _writer.WriteValue(totalCount);
                this.WritePropertyName(EntityListProperty);
            }

            _writer.WriteStartArray();

            foreach (var entity in entityList)
            {
                this.SerializeEntity(entity);
            }

            _writer.WriteEndArray();

            if (totalCount > 0)
            {
                _writer.WriteEndObject();
            }
        }

        private void WritePropertyName(string property)
        {
            if (this.UseCamelProperty)
            {
                property = this.ToCamel(property);
            }
            _writer.WritePropertyName(property);
        }

        private string ToCamel(string property)
        {
            if (char.IsLower(property[0]))
            {
                return property;
            }

            return char.ToLower(property[0]) + property.Substring(1);
        }
    }
}