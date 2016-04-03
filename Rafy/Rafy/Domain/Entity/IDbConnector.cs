/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101229
 * 说明：IDb工厂
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101229
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM;
using Rafy.Data;

namespace Rafy.Domain
{
    /// <summary>
    /// 数据库连接器。
    /// </summary>
    public interface IDbConnector
    {
        /// <summary>
        /// 临时创建一个IDb对象
        /// </summary>
        /// <returns></returns>
        IDbAccesser CreateDbAccesser();

        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        DbSetting DbSetting { get; }

        ///// <summary>
        ///// 获取该实体对应的 ORM 运行时对象。
        ///// 
        ///// 如果该实体没有对应的实体元数据或者该实体没有被配置为映射数据库，
        ///// 则本方法则无法创建对应的 ORM 运行时，此时会返回 null。
        ///// </summary>
        ///// <returns></returns>
        //ITable GetDbTable();
    }
}
