/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using SimpleCsla;
using System.Diagnostics;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 某个实体类的TableInfo的查找器
    /// </summary>
    class TableInfoFinder
    {
        private IDbFactory _dbFactory;

        private Type _entityType;

        public TableInfoFinder(IDbFactory dbFactory)
        {
            if (dbFactory == null) throw new ArgumentNullException("db");

            this._dbFactory = dbFactory;
        }

        /// <summary>
        /// 查找某个实体类的表信息
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public ITable GetTableInfo(Type entityType)
        {
            this._entityType = entityType;

            ITable fromDb = null;

            //这里加载表信息时，可能需要和服务器交互。
            if (OEAEnvironment.Location.IsOnServer())
            {
                fromDb = this.GetTableInfo_OnServer();
            }
            else
            {
                fromDb = this.GetTableInfo_OnClient();
            }

            return fromDb;
        }

        /// <summary>
        /// 客户端的实现方法
        /// </summary>
        /// <returns></returns>
        private ITable GetTableInfo_OnClient()
        {
            var cmd = new GetTableInfoService() { EntityTypeName = this._entityType.AssemblyQualifiedName };
            cmd.Invoke(out cmd);
            return cmd.ResultTableInfo;
        }

        /// <summary>
        /// 服务端的实现方法
        /// </summary>
        /// <returns></returns>
        private ITable GetTableInfo_OnServer()
        {
            return this.GetByLiteORM();
        }

        private ITable GetByLiteORM()
        {
            using (var db = this._dbFactory.CreateDb())
            {
                var tableInfo = db.GetTable(this._entityType);
                Debug.Assert(tableInfo != null, "类型不能操作数据库！");
                return tableInfo;
            }
        }
    }
}