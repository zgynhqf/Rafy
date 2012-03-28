using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    public class MetaRepositoryBase<TMeta> : IEnumerable<TMeta>
        where TMeta : MetaBase
    {
        private List<Action> _relationSetters = new List<Action>(100);

        private List<TMeta> _allPrimes = new List<TMeta>();

        protected void AddPrime(TMeta meta)
        {
            lock (this._allPrimes) this._allPrimes.Add(meta);
        }

        /// <summary>
        /// 把某个行为添加到“待办”列表中，
        /// 等待所有的元数据都准备好后再执行。
        /// 
        /// 一般用于关系属性的设置
        /// </summary>
        /// <param name="a"></param>
        protected void FireAfterAllPrimesReady(Action a)
        {
            lock (this._relationSetters) this._relationSetters.Add(a);
        }

        /// <summary>
        /// 在完成添加初始的单个元数据之后，进行元数据间的关系设置。
        /// </summary>
        internal void InitRelations()
        {
            if (this._relationSetters == null) throw new InvalidOperationException("关系已经被初始化。");

            foreach (var relation in this._relationSetters) { relation(); }

            this._relationSetters = null;
        }

        /// <summary>
        /// 冻结所有的命令元数据
        /// </summary>
        internal void Freeze()
        {
            foreach (var v in this) v.Freeze();
        }

        internal List<TMeta> GetInnerList()
        {
            return this._allPrimes;
        }

        #region IEnumerable<TMeta>

        IEnumerator<TMeta> IEnumerable<TMeta>.GetEnumerator()
        {
            return this._allPrimes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._allPrimes.GetEnumerator();
        }

        #endregion
    }

    internal static class EntityMetaHelper
    {
        internal static IManagedProperty[] GetEntityPropertiesExtension(EntityMeta em)
        {
            //对于使用托管属性编写，而没有标记 EntityPropertyAttribute 的托管属性，也创建它对应的元数据。
            return em.ManagedProperties.GetCompiledProperties().Where(mp => mp.IsExtension).ToArray();
        }

        /// <summary>
        /// 通过反射、托管属性构建属性元数据列表
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        internal static List<PropertySource> GetEntityProperties(EntityMeta entityMeta)
        {
            //通过反射属性列表构建属性元数据列表
            var properties = entityMeta.EntityType.GetProperties();
            var list = properties
                .Where(property => property.HasMarked<EntityPropertyAttribute>())
                .ToList();

            //对于使用托管属性编写，而没有标记 EntityPropertyAttribute 的托管属性，也创建它对应的元数据。
            var managedProperties = entityMeta.ManagedProperties.GetCompiledProperties();
            foreach (var mp in managedProperties)
            {
                if (!mp.IsExtension)
                {
                    var name = mp.GetMetaPropertyName(entityMeta.EntityType);
                    var property = properties.FirstOrDefault(i => i.Name == name);
                    if (property != null
                        && !property.HasMarked<EntityPropertyAttribute>()
                        && !property.HasMarked<AssociationAttribute>())
                    {
                        list.Add(property);
                    }
                }
            }

            //由于托管属性中的顺序是按照名称排序的，所以要使用 CLR 属性重新排序。
            list.Sort((p1, p2) =>
            {
                var i1 = Array.IndexOf(properties, p1);
                var i2 = Array.IndexOf(properties, p2);
                return i1.CompareTo(i2);
            });

            var result = list.Select(p => new PropertySource
            {
                CLR = p,
                MP = managedProperties.FirstOrDefault(mp => mp.GetMetaPropertyName(entityMeta.EntityType) == p.Name)
            }).ToList();
            return result;
        }

        /// <summary>
        /// 通过反射、托管属性构建子属性元数据列表
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        internal static List<PropertySource> GetChildrenProperties(EntityMeta entityMeta)
        {
            var list = entityMeta.EntityType.GetProperties()
                .Where(property => property.HasMarked<AssociationAttribute>())
                .ToList();

            var managedProperties = entityMeta.ManagedProperties.GetCompiledProperties();
            var result = list.Select(p => new PropertySource
            {
                CLR = p,
                MP = managedProperties.FirstOrDefault(mp => mp.GetMetaPropertyName(entityMeta.EntityType) == p.Name)
            }).ToList();

            return result;
        }
    }
}