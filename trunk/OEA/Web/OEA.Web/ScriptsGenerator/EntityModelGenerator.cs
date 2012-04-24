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
using System.CodeDom.Compiler;
using System.IO;
using OEA.MetaModel;
using OEA.Web.ClientMetaModel;
using OEA.ManagedProperty;
using OEA.Web.EntityDataPortal;
using OEA.Library;

namespace OEA.Web
{
    /// <summary>
    /// 实体定义脚本的生成器
    /// </summary>
    internal class EntityModelGenerator
    {
        private EntityModel _model;

        internal EntityMeta EntityMeta { get; set; }

        internal EntityModel Generate()
        {
            this._model = new EntityModel();

            this.WriteFields();

            this.WriteReference();

            this.WriteChildren();

            this.WriteTreeAssociations();

            return this._model;
        }

        private void WriteFields()
        {
            var properties = this.EntityMeta.EntityProperties;
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var property = properties[i];
                var pName = property.Name;
                if (pName != DBConvention.FieldName_Id)
                {
                    var field = new EntityField
                    {
                        name = pName,
                        type = ServerTypeHelper.GetServerType(property.Runtime.PropertyType),
                        persist = property.Runtime.CanWrite,
                    };

                    var mp = property.ManagedProperty;
                    if (mp != null)
                    {
                        //为外键添加一个视图属性
                        if (mp is IRefProperty)
                        {
                            this._model.fields.Add(field);

                            var refMp = mp as IRefProperty;
                            field = new EntityField
                            {
                                name = LabeledRefProperty(pName),
                                type = ServerTypeHelper.GetServerType(typeof(string)),
                                persist = false,
                            };
                        }
                        else
                        {
                            var v = mp.GetMeta(this.EntityMeta.EntityType).DefaultValue;
                            field.defaultValue = EntityJsonConverter.ToClientValue(mp.PropertyType, v);
                        }
                    }

                    this._model.fields.Add(field);
                }
            }
        }

        private void WriteReference()
        {
            var properties = this.EntityMeta.EntityProperties;
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var property = properties[i];
                if (property.ReferenceInfo != null)
                {
                    var association = new BelongsToAssociation
                    {
                        associationKey = property.ReferenceInfo.RefEntityProperty,
                        foreignKey = property.Name,
                        model = ClientEntities.GetClientName(property.ReferenceInfo.RefType),
                    };

                    this._model.associations.Add(association);
                }
            }
        }

        private void WriteChildren()
        {
            var em = this.EntityMeta;
            var children = em.ChildrenProperties;
            for (int i = 0, c = children.Count; i < c; i++)
            {
                var child = children[i];

                var listProperty = child.ManagedProperty as IListProperty;
                var meta = listProperty.GetMeta(em.EntityType);
                if (meta.HasManyType == HasManyType.Composition)
                {
                    var pRef = child.ChildType.FindParentReferenceProperty();
                    if (pRef != null)
                    {
                        var childType = child.ChildType.EntityType;
                        var association = new HasManyAssociation
                        {
                            name = child.Name,
                            foreignKey = pRef.Name,
                            model = ClientEntities.GetClientName(childType),
                        };
                        this._model.associations.Add(association);
                    }
                }
            }
        }

        private void WriteTreeAssociations()
        {
            var supportTree = this.EntityMeta.IsTreeEntity;

            if (supportTree)
            {
                foreach (var property in this.EntityMeta.EntityProperties)
                {
                    if (property.Name == DBConvention.FieldName_PId
                        || property.Name == DBConvention.FieldName_TreePId)
                    {
                        this._model.associations.Add(new BelongsToAssociation
                        {
                            associationKey = "TreeParent",
                            foreignKey = property.Name,
                            model = ClientEntities.GetClientName(this.EntityMeta.EntityType),
                        });
                        this._model.associations.Add(new HasManyAssociation
                        {
                            name = "TreeChildren",
                            foreignKey = property.Name,
                            model = ClientEntities.GetClientName(this.EntityMeta.EntityType),
                        });

                        break;
                    }
                }
            }
        }

        internal static string LabeledRefProperty(string refProperty)
        {
            return refProperty + "_Label";
        }
    }
}
