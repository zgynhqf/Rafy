/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150510
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150510 23:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 一个连接的管理容器
    /// </summary>
    internal interface IConnectionManager : IDisposable
    {
        /// <summary>
        /// 对应的连接对象。
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 对应的数据库配置信息
        /// </summary>
        DbSetting DbSetting { get; }
    }
}
