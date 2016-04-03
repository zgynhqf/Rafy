/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using Rafy.Domain.Caching;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Domain.ORM;
using Rafy.Utils;

namespace Rafy.Domain
{
    public partial class Entity
    {
        /// <summary>
        /// 从 EntityRepository 中加载完成，并从中返回时，都会执行此方法。
        /// </summary>
        internal void NotifyLoaded(IRepository repository)
        {
            _repository = repository;

            if (_status != PersistenceStatus.New)
            {
                this.PersistenceStatus = PersistenceStatus.Unchanged;
            }
        }

        #region 冗余属性处理

        /// <summary>
        /// 是否在更新本行数据时，同时更新所有依赖它的冗余属性。
        /// </summary>
        internal bool UpdateRedundancies
        {
            get { return this.GetFlags(EntitySerializableFlags.UpdateRedundancies); }
            set { this.SetFlags(EntitySerializableFlags.UpdateRedundancies, value); }
        }

        /// <summary>
        /// 在属性变更时，如果该属性在某个冗余路径中，则应该使用冗余更新策略。
        /// </summary>
        /// <param name="property">变更的属性.</param>
        private void NotifyIfInRedundancyPath(IProperty property)
        {
            if (property.IsInRedundantPath)
            {
                var refProperty = property as IRefIdProperty;
                if (refProperty != null)
                {
                    foreach (var path in property.InRedundantPathes)
                    {
                        //如果该引用属性是首位引用属性，并且冗余属性就是声明在这个对象上的，则直接计算冗余值，更新对象的值。
                        if (path.RefPathes[0].Property == refProperty)
                        {
                            //在继承实体的情况下，引用属性声明在父类，而冗余属性声明在子类B中时，子类A中则有引用属性而无冗余属性。
                            if (path.Redundancy.Owner.IsInstanceOfType(this))
                            {
                                //如果是第一个，说明冗余属性和这个引用属性是在当前类型中声明的，
                                //此时，直接更新冗余属性的值。
                                object value = this.GetRedundancyValue(path);
                                this.SetProperty(path.Redundancy.Property, value);
                            }
                            else
                            {
                                //并不是本实体类的引用属性变更，不需要更新冗余属性。
                                //do nothing
                            }
                        }
                        else
                        {
                            //延迟到更新数据库行时，才更新其它表的冗余属性
                            this.UpdateRedundancies = true;
                        }
                    }
                }
                else
                {
                    this.UpdateRedundancies = true;
                }
            }
        }

        /// <summary>
        /// 根据冗余路径从当前对象开始搜索，获取真实的属性值。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="from">
        /// 本对象在路径中拥有的引用属性。
        /// 在 D->C->B->A.Name 场景中，当前对象（this）可能是 C，那么 from 就是 C.BRefProperty.
        /// 如果没有指定此属性，则表示从第一个开始。
        /// </param>
        /// <returns></returns>
        internal object GetRedundancyValue(RedundantPath path, IRefIdProperty from = null)
        {
            Entity refEntity = this;
            foreach (var refP in path.RefPathes)
            {
                if (from != null && refP.Property != from) continue;

                refEntity = refEntity.GetRefEntity((refP.Property as IRefProperty).RefEntityProperty);
                if (refEntity == null) break;
            }

            object value = null;
            if (refEntity != this && refEntity != null) value = refEntity.GetProperty(path.ValueProperty.Property);

            return value;
        }

        #endregion
    }
}
