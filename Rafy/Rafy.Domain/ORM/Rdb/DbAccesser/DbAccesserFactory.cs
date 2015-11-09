/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130523
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 16:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 使用此工厂来创建一个被管理连接的数据库访问器。
    /// </summary>
    public static class DbAccesserFactory
    {
        /// <summary>
        /// 根据配置文件，构造一个数据库访问器。
        /// </summary>
        /// <param name="dbSettingName">数据库配置的名称。</param>
        /// <returns></returns>
        public static IDbAccesser Create(string dbSettingName)
        {
            var setting = DbSetting.FindOrCreate(dbSettingName);
            return Create(setting);
        }

        /// <summary>
        /// 根据配置文件，构造一个数据库访问器。
        /// </summary>
        /// <param name="dbSetting">数据库配置。</param>
        /// <returns></returns>
        public static IDbAccesser Create(DbSetting dbSetting)
        {
            return new ManagedConnectionDbAccesser(dbSetting);
        }

        /// <summary>
        /// 根据配置文件，构造一个数据库访问器。
        /// </summary>
        /// <param name="repository">实体仓库所代表的数据库配置。</param>
        /// <returns></returns>
        public static IDbAccesser Create(IRepository repository)
        {
            var setting = RdbDataProvider.Get(repository).DbSetting;
            return new ManagedConnectionDbAccesser(setting);
        }
    }
}
