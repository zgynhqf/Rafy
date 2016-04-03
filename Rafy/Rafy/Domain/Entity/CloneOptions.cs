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

using Rafy.ManagedProperty;
using System.Runtime;
using Rafy.Utils;

namespace Rafy.Domain
{
    /// <summary>
    /// 克隆方法选项
    /// </summary>
    public class CloneOptions
    {
        #region Actions

        /// <summary>
        /// 读取单个实体的数据
        /// CloneActions.NormalProperties | CloneActions.IdProperty | CloneActions.RefEntities | CloneActions.ParentRefEntity
        /// </summary>
        public static CloneOptions ReadSingleEntity()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.IdProperty | CloneActions.RefEntities | CloneActions.ParentRefEntity);
        }

        /// <summary>
        /// 读取数据行的数据
        /// CloneActions.NormalProperties | CloneActions.IdProperty
        /// </summary>
        public static CloneOptions ReadDbRow()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.IdProperty);
        }

        /// <summary>
        /// 拷贝新的实体的数据（不含 Id）。
        /// CloneActions.NormalProperties | CloneActions.RefEntities | CloneActions.ParentRefEntity
        /// </summary>
        public static CloneOptions NewSingleEntity()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.RefEntities | CloneActions.ParentRefEntity);
        }

        /// <summary>
        /// 组合克隆。
        /// 会克隆其所有的孩子对象。
        /// CloneActions.NormalProperties | CloneActions.RefEntities | CloneActions.ChildrenRecur
        /// </summary>
        public static CloneOptions NewComposition()
        {
            return new CloneOptions(CloneActions.NormalProperties | CloneActions.RefEntities | CloneActions.ChildrenRecur);
        }

        ///// <summary>
        ///// 从旧实体中合并值。
        ///// 
        ///// 复制完成后，旧实体不再可用。
        ///// 
        ///// CloneActions.NormalProperties | CloneActions.GrabChildren
        ///// </summary>
        //public static CloneOptions MergeOldEntity()
        //{
        //    return new CloneOptions(CloneActions.NormalProperties | CloneActions.GrabChildren);
        //}

        private CloneActions _actions;

        /// <summary>
        /// 所有要进行的复制操作。
        /// </summary>
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

        #endregion

        public CloneOptions(CloneActions cloneActions)
        {
            this._actions = cloneActions;
            this.Method = CloneValueMethod.SetProperty;
        }

        /// <summary>
        /// 值在复制时使用的方法。
        /// 默认值：<see cref="CloneValueMethod.SetProperty"/>。
        /// 
        /// <note type="note">使用设置属性的方式来拷贝值，这样可以使得冗余属性知道自己变更了。</note>
        /// </summary>
        public CloneValueMethod Method { get; set; }

        #region CloneMappings

        private CloneMappings _mappings;

        /// <summary>
        /// 如果设置了这个属性，则会在复制过程中记住所有的映射序列。
        /// </summary>
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
        /// 调用此方法，通知框架在复制属性值时，过滤掉一些无用的属性。
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
        /// 拷贝Id
        /// </summary>
        IdProperty = 1,
        /// <summary>
        /// 拷贝一般的属性
        /// </summary>
        NormalProperties = 2,
        /// <summary>
        /// 拷贝引用的对象
        /// </summary>
        RefEntities = 4,
        /// <summary>
        /// 拷贝引用的父对象
        /// </summary>
        ParentRefEntity = 8,
        /// <summary>
        /// 使用相同的拷贝方案，递归拷贝子对象及树的子节点。
        /// </summary>
        ChildrenRecur = 16,
        /// <summary>
        /// 抢夺对方的组合子。
        /// 组合子集合属性中的所有孩子对象的父指针也已经被更改为新实体。
        /// </summary>
        GrabChildren = 32
    }

    /// <summary>
    /// 值在复制时的方法
    /// </summary>
    public enum CloneValueMethod
    {
        /// <summary>
        /// 直接把值加载到对象中。
        /// </summary>
        LoadProperty,
        /// <summary>
        /// 使用较慢的设置值方法来拷贝值，但是发生对应的属性变更事件。
        /// </summary>
        SetProperty
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

        public T FindNewEntity<T>(object oldEntityId)
            where T : Entity
        {
            var list = this.MappingsForType(typeof(T));
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.OldEntity.Id.Equals(oldEntityId))
                {
                    return item.NewEntity as T;
                }
            }

            return null;
        }

        public T FindOldEntity<T>(object newEntityId)
            where T : Entity
        {
            var list = this.MappingsForType(typeof(T));
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.NewEntity.Id.Equals(newEntityId))
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