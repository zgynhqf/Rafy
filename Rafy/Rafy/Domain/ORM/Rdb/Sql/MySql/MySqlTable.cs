/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170104 10:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.MySql
{
    /// <summary>
    /// MySql的表对象
    /// </summary>
    internal sealed class MySqlTable : RdbTable
    {
        /// <summary>
        /// 构造函数 初始化仓库对象
        /// </summary>
        /// <param name="repository">仓库对象</param>
        public MySqlTable(IRepositoryInternal repository) : base(repository) { }

        /// <summary>
        /// 创建Sql生成器对象
        /// </summary>
        /// <returns></returns>
        public override SqlGenerator CreateSqlGenerator()
        {
            return new MySqlGenerator();
        }

        /// <summary>
        /// 获取数据分页位置的类别
        /// </summary>
        /// <param name="pagingInfo">分页对象</param>
        /// <returns></returns>
        protected override PagingLocation GetPagingLocation(PagingInfo pagingInfo)
        {
            return PagingLocation.Database;
        }

        /// <summary>
        /// 追加引用符号
        /// </summary>
        /// <param name="sql">sql文本写入器</param>
        /// <param name="identifier">标识符</param>
        internal override void AppendQuote(TextWriter sql, string identifier)
        {
            sql.Write("`");
            this.AppendPrepare(sql, identifier);
            sql.Write("`");
        }
    }
}