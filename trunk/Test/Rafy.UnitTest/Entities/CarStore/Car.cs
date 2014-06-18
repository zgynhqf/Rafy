/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140507
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140507 23:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace UT
{
    /// <summary>
    /// 车辆
    /// </summary>
    [RootEntity, Serializable]
    public partial class Car : UnitTestEntity
    {
        #region 构造函数

        public Car() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Car(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Car>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<Car>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<DateTime> AddTimeProperty = P<Car>.Register(e => e.AddTime);
        public DateTime AddTime
        {
            get { return this.GetProperty(AddTimeProperty); }
            set { this.SetProperty(AddTimeProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 车辆 列表类。
    /// </summary>
    [Serializable]
    public partial class CarList : UnitTestEntityList { }

    /// <summary>
    /// 车辆 配置类。
    /// 负责 车辆 类的实体元数据的配置。
    /// </summary>
    internal class CarConfig : UnitTestEntityConfig<Car>
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