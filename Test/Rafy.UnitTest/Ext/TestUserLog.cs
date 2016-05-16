/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160516
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160516 16:29
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

namespace UT
{
    /// <summary>
    /// 用户的行为日志
    /// </summary>
    [ChildEntity, Serializable]
    public partial class TestUserLog : UnitTestEntity
    {
        #region 构造函数

        public TestUserLog() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TestUserLog(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty TestUserIdProperty =
            P<TestUserLog>.RegisterRefId(e => e.TestUserId, ReferenceType.Parent);
        public int TestUserId
        {
            get { return (int)this.GetRefId(TestUserIdProperty); }
            set { this.SetRefId(TestUserIdProperty, value); }
        }
        public static readonly RefEntityProperty<TestUser> TestUserProperty =
            P<TestUserLog>.RegisterRef(e => e.TestUser, TestUserIdProperty);
        public TestUser TestUser
        {
            get { return this.GetRefEntity(TestUserProperty); }
            set { this.SetRefEntity(TestUserProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 用户的行为日志 列表类。
    /// </summary>
    [Serializable]
    public partial class TestUserLogList : UnitTestEntityList { }

    /// <summary>
    /// 用户的行为日志 仓库类。
    /// 负责 用户的行为日志 类的查询、保存。
    /// </summary>
    public partial class TestUserLogRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected TestUserLogRepository() { }
    }

    /// <summary>
    /// 用户的行为日志 配置类。
    /// 负责 用户的行为日志 类的实体元数据的配置。
    /// </summary>
    internal class TestUserLogConfig : UnitTestEntityConfig<TestUserLog>
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