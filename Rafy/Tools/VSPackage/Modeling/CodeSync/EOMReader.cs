/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130409 14:31
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130409 14:31
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Rafy.DomainModeling;
using Rafy.EntityObjectModel;
using RafySDK;

namespace Rafy.VSPackage.Modeling.CodeSync
{
    /// <summary>
    /// 从代码到实体对象模型的读取器。
    /// </summary>
    class EOMReader : CodeElementVisitor
    {
        private EOMGroup _result = new EOMGroup();

        private EntityTypeCollection _entityTypes;
        private EnumTypeCollection _enumTypes;

        /// <summary>
        /// 读取整个项目的实体类结构
        /// </summary>
        /// <param name="project"></param>
        public void ReadProject(Project project)
        {
            Reset(project);

            //对所有 CSharp 文件进行解析。
            foreach (var item in ProjectHelper.EnumerateCSharpFiles(project.ProjectItems))
            {
                this.Visit(item.FileCodeModel.CodeElements);
            }

            AddRelations();
        }

        /// <summary>
        /// 获取解析完成的结构。
        /// </summary>
        /// <returns></returns>
        public EOMGroup GetResult()
        {
            return _result;
        }

        private void Reset(Project project)
        {
            _entityTypes = _result.EntityTypes;
            _enumTypes = _result.EnumTypes;
            _entityTypes.Clear();
            _enumTypes.Clear();
            _unknownProperties.Clear();
        }

        #region 解析关系

        private List<ValueProperty> _unknownProperties = new List<ValueProperty>();

        private void AddRelations()
        {
            AddInheritanceRelation();

            AddReferenceRelation();

            AddChildRelation();

            AddEnumTypeRelation();
        }

        private void AddReferenceRelation()
        {
            foreach (var type in _entityTypes)
            {
                foreach (var reference in type.References)
                {
                    var refTypeName = reference[RefEntityTypeFullNameProperty] as string;
                    if (refTypeName != null)
                    {
                        reference[RefEntityTypeFullNameProperty] = null;
                        var refType = _entityTypes.Find(refTypeName);
                        if (refType != null) { reference.RefEntityType = refType; }
                    }
                }
            }
        }

        private void AddChildRelation()
        {
            foreach (var type in _entityTypes)
            {
                foreach (var child in type.Children)
                {
                    var entityTypeName = Convention_EntityType(child.ListTypeFullName);
                    var refType = _entityTypes.Find(entityTypeName);
                    if (refType != null) { child.ChildEntityType = refType; }
                }
            }
        }

        private void AddInheritanceRelation()
        {
            foreach (var type in _entityTypes)
            {
                var baseTypeFullName = type[BaseTypeFullNameProperty] as string;
                if (baseTypeFullName != null)
                {
                    type[BaseTypeFullNameProperty] = null;
                    var baseType = _entityTypes.Find(baseTypeFullName);
                    if (baseType != null) { type.BaseType = baseType; }
                }
            }
        }

        private void AddEnumTypeRelation()
        {
            foreach (var property in _unknownProperties.ToArray())
            {
                var enumTypeFullName = property[PropertyTypeFullNameProperty] as string;
                if (enumTypeFullName != null)
                {
                    property[PropertyTypeFullNameProperty] = null;
                    var enumType = _enumTypes.Find(enumTypeFullName);
                    if (enumType != null)
                    {
                        property.EnumType = enumType;
                        property.PropertyType = ValuePropertyType.Enum;

                        //从集合中移除，这样剩下的属性都是未处理的属性。
                        _unknownProperties.Remove(property);
                    }
                }
            }
        }

        private static string Convention_EntityType(string listTypeName)
        {
            var entityTypeName = listTypeName;

            if (listTypeName.EndsWith("List"))
            {
                entityTypeName = listTypeName.Substring(0, listTypeName.Length - 4);
            }

            return entityTypeName;
        }

        #endregion

        protected override void VisitClass(CodeClass codeClass)
        {
            if (Helper.IsEntity(codeClass))
            {
                ParseEntity(codeClass);
            }
            else
            {
                base.VisitClass(codeClass);
            }
        }

        protected override void VisitVariable(CodeVariable codeVariable)
        {
            if (_currentType != null)
            {
                //只读取静态字段。
                if (codeVariable.IsShared)
                {
                    ParseRafyProperty(codeVariable);
                }
            }
            else if (_currentEnum != null)
            {
                ParseEnumValue(codeVariable);
            }

            base.VisitVariable(codeVariable);
        }

