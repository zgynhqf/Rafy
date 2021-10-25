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
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

namespace Rafy.LicenseManager.Entities
{
    [Serializable]
    public abstract class LicenseManagerEntity : IntEntity { }

    [Serializable]
    public abstract class LicenseManagerEntityList : EntityList { }

    public abstract class LicenseManagerEntityRepository : EntityRepository
    {
        public static string DbSettingName = "LicenseDb";

        protected LicenseManagerEntityRepository() { }
    }

    [DataProviderFor(typeof(LicenseManagerEntityRepository))]
    public class LicenseManagerEntityDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return LicenseManagerEntityRepository.DbSettingName; }
        }
    }

    public abstract class LicenseManagerEntityConfig<TEntity> : EntityConfig<TEntity> { }
    public abstract class LicenseManagerEntityWPFViewConfig<TEntity> : WPFViewConfig<TEntity> { }
    public abstract class LicenseManagerEntityWebViewConfig<TEntity> : WebViewConfig<TEntity> { }
}