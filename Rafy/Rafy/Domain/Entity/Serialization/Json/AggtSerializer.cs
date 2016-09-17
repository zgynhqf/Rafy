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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.Domain.Serialization.Json
{
    /// <summary>
    /// 聚合实体的序列化器。
    /// 序列化后的数据只是暴露了实体的数据，而忽略了实体的状态。
    /// </summary>
    public class AggtSerializer
    {
        internal const string TreeChildrenProperty ="TreeChildren";
        private const string TotalCountProperty = "TotalCount";
        private const string EntityListProperty ="Data";

        private JsonTextWriter _writer;

        /// <summary>
        /// 内部的 JsonTextWriter。
        /// </summary>
        protected JsonTextWriter InnerWriter
        {
            get { return _writer; }
        }

        #region 序列化的配置属性

        /// <summary>
        /// 是否需要同时序列化所有子对象。
        /// 默认为 true。
        /// </summary>
        public bool SerializeAggt { get; set; } = true;

        /// <summary>
        /// 是否需要同时序列化相关的引用属性。
        /// 默认为 true。
        /// </summary>
        public bool SerializeReference { get; set; } = true;

        /// <summary>
        /// 如果使用了幽灵框架，那么此属性表示是否需要同时序列化幽灵属性。
        /// 默认为 false。
        /// </summary>
        public bool SerializeIsPhantom { get; set; }

        /// <summary>
        /// 是否使用舵峰式。
        /// 默认为 true。
        /// </summary>
        public bool UseCamelProperty { get; set; } = true;

        /// <summary>
        /// 是把在序列化枚举时，把值输出为字符串。
        /// 默认为 <see cref="EnumSerializationMode.Integer"/>。
        /// </summary>
        public EnumSerializationMode EnumSerializationMode { get; set; }

        /// <summary>
        /// 是否需要在序列化时忽略默认值的属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreDefault { get; set; }

        /// <summary>
        /// 是否需要在序列化时忽略只读属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreROProperties { get; set; }

        /// <summary>
        /// 是否输出实体列表的 TotalCount 的值，而把列表的值放到一个名为 Data 的属性值中。
        /// 但是，如果 TotalCount 中没有值时，则不会输出 TotalCount，而只输出 Data 属性。
        /// 默认为 false。
        /// </summary>
        public bool OutputListTotalCount { get; set; } = false;

        /// <summary>
        /// 是否忽略所有的动态属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreDynamicProperties { get; set; } = false;

        /// <summary>
        /// 是否采用缩进的格式。
        /// 默认为 false。
        /// </summary>
        public bool Indent { get; set; }

        #endregion

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

                this.Serialize(entityOrList, _writer);

                _writer.Flush();
            }
        }

        /// <summary>
        /// 序列化指定的实体元素到指定的 JsonTextWriter 中。
        /// </summary>
        /// <param name="entityOrList">The entity or list.</param>
        /// <param name="jsonWriter">The json writer.</param>
        /// <exception cref="System.ArgumentNullException">jsonWriter</exception>
        public void Serialize(IDomainComponent entityOrList, JsonTextWriter jsonWriter)
        {
            if (jsonWriter == null) throw new ArgumentNullException("jsonWriter");

            _writer = jsonWriter;

            if (entityOrList is Entity)
            {
                this.SerializeEntity(entityOrList as Entity);
            }
            else
            {
                this.SerializeOuterList(entityOrList as EntityList);
            }
        }

        /// <summary>
        /// 向 JSON 中序列化指定实体容。
        /// </summary>
        /// <param name="entity"></param>
        protected void SerializeEntity(Entity entity)
        {
            _writer.WriteStartObject();

            this.SerializeEntityContent(entity);

            _writer.WriteEndObject();
        }

        /// <summary>
        /// 向 JSON 中序列化指定实体的所有内容。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SerializeEntityContent(Entity entity)
        {
            //序列化所有的编译期属性。
            this.SerializeCompiledProperties(entity);

            if (!this.IgnoreDynamicProperties)
            {
                this.SerializeDynamicProperties(entity);
            }

            //如果是树实体，还需要输出树实体下的所有树子节点。
            if (entity.SupportTree)
            {
                var treeChildren = entity.TreeChildrenField;
                if (treeChildren != null)
                {
                    SerializeTreeChildren(treeChildren);
                }
            }
        }

        /// <summary>
        /// 序列化所有的编译期属性。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SerializeCompiledProperties(Entity entity)
        {
            var isTree = entity.SupportTree;

            foreach (var field in entity.GetCompiledPropertyValues())
            {
                var property = field.Property as IProperty;

                if (property.IsReadOnly && this.IgnoreROProperties) continue;
                if (!isTree && (property == Entity.TreePIdProperty || property == Entity.TreeIndexProperty)) { continue; }
                if (!this.SerializeIsPhantom && property == EntityConvention.Property_IsPhantom) continue;

                var value = field.Value;
                if (this.IgnoreDefault)
                {
                    var defaultValue = property.GetMeta(entity).DefaultValue;
                    if (object.Equals(defaultValue, value)) { continue; }
                }

                this.SerializeProperty(property, value);
            }
        }

        /// <summary>
        /// 序列化所有的编译期属性。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SerializeDynamicProperties(Entity entity)
        {
            if (entity.DynamicPropertiesCount > 0)
            {
                var properties = entity.GetDynamicProperties();
                foreach (var kv in properties)
                {
                    this.WritePropertyName(kv.Key);
                    this.SerializeValue(kv.Value);
                }
            }
        }

        /// <summary>
        /// 序列化实体的树子节点列表属性。
        /// </summary>
        /// <param name="treeChildren"></param>
        protected virtual void SerializeTreeChildren(Entity.EntityTreeChildren treeChildren)
        {
            //属性名
            this.WritePropertyName(TreeChildrenProperty);

            //属性值。
            _writer.WriteStartArray();
            for (int i = 0, c = treeChildren.Count; i < c; i++)
            {
                var treeChild = treeChildren[i];
                this.SerializeEntity(treeChild);
            }
            _writer.WriteEndArray();
        }

        /// <summary>
        /// 序列化某个指定的属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected virtual void SerializeProperty(IProperty property, object value)
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
                case PropertyCategory.ReferenceId:
                    this.WritePropertyName(property.Name);
                    this.SerializeValue(value);
                    break;
                //ReferenceId 也都全部直接输出。
                //case PropertyCategory.ReferenceId:
                //    var refProperty = property as IRefProperty;
                //    switch (refProperty.ReferenceType)
                //    {
                //        case ReferenceType.Child:
                //        case ReferenceType.Normal:
                //            this.WritePropertyName(property.Name);
                //            _writer.WriteValue(value);
                //            break;
                //        case ReferenceType.Parent:
                //            break;
                //        default:
                //            throw new NotSupportedException();
                //    }
                //    break;
                case PropertyCategory.ReferenceEntity:
                    if (this.SerializeReference && value != null && (property as IRefProperty).ReferenceType != ReferenceType.Parent)
                    {
                        this.WritePropertyName(property.Name);
                        this.SerializeEntity(value as Entity);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 序列化某个指定的属性的值。
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SerializeValue(object value)
        {
            //if (value is byte[])
            //{
            //    var base64 = Convert.ToBase64String(value as byte[]);
            //    _writer.WriteValue(base64);
            //}
            //else
            if (value is IList && !(value is byte[]))
            {
                _writer.WriteStartArray();
                var list = value as IList;
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var item = list[i];
                    _writer.WriteValue(item);
                }
                _writer.WriteEndArray();
            }
            else
            {
                if (value != null && value.GetType().IsEnum)
                {
                    switch (this.EnumSerializationMode)
                    {
                        case EnumSerializationMode.String:
                            value = value.ToString();
                            break;
                        case EnumSerializationMode.EnumLabel:
                            value = EnumViewModel.EnumToLabel((Enum)value) ?? value.ToString();
                            break;
                        default:
                            break;
                    }
                }
                _writer.WriteValue(value);
            }
        }

        private void SerializeList(EntityList entityList)
        {
            _writer.WriteStartArray();
            for (int i = 0, c = entityList.Count; i < c; i++)
            {
                var entity = entityList[i];
                this.SerializeEntity(entity);
            }
            _writer.WriteEndArray();
        }

        /// <summary>
        /// 序列化最外层的 EntityList。需要处理分页的信息。
        /// </summary>
        /// <param name="entityList"></param>
        private void SerializeOuterList(EntityList entityList)
        {
            if (this.OutputListTotalCount)
            {
                _writer.WriteStartObject();

                var tc = entityList.TotalCount;
                if (tc >= 0)
                {
                    this.WritePropertyName(TotalCountProperty);

                    _writer.WriteValue(entityList.TotalCount);
                }

                this.WritePropertyName(EntityListProperty);
            }

            this.SerializeList(entityList);

            if (this.OutputListTotalCount)
            {
                _writer.WriteEndObject();
            }
        }

        /// <summary>
        /// 向 JSON 中写入某个指定的属性。
        /// </summary>
        /// <param name="property"></param>
        protected virtual void WritePropertyName(string property)
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

    /// <summary>
    /// 枚举的输出模式。
    /// </summary>
    public enum EnumSerializationMode
    {
        /// <summary>
        /// 输出整形。
        /// </summary>
        Integer,
        /// <summary>
        /// 以字符串的形式输出。
        /// </summary>
        String,
        /// <summary>
        /// 在枚举上标记的 Label。
        /// 如果枚举没有标记 <see cref="Rafy.MetaModel.Attributes.LabelAttribute"/>，则直接输出名字。
        /// </summary>
        EnumLabel
    }
}