        protected override void VisitProperty(CodeProperty codeProperty)
        {
            if (_currentType != null)
            {
                ParseValuePropertyDomainName(codeProperty);

                SetNullableReference(codeProperty);
            }

            base.VisitProperty(codeProperty);
        }

        #region 解析实体

        #region 常量

        private const string PropertyToken = "Property";
        private const string RefIdProperty = "RefIdProperty";
        private const string RefEntityProperty = "RefEntityProperty<";
        private const string ListProperty = "ListProperty<";
        private const string NullableTypeToken = "System.Nullable<";
        private const string ListAsAggregationToken = "HasManyType.Aggregation";

        private const string BaseTypeFullNameProperty = "BaseTypeFullNameProperty";
        private const string RefEntityTypeFullNameProperty = "RefEntityTypeFullNameProperty";

        #endregion

        private EntityType _currentType;

        private void ParseEntity(CodeClass codeClass)
        {
            _currentType = new EntityType();
            _currentType.Name = codeClass.Name;
            _currentType.FullName = codeClass.FullName;
            _currentType.CodeElement = codeClass;
            var baseType = Helper.GetBaseClass(codeClass);
            if (baseType != null) _currentType[BaseTypeFullNameProperty] = baseType.FullName;
            //使用 Attribute 来进行检测是否为聚合根。
            foreach (CodeAttribute attri in codeClass.Attributes)
            {
                if (attri.FullName == Consts.RootEntityAttributeClassFullName)
                {
                    _currentType.IsAggtRoot = true;
                    break;
                }
            }

            //获取模型的注释。
            TryParseDomainName(_currentType, codeClass.DocComment);

            base.VisitClass(codeClass);

            _entityTypes.Add(_currentType);
            _currentType = null;
        }

        private void ParseRafyProperty(CodeVariable codeVariable)
        {
            var name = codeVariable.Name;
            var typeFullName = codeVariable.Type.AsFullName;
            //名字与类型中都有 Property 字样的属性，才是托管属性。
            if (name.EndsWith(PropertyToken) && typeFullName.Contains(PropertyToken))
            {
                var propertyName = name.Substring(0, name.Length - PropertyToken.Length);//去除后缀 Property

                var handled = ParseReference(codeVariable, propertyName, typeFullName);
                if (handled) return;

                handled = ParseChild(codeVariable, propertyName, typeFullName);
                if (handled) return;

                ParseValueProperty(codeVariable, propertyName, typeFullName);
            }
        }

        private bool ParseChild(CodeVariable codeVariable, string propertyName, string typeFullName)
        {
            if (typeFullName.Contains(ListProperty))
            {
                var initExp = codeVariable.InitExpression as string;
                if (!initExp.Contains(ListAsAggregationToken))
                {
                    var child = new Child
                    {
                        CodeElement = codeVariable,
                        Name = propertyName,
                        ListTypeFullName = UnwrapGenericType(typeFullName)
                    };
                    _currentType.Children.Add(child);
                    return true;
                }
            }

            return false;
        }

        #region 解析引用属性

        private Reference _currentReference;

        private bool ParseReference(CodeVariable codeVariable, string propertyName, string typeFullName)
        {
            if (typeFullName.Contains(RefIdProperty))
            {
                _currentReference = new Reference
                {
                    CodeElement = codeVariable,
                    IdProperty = propertyName,
                };

                var initExp = codeVariable.InitExpression.ToString();
                if (initExp.Contains("ReferenceType.Parent"))
                {
                    _currentReference.ReferenceType = ReferenceType.Parent;
                }
                else if (initExp.Contains("ReferenceType.Child"))
                {
                    _currentReference.ReferenceType = ReferenceType.Child;
                }

                //Nullable 放到 VisitProperty 中处理。
                //...

                _currentType.References.Add(_currentReference);
                return true;
            }

            if (typeFullName.Contains(RefEntityProperty))
            {
                var refEntityTypeFullName = UnwrapGenericType(typeFullName);
                _currentReference[RefEntityTypeFullNameProperty] = refEntityTypeFullName;
                _currentReference.EntityProperty = propertyName;
                _currentReference = null;
                return true;
            }

            return false;
        }

