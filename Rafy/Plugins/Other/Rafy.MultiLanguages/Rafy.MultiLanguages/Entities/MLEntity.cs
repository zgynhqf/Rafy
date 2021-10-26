/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121108 10:36
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121108 10:36
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
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.MultiLanguages
{
    public abstract class MLEntity : IntEntity
    {
        //public static string ConnectionString = "MultiLanuages";

        //protected override string ConnectionStringSettingName
        //{
        //    get { return ConnectionString; }
        //}
    }

    public abstract class MLEntityList : EntityList { }

    [Serializable]
    public abstract class MLEntityRepository : EntityRepository { }
}