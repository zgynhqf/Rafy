/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150816
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150816 12:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.BatchSubmit
{
    /// <summary>
    /// 实体的批量导入器。
    /// </summary>
    public interface IBatchImporter
    {
        /// <summary>
        /// 每次导入时，一批最多同时导入多少条数据。
        /// </summary>
        int BatchSize { get; set; }

        /// <summary>
        /// 是否在更新时，是否一并更新 LOB 属性。默认为 false。
        /// </summary>
        bool UpdateLOB { get; set; }

        /// <summary>
        /// 批量导入指定的实体或列表。
        /// </summary>
        /// <param name="entityOrList"></param>
        void Save(IDomainComponent entityOrList);
    }
}