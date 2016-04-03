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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 中所有实体的属性标记都使用这个类或者这个类的子类
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class Property<TPropertyType> : ManagedProperty<TPropertyType>, IPropertyInternal, IProperty, IManagedProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Property{TPropertyType}"/> class.
        /// </summary>
        /// <param name="ownerType">Type of the owner.</param>
        /// <param name="declareType">Type of the declare.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultMeta">The default meta.</param>
        public Property(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property{TPropertyType}"/> class.
        /// </summary>
        /// <param name="ownerType">Type of the owner.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultMeta">The default meta.</param>
        public Property(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        /// <summary>
        /// Rafy 属性的类型
        /// </summary>
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
        /// 本托管属性是否是一个冗余属性。
        /// </summary>
        public bool IsRedundant
        {
            get { return this._redundantPath != null; }
        }

        /// <summary>
        /// 如果本托管属性是一个冗余属性，则这里返回它对应的冗余路径。
        /// </summary>
        public RedundantPath RedundantPath
        {
            get { return this._redundantPath; }
        }

        /// <summary>
        /// 声明本属性为只读属性
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.InvalidOperationException">
        /// 属性已经注册完毕，不能修改！
        /// or
        /// 冗余属性不能被其它冗余属性再次冗余，请直接写全冗余路径。
        /// </exception>
        internal void AsRedundantOf(RedundantPath path)
        {
            if (this.GlobalIndex >= 0) throw new InvalidOperationException("属性已经注册完毕，不能修改！");
            if ((path.ValueProperty.Property as IProperty).IsRedundant) throw new InvalidOperationException("冗余属性不能被其它冗余属性再次冗余，请直接写全冗余路径。");
            if (!path.RefPathes[0].Owner.IsAssignableFrom(this.OwnerType))
            {
                throw new InvalidOperationException(path.RefPathes[0].FullName + "作为冗余路径中的第一个引用属性，必须和冗余属性同在一个实体类型中。");
            }

            var list = (path.ValueProperty.Property as IPropertyInternal).InRedundantPathes;
            list.Add(path);

            foreach (var refProperty in path.RefPathes)
            {
                (refProperty.Property as IPropertyInternal).InRedundantPathes.Add(path);
            }

            this._redundantPath = path;
            path.Redundancy = new ConcreteProperty(this);
        }

        bool IProperty.IsInRedundantPath
        {
            get { return this._inRedundantPathes.Count > 0; }
        }

        List<RedundantPath> IPropertyInternal.InRedundantPathes
        {
            get { return this._inRedundantPathes; }
        }

        IEnumerable<RedundantPath> IProperty.InRedundantPathes
        {
            get { return this._inRedundantPathes; }
        }

        #endregion

        #region 操作符重载

        //public static PropertyComparisonExpression operator ==(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.Equal, value);
        //}

        //public static PropertyComparisonExpression operator !=(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.NotEqual, value);
        //}

        //public static PropertyComparisonExpression operator <(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.Less, value);
        //}

        //public static PropertyComparisonExpression operator <=(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.LessEqual, value);
        //}

        //public static PropertyComparisonExpression operator >(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.Greater, value);
        //}

        //public static PropertyComparisonExpression operator >=(Property<TPropertyType> left, object value)
        //{
        //    return new PropertyComparisonExpression(left, PropertyCompareOperator.GreaterEqual, value);
        //}

        //public PropertyComparisonExpression Like(string value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.Like, value);
        //}

        //public PropertyComparisonExpression Contains(string value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.Contains, value);
        //}

        //public PropertyComparisonExpression StartWith(string value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.StartWith, value);
        //}

        //public PropertyComparisonExpression EndWith(string value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.EndWith, value);
        //}

        //public PropertyComparisonExpression In(IEnumerable value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.In, value);
        //}

        //public PropertyComparisonExpression NotIn(IEnumerable value)
        //{
        //    return new PropertyComparisonExpression(this, PropertyCompareOperator.NotIn, value);
        //}

        ////消除警告
        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        ////消除警告
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        #endregion
    }

    internal interface IPropertyInternal
    {
        /// <summary>
        /// 其它类声明的本依赖属性的冗余属性路径
        /// </summary>
        List<RedundantPath> InRedundantPathes { get; }
    }
}