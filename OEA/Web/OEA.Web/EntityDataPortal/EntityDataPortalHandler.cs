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
using System.Linq;
using System.Transactions;
using System.Web;
using hxy;
using hxy.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.Utils;
using OEA.Web.ClientMetaModel;
using OEA.Web.EntityDataPortal;
using OEA.Web.Json;
using OEA.MetaModel.View;

namespace OEA.Web
{
    /// <summary>
    /// 实体数据门户：CRUD
    /// </summary>
    public class EntityDataPortalHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            var request = context.Request;

            var strType = request.QueryString["type"];
            if (string.IsNullOrEmpty(strType)) return string.Empty;

            var meta = ClientEntities.Find(strType);

            JsonModel res = null;
            if (context.Request.HttpMethod == "GET")
            {
                var evm = UIModel.Views.CreateBaseView(meta.EntityType);
                res = this.QueryEntityList(request, evm);
            }
            else
            {
                res = this.SaveEntityList(request, meta);
            }

            var js = LiteJsonWriter.Convert(res);

            return Compress(js);
        }

        private JsonModel SaveEntityList(HttpRequest request, EntityMeta meta)
        {
            var clientResult = new ClientResult();

            var entityListJson = request.Form["entityList"];
            if (!string.IsNullOrWhiteSpace(entityListJson))
            {
                var repo = RF.Create(meta.EntityType);

                JObject jEntityList = JObject.Parse(entityListJson);

                var list = EntityJsonConverter.JsonToEntityList(jEntityList, repo);

                if (OEAEnvironment.IsDebuggingEnabled)
                {
                    SaveList(repo, list);

                    clientResult.success = true;
                }
                else
                {
                    try
                    {
                        SaveList(repo, list);

                        clientResult.success = true;
                    }
                    catch (Exception ex)
                    {
                        clientResult.msg = ex.Message
                            .Replace("\"", string.Empty)
                            .Replace("'", string.Empty)
                            .Replace(Environment.NewLine, string.Empty);
                    }
                }
            }

            return clientResult;
        }

        private static void SaveList(EntityRepository repo, EntityList list)
        {
            using (var tran = new SingleConnectionTrasactionScope(list.GetRepository().DbSetting))
            {
                repo.Save(list);

                tran.Complete();
            }
        }

        private JsonModel QueryEntityList(HttpRequest request, EntityViewMeta evm)
        {
            var repo = RF.Create(evm.EntityType);

            EntityList entities = QueryEntityListCore(request, repo);

            var list = new EntityJsonList();

            if (repo.SupportTree)
            {
                var roots = entities.FindRoots();
                foreach (var rootItem in roots)
                {
                    var i = ConvertRecur(rootItem, evm);
                    i.expanded = true;
                    list.entities.Add(i);
                }

                list.total = entities.Count;
            }
            else
            {
                var page = request.GetQueryStringOrDefault("page", 1);
                var limit = request.GetQueryStringOrDefault("limit", 10);
                var pagerInfo = new PagerInfo(page, limit, true);

                entities = JumpToPage(repo, entities, pagerInfo);

                EntityJsonConverter.EntityToJson(evm, entities, list.entities);

                list.total = pagerInfo.TotalCount;
            }

            return list;





            //var repo = RF.Create(evm.EntityType);

            //EntityList entities = QueryEntityListCore(request, repo);

            //if (repo.SupportTree)
            //{
            //    entities.EnsureObjectRelations();

            //    var root = new RootTreeEntityJson();

            //    var roots = entities.FindRoots();
            //    foreach (var rootItem in roots)
            //    {
            //        var i = ConvertRecur(rootItem, evm);
            //        root.children.Add(i);
            //    }

            //    return root;
            //}
            //else
            //{
            //    var page = request.GetQueryStringOrDefault("page", 1);
            //    var limit = request.GetQueryStringOrDefault("limit", 10);
            //    var pagerInfo = new PagerInfo(page, limit, true);
            //    entities = JumpToPage(repo, entities, pagerInfo);

            //    var list = new EntityJsonList
            //    {
            //        totalCount = pagerInfo.TotalCount,
            //    };

            //    EntityJsonConverter.EntityToJson(evm, entities, list.entities);

            //    return list;
            //}
        }

        private static TreeEntityJson ConvertRecur(Entity entity, EntityViewMeta evm)
        {
            var jEntity = new TreeEntityJson();

            EntityJsonConverter.EntityToJson(evm, entity, jEntity);

            foreach (Entity child in entity.TreeChildren)
            {
                var c = ConvertRecur(child, evm);
                jEntity.children.Add(c);
            }

            return jEntity;
        }

        private static EntityList QueryEntityListCore(HttpRequest request, EntityRepository repo)
        {
            EntityList entities = null;

            var filter = request.GetQueryStringOrDefault("filter", string.Empty);
            if (!string.IsNullOrEmpty(filter))
            {
                var propertyJObjects = JArray.Parse(filter);
                //GetByParentId
                if (propertyJObjects.Count == 1)
                {
                    JObject item = propertyJObjects[0] as JObject;
                    var property = item.Property("property").Value.ToString();
                    if (!property.Contains("Id")) throw new NotSupportedException();

                    var value = item.Property("value").Value.ToString();
                    var parentId = int.Parse(value);

                    //查询数据库
                    entities = repo.GetByParentId(parentId);
                }
                //GetByCriteria
                else
                {
                    JObject jUseCriteriaType = propertyJObjects[0] as JObject;
                    if (!jUseCriteriaType.Property("property").Value.ToString().Contains("_useCriteriaType")) throw new NotSupportedException();
                    var clientTypeName = jUseCriteriaType.Property("value").Value.ToString();
                    var criteriaType = ClientEntities.Find(clientTypeName);
                    if (criteriaType == null) throw new NotSupportedException("criteriaType");

                    var jCriteria = propertyJObjects[1] as JObject;
                    if (!jCriteria.Property("property").Value.ToString().Contains("criteria")) throw new NotSupportedException();
                    var jCriteriaValues = jCriteria.Property("value").Value as JObject;

                    //创建一个 Criteria 并设置它的相关属性值
                    var cRepo = RF.Create(criteriaType.EntityType);
                    var criteria = cRepo.New();
                    var setter = new EntityPropertySetter(cRepo);
                    setter.SetEntity(criteria, jCriteriaValues);

                    //查询数据库
                    entities = (repo as IOEARepositoryInternal).GetListImplicitly(criteria);
                }
            }
            else
            {
                entities = repo.GetAll();
            }

            return entities;
        }

        private static EntityList JumpToPage(EntityRepository repo, EntityList raw, PagerInfo pageInfo)
        {
            if (raw.Count <= pageInfo.PageSize)
            {
                if (pageInfo.IsNeedCount) pageInfo.TotalCount = raw.Count;

                return raw;
            }

            var list = repo.NewList();

            var pagedList = raw.JumpToPage(pageInfo);

            foreach (var entity in pagedList)
            {
                list.Add(entity);
            }
            list.MarkOld();

            return list;
        }
    }
}