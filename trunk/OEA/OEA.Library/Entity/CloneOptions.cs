/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110309
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100309
 * 
*******************************************************/

using System;
using System.Collections.Generic;

using OEA.ManagedProperty;
using System.Runtime;
using OEA.Utils;

namespace OEA.Library
{
    /// <summary>
    /// 克隆方法选项
    /// </summary>
    public class CloneOptions
    {
        /// <summary>
        /// 读取数据行的数据
        /// </summary>
        public static CloneOptions ReadSingleEntity()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.IdProperty | CloneActions.RefEntities | CloneActions.ParentRefEntity);
        }

        /// <summary>
        /// 读取数据行的数据
        /// </summary>
        public static CloneOptions ReadDbRow()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.IdProperty);
        }

        /// <summary>
        /// 聚合克隆
        /// 会克隆其所有的孩子对象。
        /// </summary>
        public static CloneOptions NewAggregate()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.RefEntities | CloneActions.ChildrenRecur);
        }

        /// <summary>
        /// 从老对象中合并值。
        /// </summary>
        public static CloneOptions MergeOldEntity()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.GrabChildren);
        }

        private CloneActions _actions;

        public CloneActions Actions
        {
            get
            {
                return this._actions;
            }
            set
            {
                this._actions = value;
            }
        }

        public CloneOptions(CloneActions cloneActions)
        {
            this._actions = cloneActions;
        }

        #region CloneMappings

        private CloneMappings _mappings;

        public CloneMappings Mappings
        {
            get { return _mappings; }
            set { _mappings = value; }
        }

        /// <summary>
        /// 某一个 Entity 拷贝完成后，会调用此方法。
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        internal void NotifyCloned(Entity oldEntity, Entity newEntity)
        {
            if (this._mappings != null)
            {
                this._mappings.SetMapping(oldEntity, newEntity);
            }
        }

        #endregion

        #region IgnoreList

        private List<IManagedProperty> _ignoreList;

        /// <summary>
        /// 调用此方法，通知框架在复制CSLA值时，过滤掉一些无用的属性。
        /// </summary>
        /// <param name="property"></param>
        public void IgnoreProperty(IManagedProperty property)
        {
            if (this._ignoreList == null) this._ignoreList = new List<IManagedProperty>();

            this._ignoreList.Add(property);
        }

        /// <summary>
        /// 获取一次性的 IgoreList
        /// </summary>
        /// <returns></returns>
        internal IList<IManagedProperty> RetrieveIgnoreListOnce()
        {
            var value = this._ignoreList;

            this._ignoreList = null;

            return value;
        }

        #endregion

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public bool HasAction(CloneActions action)
        {
            return (this._actions & action) == action;
        }
    }

    /// <summary>
    /// 克隆操作的参数。
    /// </summary>
    [Flags]
    public enum CloneActions
    {
        /// <summary>
        /// 拷贝一般的属性
        /// </summary>
        NormalProperties = 1,
        /// <summary>
        /// 拷贝Id
        /// </summary>
        IdProperty = 2,
        /// <summary>
        /// 拷贝引用的对象
        /// </summary>
        RefEntities = 4,
        /// <summary>
        /// 拷贝引用的父对象
        /// </summary>
        ParentRefEntity = 8,
        /// <summary>
        /// 抢夺对方的孩子
        /// </summary>
        GrabChildren = 16,
        /// <summary>
        /// 使用相同的拷贝方案，递归拷贝子对象
        /// </summary>
        ChildrenRecur = 32
    }

    /// <summary>
    /// 此类职责：记录所有被复制的对象和原有对象间的映射关系
    /// </summary>
    public class CloneMappings
    {
        private SortedDictionary<Type, List<Mapping>> _copiedMap =
            new SortedDictionary<Type, List<Mapping>>(TypeNameComparer.Instance);

        public T FindNewEntity<T>(T oldEntity)
            where T : Entity
        {
            var list = this.MappingsForType(oldEntity.GetType());
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.OldEntity == oldEntity)
                {
                    return item.NewEntity as T;
                }
            }

            return null;
        }

        public T FindOldEntity<T>(T newEntity)
            where T : Entity
        {
            var list = this.MappingsForType(newEntity.GetType());
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.NewEntity == newEntity)
                {
                    return item.OldEntity as T;
                }
            }

            return null;
        }

        public T FindNewEntity<T>(int oldEntityId)
            where T : Entity
        {
            var list = this.MappingsForType(typeof(T));
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.OldEntity.Id == oldEntityId)
                {
                    return item.NewEntity as T;
                }
            }

            return null;
        }

        public T FindOldEntity<T>(int newEntityId)
            where T : Entity
        {
            var list = this.MappingsForType(typeof(T));
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.NewEntity.Id == newEntityId)
                {
                    return item.OldEntity as T;
                }
            }

            return null;
        }

        internal void SetMapping(Entity oldEntity, Entity newEntity)
        {
            var entityType = oldEntity.GetType();
            var list = this.MappingsForType(entityType);
            list.Add(new Mapping() { OldEntity = oldEntity, NewEntity = newEntity });
        }

        private List<Mapping> MappingsForType(Type type)
        {
            List<Mapping> result = null;
            if (!this._copiedMap.TryGetValue(type, out result))
            {
                result = new List<Mapping>();
                this._copiedMap[type] = result;
            }

            return result;
        }

        private class Mapping
        {
            public Entity OldEntity;
            public Entity NewEntity;
        }
    }
}