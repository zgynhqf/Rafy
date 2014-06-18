//本文件代码自动生成。用于实现强类型接口，方便应用层使用。
//为方便接口升级，请勿修改。
//自动代码模板版本号：1.1.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;

namespace $domainNamespace$
{
    partial class $domainEntityName$List : IList<$domainEntityName$>
    {
        #region IList<$domainEntityName$> 成员

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire System.Collections.ObjectModel.Collection<T>.</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf($domainEntityName$ item)
        {
            return base.IndexOf(item);
        }

        /// <summary>
        /// Inserts an element into the System.Collections.ObjectModel.Collection<T>
        /// at the specified index.</summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, $domainEntityName$ item)
        {
            base.Insert(index, item);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new $domainEntityName$ this[int index]
        {
            get
            {
                return base[index] as $domainEntityName$;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Adds an object to the end of the System.Collections.ObjectModel.Collection<T>.
        /// </summary>
        /// <param name="item"></param>
        public void Add($domainEntityName$ item)
        {
            base.Add(item);
        }

        /// <summary>
        /// Determines whether an element is in the System.Collections.ObjectModel.Collection<T>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains($domainEntityName$ item)
        {
            return base.Contains(item);
        }

        /// <summary>
        /// Copies the entire System.Collections.ObjectModel.Collection<T> to a compatible
        /// one-dimensional System.Array, starting at the specified index of the target
        /// array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo($domainEntityName$[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<$domainEntityName$>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the System.Collections.ObjectModel.Collection<T>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove($domainEntityName$ item)
        {
            return base.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<$domainEntityName$> GetEnumerator()
        {
            return new EntityListEnumerator<$domainEntityName$>(this);
        }

        #endregion
    }

    partial class $domainEntityName$Repository : IRepository<$domainEntityName$, $domainEntityName$List>
    {
        #region 私有方法，本类内部使用

        private new $domainEntityName$ FetchFirst(params object[] parameters)
        {
            return base.FetchFirst(parameters) as $domainEntityName$;
        }

        private new $domainEntityName$List FetchList(params object[] parameters)
        {
            return base.FetchList(parameters) as $domainEntityName$List;
        }

        #endregion

        #region IRepository<$domainEntityName$, $domainEntityName$List> 成员

        /// <summary>
        /// 创建一个新的实体。
        /// </summary>
        /// <returns></returns>
        public new $domainEntityName$ New()
        {
            return base.New() as $domainEntityName$;
        }

        /// <summary>
        /// 创建一个全新的列表
        /// </summary>
        /// <returns></returns>
        public new $domainEntityName$List NewList()
        {
            return base.NewList() as $domainEntityName$List;
        }

        /// <summary>
        /// 通过Id在数据层中查询指定的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public new $domainEntityName$ GetById(int id, bool withCache = true)
        {
            return base.GetById(id, withCache) as $domainEntityName$;
        }

        /// <summary>
        /// 查询所有的实体类
        /// </summary>
        /// <returns></returns>
        public new $domainEntityName$List GetAll(bool withCache = true)
        {
            return base.GetAll(withCache) as $domainEntityName$List;
        }

        /// <summary>
        /// 分页查询所有的实体类
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetAll(PagingInfo pagingInfo)
        {
            return base.GetAll(pagingInfo) as $domainEntityName$List;
        }

        /// <summary>
        /// 获取指定 id 集合的实体列表。
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetByIdList(params int[] idList)
        {
            return base.GetByIdList(idList) as $domainEntityName$List;
        }

        /// <summary>
        /// 通过组合父对象的 Id 列表，查找所有的组合子对象的集合。
        /// </summary>
        /// <param name="parentIdList"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetByParentIdList(params int[] parentIdList)
        {
            return base.GetByParentIdList(parentIdList) as $domainEntityName$List;
        }

        /// <summary>
        /// 查询某个父对象下的子对象
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetByParentId(int parentId)
        {
            return base.GetByParentId(parentId) as $domainEntityName$List;
        }

        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetByParentId(int parentId, PagingInfo pagingInfo)
        {
            return base.GetByParentId(parentId, pagingInfo) as $domainEntityName$List;
        }

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeCode"></param>
        /// <returns></returns>
        public new $domainEntityName$List GetByTreeParentCode(string treeCode)
        {
            return base.GetByTreeParentCode(treeCode) as $domainEntityName$List;
        }

        #endregion
    }
}