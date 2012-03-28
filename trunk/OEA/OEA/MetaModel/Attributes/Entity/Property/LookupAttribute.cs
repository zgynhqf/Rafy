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


namespace OEA.MetaModel.Attributes
{
    /// <summary>
    /// 字段关联属性,新增时通过选择操作
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class LookupAttribute : Attribute
    {
        /// <summary>
        /// [Lookup(typeof(PBSPropertyValue), SelectedValuePath = "Value", DataSourceProperty = "OptionalValues")]
        /// [Lookup(typeof(IndustryDictionary), SelectedValuePath = "Name")]
        /// </summary>
        /// <param name="lookupType"></param>
        public LookupAttribute(Type lookupType)
            : this(string.Empty, ReferenceType.Normal)
        {
            this.LookupType = lookupType;
        }

        /// <summary>
        /// 如果在托管属性中已经标记好实体属性名和引用类型，则可以使用这个构造函数。
        /// </summary>
        public LookupAttribute()
        {
            this.SelectionMode = ReferenceSelectionMode.Single;
        }

        public LookupAttribute(string lookupPropertyName) : this(lookupPropertyName, ReferenceType.Normal) { }

        public LookupAttribute(string lookupPropertyName, ReferenceType referenceType)
        {
            this.LookupPropertyName = lookupPropertyName;
            this.ReferenceType = referenceType;
            this.SelectionMode = ReferenceSelectionMode.Single;
        }

        #region 描述关系

        /// <summary>
        /// 此引用的类型
        /// </summary>
        public ReferenceType ReferenceType { get; private set; }

        /// <summary>
        /// 定义在对象自身上的属性的名字，当下拉选择时，会把选择的对象赋值给这个属性
        /// </summary>
        public string LookupPropertyName { get; private set; }

        /// <summary>
        /// Lookup对象类型
        /// </summary>
        public Type LookupType { get; private set; }

        #endregion

        #region 描述视图

        /// <summary>
        /// 选择的模式：多选/单选。
        /// </summary>
        public ReferenceSelectionMode SelectionMode { get; set; }

        /// <summary>
        /// 一个路径表达式。
        /// 下拉选择时，从当前选中的对象找到值，赋值给LookupPropertyName指定的属性
        /// </summary>
        public string SelectedValuePath { get; set; }

        /// <summary>
        /// 查询时，数据来源的属性。在这个属性里面查找值。
        /// 级联属性过滤串,格式如:属性.子属性.子子属性...
        /// 
        /// 如果未设置这个值，则会调用数据层方法查询
        /// </summary>
        public string DataSourceProperty { get; set; }

        /// <summary>
        /// 如果Lookup是树形结构，设置或获取RootPId的属性名，可以只显示部分树
        /// </summary>
        public string RootPIdProperty { get; set; }

        #endregion
    }
}