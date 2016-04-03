/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 16:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 16:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace MP
{
    [Serializable]
    public abstract class MPEntity : IntEntity
    {
        #region 构造函数

        public MPEntity()
        {
            //需要设置 Id 后，界面的选择行为才正常。
            this.Id = RafyEnvironment.NewLocalId();
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected MPEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class MPEntityList : EntityList { }

    [Serializable]
    public abstract class MPEntityRepository : EntityRepository { }

    [DataProviderFor(typeof(MPEntityRepository))]
    public class MPEntityDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "MonthPlan";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class MPEntityConfig<TEntity> : EntityConfig<TEntity> { }

    public class MPWPFViewConfig : WPFViewConfig<MPEntity>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.UseDetailLayoutMode(DetailLayoutMode.AutoGrid);
        }
    }
}