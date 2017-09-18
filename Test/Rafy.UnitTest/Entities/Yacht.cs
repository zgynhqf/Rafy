/*******************************************************
 * 
 * 作者：颜昌龙
 * 创建日期：20170912
 * 说明：一个测试实体属性类型的实体。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 颜昌龙 20170912 13:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using UT;

namespace Rafy.UnitTest
{
    /// <summary>
    /// 游艇
    /// </summary>
    [RootEntity, Serializable]
    public partial class Yacht : UnitTestEntity
    {
        #region 构造函数

        public Yacht() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Yacht(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Yacht>.Register(e => e.Name);
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<byte> ByteValueProperty = P<Yacht>.Register(e => e.ByteValue);
        /// <summary>
        /// 
        /// </summary>
        public byte ByteValue
        {
            get { return this.GetProperty(ByteValueProperty); }
            set { this.SetProperty(ByteValueProperty, value); }
        }


        public static readonly Property<float> FloatValueProperty = P<Yacht>.Register(e => e.FloatValue);
        /// <summary>
        /// 
        /// </summary>
        public float FloatValue
        {
            get { return this.GetProperty(FloatValueProperty); }
            set { this.SetProperty(FloatValueProperty, value); }
        }

        public static readonly Property<double> DoubleValueProperty = P<Yacht>.Register(e => e.DoubleValue);
        /// <summary>
        /// 
        /// </summary>
        public double DoubleValue
        {
            get { return this.GetProperty(DoubleValueProperty); }
            set { this.SetProperty(DoubleValueProperty, value); }
        }


        public static readonly Property<decimal> DecimalValueProperty = P<Yacht>.Register(e => e.DecimalValue);
        /// <summary>
        /// 
        /// </summary>
        public decimal DecimalValue
        {
            get { return this.GetProperty(DecimalValueProperty); }
            set { this.SetProperty(DecimalValueProperty, value); }
        }


        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 游艇 列表类。
    /// </summary>
    [Serializable]
    public partial class YachtList : UnitTestEntityList { }

    /// <summary>
    /// 游艇 仓库类。
    /// 负责 游艇 类的查询、保存。
    /// </summary>
    public partial class YachtRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected YachtRepository() { }
    }

    /// <summary>
    /// 游艇 配置类。
    /// 负责 游艇 类的实体元数据的配置。
    /// </summary>
    internal class YachtConfig : UnitTestEntityConfig<Yacht>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}