/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 属性的视图模型
    /// </summary>
    public abstract class EntityPropertyViewMeta : ViewMeta
    {
        private EntityPropertyMeta _PropertyMeta;
        /// <summary>
        /// 对应的属性信息
        /// </summary>
        public EntityPropertyMeta PropertyMeta
        {
            get { return this._PropertyMeta; }
            set { this.SetValue(ref this._PropertyMeta, value); }
        }

        /// <summary>
        /// 属性名
        /// </summary>
        public override string Name
        {
            get { return this.PropertyMeta.Name; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 是否为“引用属性”
        /// </summary>
        public bool IsReference
        {
            get { return this.PropertyMeta.ReferenceInfo != null; }
        }

        private SelectionViewMeta _selectionViewMeta;
        /// <summary>
        /// 本属性需要选择实体的相关视图信息
        /// 
        /// 一般情况下，如果当前属性为引用实体属性，那么它默认带有这个值。其它的一般属性需要主动设置本值。
        /// </summary>
        public SelectionViewMeta SelectionViewMeta
        {
            get { return this._selectionViewMeta; }
            set
            {
                this._selectionViewMeta = value;

                if (value != null && this.IsReference)
                {
                    value.RefInfo = this.PropertyMeta.ReferenceInfo;
                }
            }
        }

        [UnAutoFreeze]
        private EntityViewMeta _Owner;
        /// <summary>
        /// 属性所在的实体类型的视图信息
        /// </summary>
        public EntityViewMeta Owner
        {
            get { return this._Owner; }
            set { this.SetValue(ref this._Owner, value); }
        }

        private PropertyVisibilityIndicator _VisibilityIndicator = new PropertyVisibilityIndicator();
        /// <summary>
        /// 用于检测是否可见的属性
        /// </summary>
        public PropertyVisibilityIndicator VisibilityIndicator
        {
            get { return this._VisibilityIndicator; }
            set { this.SetValue(ref this._VisibilityIndicator, value); }
        }

        private double _OrderNo;
        /// <summary>
        /// 排序此属性使用的属性。
        /// </summary>
        public double OrderNo
        {
            get { return this._OrderNo; }
            set { this.SetValue(ref this._OrderNo, value); }
        }

        private ShowInWhere _ShowInWhere;
        /// <summary>
        /// 在哪里显示
        /// </summary>
        public ShowInWhere ShowInWhere
        {
            get { return this._ShowInWhere; }
            set
            {
                if (this.IsChildReference()) throw new InvalidOperationException("聚合子属性，请设置他的 IsVisible 属性");

                this.SetValue(ref this._ShowInWhere, value);
            }
        }

        public override bool IsVisible
        {
            get
            {
                if (this.IsChildReference()) return base.IsVisible;

                return this.ShowInWhere != ShowInWhere.Hide;
            }
            set
            {
                if (this.IsChildReference())
                {
                    base.IsVisible = value;
                    return;
                }

                throw new InvalidOperationException("请设置他的 ShowInWhere 属性");
            }
        }

        private bool IsChildReference()
        {
            return this.IsReference && this.PropertyMeta.ReferenceInfo.Type == ReferenceType.Child;
        }

        #region 查询方法

        /// <summary>
        /// 判断是否可以显示在某处
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool CanShowIn(ShowInWhere where)
        {
            return (this.ShowInWhere & where) == where;
        }

        /// <summary>
        /// 把000010000300002排序为000000000000123
        /// 
        /// 不把0的位置改变，这样可以保证顺序与属性定义的顺序一致。
        /// </summary>
        internal static EntityPropertyViewMeta[] Order(IEnumerable<EntityPropertyViewMeta> properties)
        {
            //这个方法会打乱都是0的节点原有的顺序。
            //properties.Sort((a, b) => a.OrderNo.CompareTo(b.OrderNo));

            var res = new EntityPropertyViewMeta[properties.Count()];

            var lesszerolist = properties.Where(p => p.OrderNo < 0).OrderBy(p => p.OrderNo).ToArray();
            var zeroList = properties.Where(p => p.OrderNo == 0).ToArray();
            var greaterzerolist = properties.Where(p => p.OrderNo > 0).OrderBy(p => p.OrderNo).ToArray();

            int i = 0;
            foreach (var item in lesszerolist) res[i++] = item;
            foreach (var item in zeroList) res[i++] = item;
            foreach (var item in greaterzerolist) res[i++] = item;

            return res;
        }


        #endregion
    }

    /// <summary>
    /// 导航属性的元数据
    /// </summary>
    public class NavigationPropertyMeta : Freezable
    {
        private string _IdPropertyName;

        /// <summary>
        /// 如果本导航属性是一个子实体集合时，IdPropertyName 表示集合的主键应该赋值给我这个导航对象的哪个属性。
        /// </summary>
        public string IdPropertyAfterSelection
        {
            get { return this._IdPropertyName; }
            set { this.SetValue(ref this._IdPropertyName, value); }
        }
    }

    [Flags]
    public enum ShowInWhere
    {
        /// <summary>
        /// 默认值：不显示。
        /// </summary>
        Hide = 0,

        /// <summary>
        /// 是否在下拉框中显示
        /// </summary>
        DropDown = 1,

        /// <summary>
        /// 是否在列表中显示
        /// </summary>
        List = 2,

        /// <summary>
        /// 是否在表单中显示
        /// </summary>
        Detail = 4,

        /// <summary>
        /// 显示在列表和表单中
        /// </summary>
        ListDetail = List | Detail,

        /// <summary>
        /// 显示在列表和下拉框中
        /// </summary>
        ListDropDown = List | DropDown,

        [Label("全显示")]
        All = DropDown | List | Detail
    }
}