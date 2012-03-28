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
using System.Linq;
using System.Text;
using OEA.Library;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace OEA.Web.EntityDataPortal
{
    /// <summary>
    /// Json 到 EntityList 的读取器
    /// </summary>
    internal abstract class ListReader
    {
        [DebuggerStepThrough]
        internal static EntityList JsonToEntity(JObject jEntityList, EntityRepository repository)
        {
            ListReader reader = repository.SupportTree ? new TreeEntityListReader() : new EntityListReader() as ListReader;
            reader.Repository = repository;
            reader.ChangeSet = jEntityList;

            reader.Read();

            return reader.ResultEntityList;
        }

        internal EntityRepository Repository;

        internal JObject ChangeSet;

        internal EntityList ResultEntityList;

        protected EntityPropertySetter _setter;

        internal void Read()
        {
            this._setter = new EntityPropertySetter(this.Repository);

            this.ReadCore();
        }

        protected abstract void ReadCore();

        protected void ReadList(JObject changeSet, string jsonListName, IList<Entity> list)
        {
            var p = changeSet.Property(jsonListName);
            if (p != null)
            {
                var jsonList = p.Value as JArray;
                foreach (JObject item in jsonList)
                {
                    var e = this.Repository.CreateEmptyOldEntity();

                    this._setter.SetEntity(e, item);

                    list.Add(e);
                }
            }
        }
    }
}