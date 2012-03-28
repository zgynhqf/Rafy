/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 客户端向服务端获取表信息的命令
    /// </summary>
    [Serializable]
    class GetTableInfoCommand : SimpleCsla.ServiceBase
    {
        public TableInfo ResultTableInfo { get; private set; }

        /// <summary>
        /// Type.AssemblyQualifiedName
        /// </summary>
        public string EntityTypeName { get; set; }

        protected override void DataPortal_Execute()
        {
            var entityType = Type.GetType(this.EntityTypeName, true, true);
            var repository = RepositoryFactory.Instance.Find(entityType);
            var tiRepo = new TableInfoFinder(repository);
            var serverTable = tiRepo.GetTableInfo(entityType);
            this.ResultTableInfo = TableInfo.Convert(serverTable);
        }
    }
}
