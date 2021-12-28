/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210814
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210814 02:46
 * 
*******************************************************/

using Rafy.Data;
using Rafy.Domain.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.WPF
{
    /// <summary>
    /// 客户端使用的，不太精确的服务器时间提供程序。
    /// 该程序调用数据库中的 GETDATE 函数来查询服务器时间。
    /// </summary>
    public class DbTimeCache
    {
        public static readonly DbTimeCache Instance = new DbTimeCache();

        private DbTimeCache() { }

        /// <summary>
        /// 存储每一个数据库的服务器时间与客户端时间的差距。
        /// </summary>
        private Dictionary<DbSetting, TimeSpan> _distances = new Dictionary<DbSetting, TimeSpan>();

        /// <summary>
        /// 获取指定数据库的当前时间。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public DateTime Get(string dbSettingName)
        {
            var dbSettings = DbSetting.FindOrCreate(dbSettingName);

            return this.Get(dbSettings);
        }

        /// <summary>
        /// 获取指定数据库的当前时间。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public DateTime Get(DbSetting dbSetting)
        {
            if (!_distances.TryGetValue(dbSetting, out var ts))
            {
                var dbTime = GetFromDb(dbSetting);
                ts = dbTime - DateTime.Now;
                _distances.Add(dbSetting, ts);
            }

            //在第一次查询服务器时间与本地时间的差值之后，后续都直接使用本地时间来模拟服务器时间。
            return DateTime.Now + ts;
        }

        private DateTime GetFromDb(DbSetting dbSetting)
        {
            using (var dba = DbAccesserFactory.Create(dbSetting))
            {
                return (DateTime)dba.QueryValue("SELECT GETDATE()");
            }
        }
    }
}
