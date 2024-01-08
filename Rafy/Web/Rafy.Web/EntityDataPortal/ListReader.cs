/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy;
using Newtonsoft.Json.Linq;
using Rafy.Domain;
using System.Collections;

namespace Rafy.Web.EntityDataPortal
{
    /// <summary>
    /// Json 到 EntityList 的读取器
    /// </summary>
    internal abstract class ListReader
    {
        /// <summary>
        /// 把实体列表对应的 json 转换为 EntityList 对象。
        /// </summary>
        /// <param name="jEntityList"></param>
        /// <param name="repository">如果没有显式指定 Repository，则会根据 json 中的 _model 属性来查找对应的实体仓库。</param>
        /// <param name="resultEntityList">如果显式指定 Repository，则会把转换的实体对象都加入到这个列表中。</param>
        /// <returns></returns>
        internal static IEntityList JsonToEntity(JObject jEntityList, EntityRepository repository = null, IEntityList resultEntityList = null)
        {
            if (repository == null)
            {
                var modelProperty = jEntityList.Property(Consts.modelProperty);
                if (modelProperty == null) { throw new NotSupportedException($"实体列表对应的 Json 应该有 {Consts.modelProperty} 属性。"); }
                var model = modelProperty.Value.CastTo<JValue>().Value.CastTo<string>();
                var clientEntity = ClientEntities.Find(model);
                repository = RF.Find(clientEntity.EntityType);
            }

            ListReader reader = repository.SupportTree ? new TreeEntityListReader() : new EntityListReader() as ListReader;
            reader.Repository = repository;
            reader.ChangeSet = jEntityList;
            reader.ResultEntityList = resultEntityList;

            reader.Read();

            return reader.ResultEntityList;
        }

        internal EntityRepository Repository;

        internal JObject ChangeSet;

        internal IEntityList ResultEntityList;

        protected EntityPropertySetter _setter;

        internal void Read()
        {
            this._setter = new EntityPropertySetter(this.Repository.EntityMeta);

            if (this.ResultEntityList == null)
            {
                this.ResultEntityList = this.Repository.NewList();
            }

            this.ReadCore();
        }

        protected abstract void ReadCore();

        protected void ReadList(JObject changeSet, string jsonListName, IList list)
        {
            var p = changeSet.Property(jsonListName);
            if (p != null)
            {
                var jsonList = p.Value as JArray;
                foreach (JObject item in jsonList)
                {
                    var e = this.Repository.New();
                    e.PersistenceStatus = PersistenceStatus.Saved;

                    this._setter.SetEntity(e, item);

                    list.Add(e);
                }
            }
        }
    }
}