/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using JXC.WPF.Templates;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace JXC.WPF
{
    public class OtherStorageOutModule : ConditionQueryModule
    {
        protected override void OnItemCreated(Entity entity)
        {
            base.OnItemCreated(entity);

            var code = RF.Concrete<AutoCodeInfoRepository>().GetOrCreateAutoCode<OtherStorageOutBill>();
            var p = entity as OtherStorageOutBill;
            p.Code = code;

            p.Storage = RF.Concrete<StorageRepository>().GetDefault();
        }
    }
}