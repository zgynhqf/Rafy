/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110106
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110106
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using Rafy.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 手工迁移操作的容器对象
    /// </summary>
    public class ManualMigrationsContainer : Collection<ManualDbMigration>
    {
        private DbSetting _dbSetting;

        private bool _initialized;

        internal void TryInitalize(DbSetting dbSetting)
        {
            this._dbSetting = dbSetting;

            if (!this._initialized)
            {
                this.OnInit();

                this._initialized = true;
            }
        }

        /// <summary>
        /// 子类重写此方法并调用 AddByAssembly 方法来自动添加某个程序集类所有对应该数据库的手工迁移
        /// </summary>
        protected virtual void OnInit() { }

        public void AddByAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(ManualDbMigration)) &&
                    !type.IsAbstract && !type.IsGenericTypeDefinition)
                {
                    var migration = Activator.CreateInstance(type, true) as ManualDbMigration;
                    if (migration.DbSetting == this._dbSetting.Name)
                    {
                        this.Add(migration);
                    }
                }
            }
        }
    }
}