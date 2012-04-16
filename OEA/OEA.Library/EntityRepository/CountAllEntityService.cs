/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace OEA.Library
{
    /// <summary>
    /// 本服务用于统计所有的指定实体的数据
    /// </summary>
    [Serializable]
    public class CountAllEntityService : Service
    {
        [ServiceInput]
        public Type EntityType { get; set; }

        [ServiceOutput]
        public int Count { get; set; }

        protected override void Execute()
        {
            //暂时使用比较简单的方式来获取，以后可统一变更。
            this.Count = RF.Create(this.EntityType).GetAll().Count;
        }
    }
}
