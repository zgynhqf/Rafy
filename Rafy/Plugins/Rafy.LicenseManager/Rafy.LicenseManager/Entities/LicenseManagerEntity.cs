/*******************************************************
 * 
 * ���ߣ��ξ���
 * �������ڣ�20160921
 * ˵�������ļ�ֻ����һ���࣬�������ݼ�����ע�͡�
 * ���л�����.NET 4.5
 * �汾�ţ�1.0.0
 * 
 * ��ʷ��¼��
 * �����ļ� �ξ��� 20160921 10:00
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
    public abstract class LicenseManagerEntity : IntEntity
    {
        #region ���캯��

        protected LicenseManagerEntity() { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected LicenseManagerEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

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