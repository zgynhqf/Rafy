/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160921 14:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.BatchSubmit.SqlServer
{
    /// <summary>
    /// Sql Azure 数据库的实体批量导入器。
    /// 目前 Sql Azure 数据库有缺陷：于 Sql Azure 暂时不支持 DBCC CHECKIDENT 方法，所以无法获取实体的 Id。
    /// </summary>
    [Serializable]
    public class SqlAzureBatchImporter : SqlServerBatchImporter
    {
        internal override void GenerateId(EntityBatch meta, IList<Entity> entities)
        {
            //由于 Sql Azure 暂时不支持 DBCC CHECKIDENT 方法，所以无法提前生成大量 Id。
            //base.GenerateId(meta, entities);
        }

        /// <summary>
        /// 批量导入指定的实体或列表。
        /// </summary>
        /// <param name="batch"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ImportInsert(EntityBatch batch)
        {
            var entities = batch.InsertBatch;

            foreach (var section in this.EnumerateAllBatches(entities))
            {
                var table = this.ToDataTable(batch.Table, section);

                this.SaveBulk(table, batch, false);

                //this.ReadIdFromTable(entities, table);
            }
        }

        //private void ReadIdFromTable(IList<Entity> list, DataTable table)
        //{
        //    var rows = table.Rows;
        //    for (int i = 0, c = list.Count; i < c; i++)
        //    {
        //        var entity = list[i];
        //        var row = rows[i];
        //        var id = row[EntityConvention.IdColumnName];
        //        entity.Id = id;
        //    }
        //}
    }
}
