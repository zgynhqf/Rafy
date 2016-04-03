/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Operations;
using System.IO;
using System.CodeDom.Compiler;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据库执行项 MigrationRun 的生成器
    /// 
    /// 子类继承此类以实现不同类型数据库对指定数据库操作的执行项生成。
    /// </summary>
    public abstract class RunGenerator
    {
        private List<MigrationRun> _runList = new List<MigrationRun>();

        internal IList<MigrationRun> Generate(IEnumerable<MigrationOperation> operations)
        {
            this._runList.Clear();

            foreach (var op in operations)
            {
                this.Distribute(op);
            }

            return this._runList;
        }

        /// <summary>
        /// 把抽象的操作分发到具体的生成方法上。
        /// </summary>
        /// <param name="op"></param>
        private void Distribute(MigrationOperation op)
        {
            //手工分发的原因：类并不多、可以处理类之间的继承层次关系。

            if (op is CreateNormalColumn) { this.Generate(op as CreateNormalColumn); }
            else if (op is DropNormalColumn) { this.Generate(op as DropNormalColumn); }
            else if (op is AddPKConstraint) { this.Generate(op as AddPKConstraint); }
            else if (op is RemovePKConstraint) { this.Generate(op as RemovePKConstraint); }
            else if (op is AddNotNullConstraint) { this.Generate(op as AddNotNullConstraint); }
            else if (op is RemoveNotNullConstraint) { this.Generate(op as RemoveNotNullConstraint); }
            else if (op is AddNotNullConstraintFK) { this.Generate(op as AddNotNullConstraintFK); }
            else if (op is RemoveNotNullConstraintFK) { this.Generate(op as RemoveNotNullConstraintFK); }
            else if (op is AlterColumnType) { this.Generate(op as AlterColumnType); }
            else if (op is AddFKConstraint) { this.Generate(op as AddFKConstraint); }
            else if (op is RemoveFKConstraint) { this.Generate(op as RemoveFKConstraint); }
            else if (op is CreateTable) { this.Generate(op as CreateTable); }
            else if (op is DropTable) { this.Generate(op as DropTable); }
            else if (op is CreateDatabase) { this.Generate(op as CreateDatabase); }
            else if (op is DropDatabase) { this.Generate(op as DropDatabase); }
            else if (op is RunSql) { this.Generate(op as RunSql); }
            else if (op is RunAction) { this.Generate(op as RunAction); }
            else if (op is UpdateComment) { this.Generate(op as UpdateComment); }
            else { this.Generate(op); }
        }

        #region 需要子类实现的抽象方法

        protected abstract void Generate(CreateNormalColumn op);
        protected abstract void Generate(DropNormalColumn op);
        protected abstract void Generate(AddPKConstraint op);
        protected abstract void Generate(RemovePKConstraint op);
        protected abstract void Generate(AddNotNullConstraint op);
        protected abstract void Generate(RemoveNotNullConstraint op);
        protected abstract void Generate(AddNotNullConstraintFK op);
        protected abstract void Generate(RemoveNotNullConstraintFK op);
        protected abstract void Generate(AlterColumnType op);
        protected abstract void Generate(AddFKConstraint op);
        protected abstract void Generate(RemoveFKConstraint op);
        protected abstract void Generate(CreateTable op);
        protected abstract void Generate(DropTable op);
        protected abstract void Generate(CreateDatabase op);
        protected abstract void Generate(DropDatabase op);
        protected abstract void Generate(UpdateComment op);
        protected virtual void Generate(MigrationOperation op)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region 直接实现的两个子类：RunSql、RunAction

        protected void Generate(RunSql op)
        {
            this.AddRun(new SqlMigrationRun
            {
                Sql = op.Sql
            });
        }

        protected void Generate(RunAction op)
        {
            this.AddRun(new ActionMigrationRun
            {
                Action = op.Action
            });
        }

        #endregion

        /// <summary>
        /// 获取一个缩进的 TextWriter 用于写 SQL。
        /// </summary>
        /// <returns></returns>
        protected IndentedTextWriter Writer()
        {
            return new IndentedTextWriter(new StringWriter());
        }

        /// <summary>
        /// 添加一个 SQL 语句的执行项
        /// </summary>
        /// <param name="sql"></param>
        protected void AddRun(IndentedTextWriter sql)
        {
            this.AddRun(new SqlMigrationRun
            {
                Sql = sql.InnerWriter.ToString()
            });
        }

        /// <summary>
        /// 添加一个执行项
        /// </summary>
        /// <param name="run"></param>
        protected void AddRun(MigrationRun run)
        {
            this._runList.Add(run);
        }
    }
}