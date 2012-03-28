using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using SimpleCsla.Core;
using System.Collections;

namespace OEA.Library
{
    public abstract partial class EntityList
    {
        protected virtual void OnSave()
        {
            //using (var ctx = ConnectionManager<SqlConnection>.GetManager(this.ConnectionString))
            {
                var repo = this.FindRepository();

                var toDelete = this.DeletedList;

                if (this.SupportTree)
                {
                    /*********************** 代码块解释 *********************************
                     * 
                     * 如果是树型列表的删除，需要特殊地处理父子间的关系
                     * 
                     * 目前 B/S 端还是有问题：
                     * 当把结点 a 移动到 b 下，然后再把 b 删除后再保存，并不能把 a 删除。
                     * 
                    **********************************************************************/

                    //有时，客户端会把非根对象也传输过来，这时需要过滤一下。
                    var rootsToDelete = toDelete.Where(i => toDelete.All(j => i.TreePId != i.TreeId)).ToList();

                    foreach (var root in rootsToDelete.OrderByDescending(e => e.TreeCode))
                    {
                        //找到所有的子对象
                        var recurChildren = repo.GetByTreeParentCode(root.TreeCode);

                        //有些子对象可能已经被移动到非删除列表中了，需要把这些对象从删除列表中移除。
                        for (int i = recurChildren.Count - 1; i >= 0; i--)
                        {
                            var c = recurChildren[i];
                            if (this.Any(e => e.Id == c.Id))
                            {
                                //需要先把这些对象的 TreePId 置为空，否则父对象不能被删除。
                                c.TreePId = null;
                                repo.Save(c);

                                recurChildren.RemoveAt(i);
                            }
                        }

                        //反向删除所有子结点，然后再删除根结点
                        for (int i = recurChildren.Count - 1; i >= 0; i--)
                        {
                            var child = recurChildren[i];
                            child.MarkDeleted();

                            repo.Save(child);
                        }
                        repo.Save(root);
                    }
                }
                else
                {
                    foreach (var child in toDelete) { repo.Save(child); }
                }

                toDelete.Clear();

                foreach (var item in this) { repo.Save(item); }
            }
        }
    }
}
