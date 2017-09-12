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
    /// 实体的领域名称
    /// </summary>
    [RootEntity, Serializable]
    public partial class CarEntity : UnitTestEntity
    {
        #region 构造函数

        public CarEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected CarEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<CarEntity>.Register(e => e.Name);
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<byte> ByteNameProperty = P<CarEntity>.Register(e => e.ByteName);
        /// <summary>
        /// 
        /// </summary>
        public byte ByteName
        {
            get { return this.GetProperty(ByteNameProperty); }
            set { this.SetProperty(ByteNameProperty, value); }
        }


        public static readonly Property<float> FloatNameProperty = P<CarEntity>.Register(e => e.FloatName);
        /// <summary>
        /// 
        /// </summary>
        public float FloatName
        {
            get { return this.GetProperty(FloatNameProperty); }
            set { this.SetProperty(FloatNameProperty, value); }
        }

        public static readonly Property<double> DoubleNameProperty = P<CarEntity>.Register(e => e.DoubleName);
        /// <summary>
        /// 
        /// </summary>
        public double DoubleName
        {
            get { return this.GetProperty(DoubleNameProperty); }
            set { this.SetProperty(DoubleNameProperty, value); }
        }


        public static readonly Property<decimal> DecimalNameProperty = P<CarEntity>.Register(e => e.DecimalName);
        /// <summary>
        /// 
        /// </summary>
        public decimal DecimalName
        {
            get { return this.GetProperty(DecimalNameProperty); }
            set { this.SetProperty(DecimalNameProperty, value); }
        }


        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class CarEntityList : UnitTestEntityList { }

    /// <summary>
    /// 实体的领域名称 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class CarEntityRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected CarEntityRepository() { }
    }

    /// <summary>
    /// 实体的领域名称 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class CarEntityConfig : UnitTestEntityConfig<CarEntity>
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