        private void SetNullableReference(CodeProperty codeProperty)
        {
            if (_currentReference != null)
            {
                if (codeProperty.Type.AsFullName.Contains(NullableTypeToken))
                {
                    _currentReference.Nullable = true;
                }
            }
        }

        #endregion

        private void ParseValueProperty(CodeVariable codeVariable, string propertyName, string typeFullName)
        {
            //如果匹配以下格式，说明此属性是一个一般属性。
            //例如：Rafy.Domain.Property<System.Nullable<System.Double>>
            var match = Regex.Match(typeFullName, @"Property<(?<name>.+)>");
            var propertyType = match.Groups["name"].Value;

            var property = new ValueProperty();

            property.CodeElement = codeVariable;
            property.Name = propertyName;

            //System.Nullable<System.Double>
            if (propertyType.Contains(NullableTypeToken))
            {
                property.Nullable = true;
                propertyType = UnwrapGenericType(propertyType);//去除 System.Nullable< 及 >
            }

            switch (propertyType)
            {
                case "System.String":
                    property.PropertyType = ValuePropertyType.String;
                    break;
                case "System.Boolean":
                    property.PropertyType = ValuePropertyType.Boolean;
                    break;
                case "System.Int32":
                    property.PropertyType = ValuePropertyType.Int;
                    break;
                case "System.Double":
                    property.PropertyType = ValuePropertyType.Double;
                    break;
                case "System.Decimal":
                    property.PropertyType = ValuePropertyType.Decimal;
                    break;
                case "System.DateTime":
                    property.PropertyType = ValuePropertyType.DateTime;
                    break;
                case "System.Byte[]":
                    property.PropertyType = ValuePropertyType.Bytes;
                    break;
                default:
                    property.PropertyType = ValuePropertyType.Unknown;
                    property[PropertyTypeFullNameProperty] = propertyType;
                    _unknownProperties.Add(property);
                    break;
            }

            _currentType.ValueProperties.Add(property);
        }

        private void ParseValuePropertyDomainName(CodeProperty codeProperty)
        {
            if (!string.IsNullOrWhiteSpace(codeProperty.DocComment))
            {
                var valueProperty = _currentType.ValueProperties.FirstOrDefault(vp => vp.Name == codeProperty.Name);
                if (valueProperty != null)
                {
                    TryParseDomainName(valueProperty, codeProperty.DocComment);
                }
            }
        }

        #endregion

        #region 解析枚举

        private const string PropertyTypeFullNameProperty = "EnumTypeFullNameProperty";

        private EnumType _currentEnum;

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            _currentEnum = new EnumType();
            _currentEnum.CodeElement = codeEnum;
            _currentEnum.Name = codeEnum.Name;
            _currentEnum.TypeFullName = codeEnum.FullName;

            base.VisitEnum(codeEnum);

            _enumTypes.Add(_currentEnum);
            _currentEnum = null;
        }

        private void ParseEnumValue(CodeVariable codeVariable)
        {
            _currentEnum.Items.Add(new EnumItem
            {
                CodeElement = codeVariable,
                Name = codeVariable.Name,
                //Value = codeVariable
            });
        }

        #endregion

        /// <summary>
        /// 获取泛型类型声明中的一般类型。
        /// 
        /// <![CDATA[
        /// 如果是传入 Property<Nullable<int>>，则返回 Nullable<int>。
        /// ]]>
        /// </summary>
        /// <param name="genericType"></param>
        /// <returns></returns>
        private static string UnwrapGenericType(string genericType)
        {
            genericType = genericType.Trim();

            var res = genericType;

            var index = genericType.IndexOf('<');
            if (index >= 0)
            {
                res = genericType.Substring(index + 1, genericType.Length - index - 2);
            }

            return res;
        }

        /// <summary>
        /// 获取模型的注释。
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private static void TryParseDomainName(EOMObject type, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                try
                {
                    var xmlDoc = XDocument.Parse(comment);
                    var summary = xmlDoc.Root.Element("summary");
                    if (summary != null)
                    {
                        var value = summary.Value.Trim();

                        //只获取非空的第一行。
                        var reader = new StringReader(value);
                        while (true)
                        {
                            value = reader.ReadLine();
                            if (value == null) { break; }

                            value = value.Trim();
                            if (value.Length > 1) { break; }
                        }
                        if (value != null)
                        {
                            ODMLDocumentHelper.SetDomainName(type, value);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("无法解析以下文本：" + comment, "解析注释内容时出错");
                }
            }
        }
    }
}