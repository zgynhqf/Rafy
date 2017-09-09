﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120103
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120103
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using System.ComponentModel;
using Rafy.DbMigration.Operations;
using System.Data;
using System.Diagnostics;
using Rafy.Data;
using System.Text.RegularExpressions;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 表示一个可升级、可回滚的数据库升级项。
    /// 
    /// 该类及该类的子类需要支持 Xml 序列化，以支持存储到历史库中。
    /// </summary>
    [DebuggerDisplay("{Description}")]
    public abstract class DbMigration
    {
        private List<MigrationOperation> _operations = new List<MigrationOperation>();

        /// <summary>
        /// 本次迁移对应的时间点。
        /// </summary>
        /// <value>
        /// The time unique identifier.
        /// </value>
        public abstract DateTime TimeId { get; }

        /// <summary>
        /// 迁移描述。
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public abstract string Description { get; }

        internal void GenerateUpOperations()
        {
            this._operations.Clear();

            this.Up();
        }

        internal void GenerateDownOperations()
        {
            this._operations.Clear();

            this.Down();
        }

        /// <summary>
        /// 是否自动生成的更新项
        /// </summary>
        internal abstract MigrationType GetMigrationType();

        internal IMetadataReader DatabaseMetaReader { get; set; }

        internal IList<MigrationOperation> Operations
        {
            get { return this._operations; }
        }

        /// <summary>
        /// 返回当前更新项的类型
        /// </summary>
        public MigrationType MigrationType
        {
            get { return this.GetMigrationType(); }
        }


        /// <summary>
        /// 数据库升级
        /// 注意，开发者在实现此方法时，不能在方法中直接操作数据库，而是应该通过 <see cref="AddOperation(MigrationOperation)"/>、<see cref="ManualDbMigration.RunCode(Action{IDbAccesser})"/>、<see cref="ManualDbMigration.RunSql(string)"/> 等方法添加一些数据库的操作，进而由框架来统一规划与操作数据库。
        /// 另外，为了保证能正确的回滚、升级数据库，在实现 Up 方法的同时，应该实现相应的 <see cref="Down"/> 方法，来实现数据库操作的回滚。如果因为某些原因无法实现 Down 方法的话，则应该保证 Up 方法中的代码是可以重入的。
        /// </summary>
        protected abstract void Up();

        /// <summary>
        /// 数据库回滚
        /// 详细注释，见：<see cref="Up"/> 方法。
        /// </summary>
        protected abstract void Down();

        /// <summary>
        /// 在 Up/Down 方法中调用此方法来添加迁移操作。
        /// </summary>
        /// <param name="operation"></param>
        protected void AddOperation(MigrationOperation operation)
        {
            this._operations.Add(operation);
        }
    }

    /// <summary>
    /// 迁移类型
    /// </summary>
    public enum MigrationType
    {
        /// <summary>
        /// 自动生成的更新项
        /// </summary>
        AutoMigration,

        /// <summary>
        /// 手工更新项
        /// </summary>
        ManualMigration
    }
}