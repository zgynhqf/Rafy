/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// OEA 中所有实体的属性标记都使用这个类或者这个类的子类
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class Property<TPropertyType> : ManagedProperty<TPropertyType>, IPropertyInternal, IProperty, IManagedProperty
    {
        public Property(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        public Property(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        public virtual PropertyCategory Category
        {
            get
            {
                if (this.IsReadOnly) { return PropertyCategory.Readonly; }
                if (this.IsRedundant) { return PropertyCategory.Redundancy; }
                return PropertyCategory.Normal;
            }
        }

        #region RedundantDependencies

        /// <summary>
        /// 如果本托管属性是一个冗余属性，则这个字段表示它的依赖路径
        /// </summary>
        private RedundantPath _redundantPath;

        /// <summary>
        /// 其它类声明的本依赖属性的冗余属性
        /// </summary>
        private List<RedundantPath> _inRedundantPathes = new List<RedundantPath>();

        /// <summary>
        /// 声明本属性为只读属性
        /// </summary>
        /// <param name="readOnlyValueProvider"></param>
        /// <param name="dependencies"></param>
        internal void AsRedundantOf(RedundantPath path)
        {
            if (this.GlobalIndex >= 0) throw new InvalidOperationException("属性已经注册完毕，不能修改！");
            if (path.ValueProperty.IsRedundant) throw new InvalidOperationException("冗余属性不能被其它冗余属性再次冗余，请直接写全冗余路径。");

            var list = (path.ValueProperty as IPropertyInternal).InRedundantPathes;
            list.Add(path);

            foreach (IPropertyInternal refProperty in path.RefPathes)
            {
                refProperty.InRedundantPathes.Add(path);
            }

            this._redundantPath = path;
            path.Redundancy = this;
        }

        List<RedundantPath> IPropertyInternal.InRedundantPathes
        {
            get { return this._inRedundantPathes; }
        }

        public bool IsRedundant
        {
            get { return this._redundantPath != null; }
        }

        bool IProperty.IsInRedundantPath
        {
            get { return this._inRedundantPathes.Count > 0; }
        }

        IEnumerable<RedundantPath> IProperty.InRedundantPathes
        {
            get { return this._inRedundantPathes; }
        }

        #endregion
    }

    /// <summary>
    /// OEA 实体框架中的托管属性
    /// </summary>
    public interface IProperty : IManagedProperty
    {
        PropertyCategory Category { get; }

        /// <summary>
        /// 本托管属性是否是一个冗余属性
        /// </summary>
        bool IsRedundant { get; }

        /// <summary>
        /// 本托管属性是否在其它类上被声明了冗余属性的路径上
        /// </summary>
        bool IsInRedundantPath { get; }

        /// <summary>
        /// 其它类声明的本依赖属性的冗余属性路径
        /// </summary>
        IEnumerable<RedundantPath> InRedundantPathes { get; }
    }

    internal interface IPropertyInternal
    {
        /// <summary>
        /// 其它类声明的本依赖属性的冗余属性路径
        /// </summary>
        List<RedundantPath> InRedundantPathes { get; }
    }

    /// <summary>
    /// OEA 中可用的属性类型
    /// </summary>
    public enum PropertyCategory
    {
        /// <summary>
        /// 一般属性
        /// </summary>
        Normal,
        /// <summary>
        /// 引用属性
        /// </summary>
        Reference,
        /// <summary>
        /// 列表属性
        /// </summary>
        List,
        /// <summary>
        /// 只读属性
        /// </summary>
        Readonly,
        /// <summary>
        /// 冗余属性
        /// </summary>
        Redundancy
    }
}