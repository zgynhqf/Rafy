using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC
{
    /// <summary>
    /// 基类
    /// </summary>
    [Serializable]
    public abstract class JXCEntity : IntEntity
    {
        #region 构造函数

        public JXCEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected JXCEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        //static JXCEntity()
        //{
        //    //测试数据库升级的临时代码
        //    for (int i = 0; i < 100; i++)
        //    {
        //        P<JXCEntity>.RegisterExtension("ZExt_" + i, typeof(JXCEntity), string.Empty);
        //    }
        //}
    }

    [Serializable]
    public abstract class JXCEntityList : EntityList { }

    public abstract class JXCEntityRepository : EntityRepository { }

    [DataProviderFor(typeof(JXCEntityRepository))]
    public class JXCEntityDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "JXC";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }
}