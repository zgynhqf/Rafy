/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20160921 10:00
 * 
*******************************************************/

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.LicenseManager.Infrastructure;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.LicenseManager.Entities
{
    /// <summary>
    /// 授权信息实体的领域名称
    /// </summary>
    [RootEntity, Serializable]
    public partial class LicenseEntity : LicenseManagerEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> PrivateKeyProperty = P<LicenseEntity>.Register(e => e.PrivateKey);
        /// <summary>
        /// 获取或设置 RSA 私钥。
        /// </summary>
        public string PrivateKey
        {
            get { return this.GetProperty(PrivateKeyProperty); }
            set { this.SetProperty(PrivateKeyProperty, value); }
        }

        public static readonly Property<string> PublicKeyProperty = P<LicenseEntity>.Register(e => e.PublicKey);
        /// <summary>
        /// 获取或设置 RSA 公钥。
        /// </summary>
        public string PublicKey
        {
            get { return this.GetProperty(PublicKeyProperty); }
            set { this.SetProperty(PublicKeyProperty, value); }
        }

        public static readonly Property<string> LicenseCodeProperty = P<LicenseEntity>.Register(e => e.LicenseCode);
        /// <summary>
        /// 获取或设置授权码。
        /// </summary>
        public string LicenseCode
        {
            get { return this.GetProperty(LicenseCodeProperty); }
            set { this.SetProperty(LicenseCodeProperty, value); }
        }

        public static readonly Property<string> MacCodeProperty = P<LicenseEntity>.Register(e => e.MacCode);
        /// <summary>
        /// 获取或设置 MAC 地址。
        /// </summary>
        public string MacCode
        {
            get { return this.GetProperty(MacCodeProperty); }
            set { this.SetProperty(MacCodeProperty, value); }
        }

        public static readonly Property<LicenseTarget> LicenseTargetProperty = P<LicenseEntity>.Register(e => e.LicenseTarget);
        /// <summary>
        /// 获取或设置授权类型。
        /// </summary>
        public LicenseTarget LicenseTarget
        {
            get { return this.GetProperty(LicenseTargetProperty); }
            set { this.SetProperty(LicenseTargetProperty, value); }
        }

        public static readonly Property<DateTime> ExpireTimeProperty = P<LicenseEntity>.Register(e => e.ExpireTime);
        /// <summary>
        /// 获取或设置授权过期时间。
        /// </summary>
        public DateTime ExpireTime
        {
            get { return this.GetProperty(ExpireTimeProperty); }
            set { this.SetProperty(ExpireTimeProperty, value); }
        }

        public static readonly Property<DateTime> CreateTimeProperty = P<LicenseEntity>.Register(e => e.CreateTime);
        /// <summary>
        /// 获取或设置创建时间。
        /// </summary>
        public DateTime CreateTime
        {
            get { return this.GetProperty(CreateTimeProperty); }
            set { this.SetProperty(CreateTimeProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 表示授权类型的枚举值。
    /// </summary>
    [TypeConverter(typeof(DescriptionConverter))]
    public enum LicenseTarget
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("")]
        None = 0,

        /// <summary>
        /// 开发环境
        /// </summary>
        [Description("开发环境")]
        Development = 1,

        /// <summary>
        /// 生产环境。
        /// </summary>
        [Description("生产环境")]
        Production = 16,

        [Description("ACME")]
        Acme = 18,

        /// <summary>
        /// VICA
        /// </summary>
        [Description("VICA")]
        Vica = 20,

        /// <summary>
        /// DBK 本地版
        /// </summary>
        [Description("DBK(本地版)")]
        Dbk = 22,

        /// <summary>
        /// UK
        /// </summary>
        [Description("UK")]
        Uk = 24
    }

    /// <summary>
    /// 授权信息实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class LicenseEntityList : LicenseManagerEntityList { }

    /// <summary>
    /// 授权信息实体的领域名称 仓库类。
    /// 负责 授权信息实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class LicenseEntityRepository : LicenseManagerEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected LicenseEntityRepository() { }
    }

    /// <summary>
    /// 授权信息实体的领域名称 配置类。
    /// 负责 授权信息实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class LicenseEntityConfig : LicenseManagerEntityConfig<LicenseEntity>
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
