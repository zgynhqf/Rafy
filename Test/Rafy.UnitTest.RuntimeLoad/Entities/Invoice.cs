/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210803
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210803 03:42
 * 
*******************************************************/

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Rafy.UnitTest.RuntimeLoad
{
    /// <summary>
    /// 发票
    /// </summary>
    [RootEntity, Serializable]
    public partial class Invoice : RuntimeLoadEntity
    {
        #region 构造函数

        public Invoice() { }

        protected Invoice(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 发票 列表类。
    /// </summary>
    [Serializable]
    public partial class InvoiceList : RuntimeLoadEntityList { }

    /// <summary>
    /// 发票 仓库类。
    /// 负责 发票 类的查询、保存。
    /// </summary>
    public partial class InvoiceRepository : RuntimeLoadEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected InvoiceRepository() { }
    }

    /// <summary>
    /// 发票 配置类。
    /// 负责 发票 类的实体元数据的配置。
    /// </summary>
    public class InvoiceConfig : RuntimeLoadEntityConfig<Invoice>
    {
        public static bool RunnedAnyTime = false;

        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            RunnedAnyTime = true;

            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}