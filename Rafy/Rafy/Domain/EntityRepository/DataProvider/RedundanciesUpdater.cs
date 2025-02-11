﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 00:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 冗余属性更新器。
    /// </summary>
    public abstract class RedundanciesUpdater
    {
        /// <summary>
        /// 尝试更新依赖指定实体的值的冗余属性的值。
        /// </summary>
        /// <param name="entity">变更了值属性的实体。依赖该实体的所有冗余属性都需要被更新。</param>
        /// <param name="repository">实体对应的仓库。</param>
        public void UpdateRedundancies(Entity entity, IRepository repository)
        {
            if (repository == null) { repository = entity.GetRepository(); }
            var dbEntity = new Lazy<Entity>(() => ForceGetById(entity, repository));

            //如果有一些在冗余属性路径中的属性的值改变了，则开始更新数据库的中的所有冗余字段的值。
            var propertiesInPath = repository.GetPropertiesInRedundancyPath();
            for (int i = 0, c = propertiesInPath.Count; i < c; i++)
            {
                var property = propertiesInPath[i];

                //如果只有一个属性，那么就是它变更引起的更新
                //否则，需要从数据库获取原始值来对比检测具体哪些属性值变更，然后再发起冗余更新。
                bool isChanged = c == 1;

                var refProperty = RefPropertyHelper.Find(property);
                if (refProperty != null)
                {
                    if (!isChanged)
                    {
                        var dbKey = dbEntity.Value.GetProperty(refProperty.RefKeyProperty);
                        var newKey = entity.GetProperty(refProperty.RefKeyProperty);
                        isChanged = !object.Equals(dbKey, newKey);
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            //如果这条路径中是直接把引用属性的值作为值属性进行冗余，那么同样要进行值属性更新操作。
                            if (path.ValueProperty.Property == property)
                            {
                                this.UpdateRedundancyByRefValue(entity, path, refProperty, dbEntity);
                            }
                            //如果是引用变更了，并且只有一个 RefPath，则不需要处理。
                            //因为这个已经在属性刚变更时的处理函数中实时处理过了。
                            else if (path.RefPaths.Count > 1)
                            {
                                this.UpdateRedundancyByIntermidateRef(entity, path, refProperty, dbEntity);
                            }
                        }
                    }
                }
                else
                {
                    var newValue = entity.GetProperty(property);

                    if (!isChanged)
                    {
                        var dbValue = dbEntity.Value.GetProperty(property);
                        isChanged = !object.Equals(dbValue, newValue);
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            UpdateRedundancyByValue(entity, path, newValue, dbEntity);
                        }
                    }
                }
            }

            entity.UpdateRedundancies = false;
        }

        private Entity ForceGetById(Entity entity, IRepository repository)
        {
            using (RF.DisableEntityContext())
            {
                var dbEntity = repository.GetById(entity.Id);
                if (dbEntity == null)
                {
                    throw new InvalidOperationException(string.Format(@"{1} 类型对应的仓库中不存在 Id 为 {0} 的实体，更新冗余属性失败！", entity.Id, entity.GetType()));
                }
                return dbEntity;
            }
        }

        /// <summary>
        /// 值改变时引发的冗余值更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="dbEntity">The db entity.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void UpdateRedundancyByValue(Entity entity, ReferenceValuePath path, object newValue, Lazy<Entity> dbEntity)
        {
            this.UpdateRedundancy(entity, path.Redundancy, newValue, path.RefPaths, dbEntity);
        }

        /// <summary>
        /// 冗余路径中非首位的引用属性变化时引发的冗余值更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="refChanged">该引用属性值变化了</param>
        /// <param name="dbEntity">The db entity.</param>
        private void UpdateRedundancyByIntermidateRef(Entity entity, ReferenceValuePath path, IRefProperty refChanged, Lazy<Entity> dbEntity)
        {
            var newValue = entity.GetRedundancyValue(path, refChanged);

            //只要从开始到 refChanged 前一个
            var refPathes = new List<ConcreteProperty>(5);
            foreach (var refProperty in path.RefPaths)
            {
                if (refProperty.Property == refChanged.RefKeyProperty) break;
                refPathes.Add(refProperty);
            }

            this.UpdateRedundancy(entity, path.Redundancy, newValue, refPathes, dbEntity);
        }

        /// <summary>
        /// 冗余路径中非首位的引用属的值作为值属性进行冗余，那么同样要进行值属性更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="refChanged">该引用属性值变化了</param>
        /// <param name="dbEntity">The db entity.</param>
        private void UpdateRedundancyByRefValue(Entity entity, ReferenceValuePath path, IRefProperty refChanged, Lazy<Entity> dbEntity)
        {
            var newValue = entity.GetRefKey(refChanged);

            this.UpdateRedundancy(entity, path.Redundancy, newValue, path.RefPaths, dbEntity);
        }

        /// <summary>
        /// 更新某个冗余属性
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="redundancy">更新指定的冗余属性</param>
        /// <param name="newValue">冗余属性的新值</param>
        /// <param name="refPathes">从冗余属性声明类型开始的一个引用属性集合，
        /// 将会为这个集合路径生成更新的 Where 条件。</param>
        /// <param name="dbEntity">数据库中现有的值。</param>
        protected abstract void UpdateRedundancy(Entity entity, ConcreteProperty redundancy, object newValue, IList<ConcreteProperty> refPathes, Lazy<Entity> dbEntity);

        /// <summary>
        /// 完整刷新指定的冗余属性。
        /// </summary>
        /// <param name="redundancy">The redundancy.</param>
        public abstract void RefreshRedundancy(ConcreteProperty redundancy);
    }
}
