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
using OEA.ORM;
using hxy.Common.Data;

namespace OEA.Library
{
    /// <summary>
    /// IDb工厂
    /// </summary>
    public interface IDbFactory
    {
        /// <summary>
        /// 临时创建一个IDb对象
        /// </summary>
        /// <returns></returns>
        IDb CreateDb();

        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        DbSetting DbSetting { get; }

        /// <summary>
        /// 获取该实体对应的数据库 映射信息运行时对象。
        /// </summary>
        /// <returns></returns>
        ITable GetORMTable();
    }
}