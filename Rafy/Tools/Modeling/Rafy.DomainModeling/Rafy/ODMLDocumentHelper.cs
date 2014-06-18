/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130410 10:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130410 10:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.EntityObjectModel;
using Rafy.DomainModeling.Models;

namespace Rafy.DomainModeling
{
    /// <summary>
    /// 一个用于帮助上层程序修改 odml 文档对象的类型。
    /// </summary>
    public class ODMLDocumentHelper
    {
        #region EntityType.DomainName

        private const string DomainNameProperty = "EntityType_DomainNameProperty";

        public static string GetDomainName(EntityType type)
        {
            return type[DomainNameProperty] as string;
        }

        public static void SetDomainName(EntityType type, string value)
        {
            type[DomainNameProperty] = value;
        }

        #endregion

        /// <summary>
        /// 添加指定元素到文档中。
        /// </summary>
        /// <param name="args"></param>
        public static void AddToDocument(AddToDocumentArgs args)
        {
            AddTypeElements(args.Docment, args.TypeList);

            TryAddConnectionElements(args);
        }

        private static void AddTypeElements(ODMLDocument doc, IList<EntityType> typeList)
        {
            int row = 0, column = 0;
            for (int i = 0, c = typeList.Count; i < c; i++)
            {
                var type = typeList[i];

                var typeFullName = type.FullName;
                //如果这个实体还没有加进来，就创建一个元素并加入到列表中。
                if (doc.EntityTypes.All(e => e.FullName != typeFullName))
                {
                    var entityTypeEl = new EntityTypeElement(typeFullName);
                    entityTypeEl.FullName = typeFullName;
                    entityTypeEl.Left = column * 300;
                    entityTypeEl.Top = row * 200;
                    entityTypeEl.IsAggtRoot = type.IsAggtRoot;
                    entityTypeEl.HideProperties = true;
                    entityTypeEl.Label = GetDomainName(type);

                    foreach (var vp in type.ValueProperties)
                    {
                        var propertyEl = new PropertyElement(vp.Name);

                        if (vp.PropertyType == ValuePropertyType.Enum)
                        {
                            propertyEl.PropertyType = vp.EnumType.Name;
                        }
                        else
                        {
                            propertyEl.PropertyType = vp.PropertyType.ToString();
                        }

                        //可空类型，显示在属性上时，需要添加"?" 号。
                        if (vp.Nullable)
                        {
                            propertyEl.PropertyType += "?";
                        }

                        entityTypeEl.Properties.Add(propertyEl);
                    }

                    doc.EntityTypes.Add(entityTypeEl);

                    column++;
                    //一排四个。
                    if (column == 3)
                    {
                        row++;
                        column = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 对于文档中刚加入的元素，还没有加入它对应的连接。
        /// 此方法会添加这些连接。
        /// </summary>
        /// <param name="document"></param>
        private static void TryAddConnectionElements(AddToDocumentArgs args)
        {
            var document = args.Docment;

            var docTypeList = document.EntityTypes
                .Select(et => args.AllTypes.First(e => e.FullName == et.FullName))
                .ToArray();

            foreach (var type in docTypeList)
            {
                //添加继承连接线
                var baseType = type.BaseType;
                if (docTypeList.Contains(baseType))
                {
                    var connectionEl = new ConnectionElement(type.FullName, baseType.FullName);
                    connectionEl.ConnectionType = ConnectionType.Inheritance;
                    TryAddConnection(document, connectionEl);
                }

                //添加引用连接线
                foreach (var reference in type.References)
                {
                    switch (reference.ReferenceType)
                    {
                        case ReferenceType.Normal:
                            //如果被引用的实体也在被选择的列表中，则尝试生成相应的连接。
                            if (docTypeList.Contains(reference.RefEntityType))
                            {
                                var connectionEl = new ConnectionElement(type.FullName, reference.RefEntityType.FullName);
                                connectionEl.ConnectionType = reference.Nullable ? ConnectionType.NullableReference : ConnectionType.Reference;
                                connectionEl.Label = reference.EntityProperty ?? reference.IdProperty;
                                TryAddConnection(document, connectionEl);
                            }
                            break;
                        //暂时忽略
                        //case ReferenceType.Parent:
                        //    break;
                        //case ReferenceType.Child:
                        //    break;
                        default:
                            break;
                    }
                }

                //添加组合连接线
                foreach (var child in type.Children)
                {
                    if (docTypeList.Contains(child.ChildEntityType))
                    {
                        var connectionEl = new ConnectionElement(child.ChildEntityType.FullName, type.FullName);
                        connectionEl.ConnectionType = ConnectionType.Composition;
                        connectionEl.Label = child.Name;
                        TryAddConnection(document, connectionEl);
                    }
                }
            }
        }

        private static void TryAddConnection(ODMLDocument document, ConnectionElement connectionEl)
        {
            //目前，暂时不支持自关联的线。
            if (connectionEl.From == connectionEl.To) return;

            //要判断是否之前已经添加了这个连接，才继续添加连接。
            if (document.Connections.All(c => c.From != connectionEl.From || c.To != connectionEl.To || c.Label != connectionEl.Label))
            {
                document.Connections.Add(connectionEl);
            }
        }
    }

    public class AddToDocumentArgs
    {
        /// <summary>
        /// 正在为这个文档添加元素。
        /// </summary>
        public ODMLDocument Docment { get; set; }

        /// <summary>
        /// 所有的实体类型列表。
        /// 
        /// 这个列表中必须包含 Document 目前已经拥有的实体，否则连接会添加失败。
        /// </summary>
        public IEnumerable<EntityType> AllTypes { get; set; }

        /// <summary>
        /// 需要添加的实体类型列表
        /// </summary>
        public IList<EntityType> TypeList { get; set; }
    }
}
