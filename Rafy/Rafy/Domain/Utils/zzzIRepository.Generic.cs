//*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20130423
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20130423 10:51
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Hxy;

//namespace Rafy.Domain
//{
//    /// <summary>
//    /// 强类型接口的适配器。
//    /// </summary>
//    /// <typeparam name="TEntity"></typeparam>
//    /// <typeparam name="TEntityList"></typeparam>
//    public interface IRepository<TEntity, TEntityList>
//        where TEntity : Entity
//    {
//        /// <summary>
//        /// 创建一个全新的实体对象
//        /// </summary>
//        /// <returns></returns>
//        TEntity New();

//        /// <summary>
//        /// 创建一个全新的实体列表对象
//        /// </summary>
//        /// <returns></returns>
//        TEntityList NewList();

//        /// <summary>
//        /// 优先使用缓存中的数据来通过 Id 获取指定的实体对象
//        /// 
//        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetById 并返回结果。
//        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        TEntity CacheById(int id);

//        /// <summary>
//        /// 优先使用缓存中的数据来查询所有的实体类
//        /// 
//        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetAll 并返回结果。
//        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
//        /// </summary>
//        /// <returns></returns>
//        TEntityList CacheAll();

//        /// <summary>
//        /// 通过Id获取指定的实体对象
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="withCache">是否优先使用缓存中的数据</param>
//        /// <returns></returns>
//        TEntity GetById(int id);

//        /// <summary>
//        /// 查询所有的实体类
//        /// </summary>
//        /// <param name="withCache">是否优先使用缓存中的数据</param>
//        /// <returns></returns>
//        TEntityList GetAll();

//        /// <summary>
//        /// 以分页的方式查询所有实体。
//        /// </summary>
//        /// <param name="pagingInfo"></param>
//        /// <returns></returns>
//        TEntityList GetAll(PagingInfo pagingInfo);

//        /// <summary>
//        /// 获取指定 id 集合的实体列表。
//        /// </summary>
//        /// <param name="idList"></param>
//        /// <returns></returns>
//        TEntityList GetByIdList(params int[] idList);

//        /// <summary>
//        /// 通过父对象的 Id 列表查询所有的实体。
//        /// </summary>
//        /// <param name="parentIdList"></param>
//        /// <returns></returns>
//        TEntityList GetByParentIdList(params int[] parentIdList);

//        /// <summary>
//        /// 通过父对象 Id 获取子对象的集合。
//        /// </summary>
//        /// <param name="parentId"></param>
//        /// <returns></returns>
//        TEntityList GetByParentId(int parentId);

//        /// <summary>
//        /// 通过父对象 Id 分页查询子对象的集合。
//        /// </summary>
//        /// <param name="parentId"></param>
//        /// <param name="pagingInfo"></param>
//        /// <returns></returns>
//        TEntityList GetByParentId(int parentId, PagingInfo pagingInfo);

//        /// <summary>
//        /// 递归查找所有树型子
//        /// </summary>
//        /// <param name="treeIndex"></param>
//        /// <returns></returns>
//        TEntityList GetByTreeParentIndex(string treeIndex);
//    }
//}
