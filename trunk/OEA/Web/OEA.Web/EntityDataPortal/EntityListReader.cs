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
    /// 一般实体列表的读取器
    /// </summary>
    internal class EntityListReader : ListReader
    {
        protected List<Entity> _delete = new List<Entity>();

        protected override void ReadCore()
        {
            this.ResultEntityList = this.Repository.CreateEmptyOldList();

            this.ReadCreateList(this.ChangeSet, this.ResultEntityList);
            this.ReadList(this.ChangeSet, "toUpdate", this.ResultEntityList);
            this.ReadList(this.ChangeSet, "unchanged", this.ResultEntityList);
            this.ReadList(this.ChangeSet, "toDelete", this._delete);

            this.ResultEntityList.AddRange(this._delete);
            //加入到 Remove 列表中，这样就可以在保存时删除了。
            foreach (var item in this._delete)
            {
                this.ResultEntityList.Remove(item);
            }
        }

        protected void ReadCreateList(JObject changeSet, IList<Entity> list)
        {
            var p = changeSet.Property("toCreate");
            if (p != null)
            {
                foreach (JObject item in p.Value)
                {
                    var e = this.Repository.New();
                    this._setter.SetEntity(e, item);
                    list.Add(e);
                }
            }
        }
    }
}