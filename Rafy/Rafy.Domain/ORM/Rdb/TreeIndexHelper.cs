/*******************************************************
 *
 * 作者：胡庆访
 * 创建日期：20140814
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 *
 * 历史记录：
 * 创建文件 胡庆访 20140814 15:49
 *
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 目前本类只支持 Rdb。
    /// </summary>
    public static class TreeIndexHelper
    {
        /// <summary>
        /// 重新设置整个表的所有 TreeIndex。
        /// 注意，此方法只保证生成的 TreeIndex 有正确的父子关系，同时顺序有可能被打乱。
        /// </summary>
        /// <param name="repository"></param>
        public static void ResetTreeIndex(EntityRepository repository)
        {
            using (var tran =  RF.TransactionScope(repository))
            {
                //先清空
                var dp = repository.DataProvider as RdbDataProvider;
                if (dp == null)
                {
                    throw new InvalidProgramException("TreeIndexHelper.ResetTreeIndex 方法只支持在关系数据库上使用。");
                }
                var table = dp.DbTable;
                var column = table.Columns.First(c => c.Info.Property == Entity.TreeIndexProperty);
                using (var dba = dp.CreateDbAccesser())
                {
                    dba.ExecuteText(string.Format("UPDATE {0} SET {1} = NULL", table.Name, column.Name));
                }

                var all = repository.GetAll();
                if (all.Count > 0)
                {
                    //如果加载的过程中，第一个节点刚好是根节点，
                    //则加载完成后是一棵完整的树，Index 也生成完毕，不需要再次处理。
                    if (all.IsTreeRootList)
                    {
                        all.ResetTreeIndex();
                        repository.Save(all);
                    }
                    else
                    {
                        var cloneOptions = CloneOptions.ReadSingleEntity();
                        var oldList = new List<Entity>();
                        all.EachNode(e =>
                        {
                            var cloned = repository.New();
                            cloned.Clone(e, cloneOptions);
                            cloned.PersistenceStatus = PersistenceStatus.Unchanged;

                            oldList.Add(cloned);
                            return false;
                        });

                        var newList = repository.NewList();
                        while (oldList.Count > 0)
                        {
                            foreach (var item in oldList)
                            {
                                var treePId = item.TreePId;
                                if (treePId == null)
                                {
                                    newList.Add(item);
                                    oldList.Remove(item);
                                    break;
                                }
                                else
                                {
                                    var parent = newList.EachNode(e => e.Id.Equals(treePId));
                                    if (parent != null)
                                    {
                                        parent.TreeChildren.LoadAdd(item);
                                        oldList.Remove(item);
                                        break;
                                    }
                                }
                            }
                        }
                        TreeHelper.MarkTreeFullLoaded(newList);
                        newList.ResetTreeIndex();
                        repository.Save(newList);
                    }
                }

                tran.Complete();
            }
        }
    }
}