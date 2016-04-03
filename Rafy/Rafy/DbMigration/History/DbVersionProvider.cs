/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120107
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120107
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Data;
using System.Data.Common;

namespace Rafy.DbMigration.History
{
    /// <summary>
    /// 版本号提供程序
    /// </summary>
    public abstract class DbVersionProvider
    {
        public static readonly DateTime DefaultMinTime = new DateTime(2000, 1, 1);

        protected internal DbSetting DbSetting { get; internal set; }

        internal DateTime GetDbVersion()
        {
            return this.GetDbVersionCore();
        }

        internal Result SetDbVersion(DateTime version)
        {
            return this.SetDbVersionCore(version);
        }

        protected abstract DateTime GetDbVersionCore();

        protected abstract Result SetDbVersionCore(DateTime version);

        /// <summary>
        /// 当前的值是否直接存储在当前数据库中。
        /// </summary>
        /// <returns></returns>
        protected internal abstract bool IsEmbaded();
    }
}