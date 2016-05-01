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
using System.Reflection;
using System.Transactions;
using System.Web;
using Rafy;
using Rafy.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Utils;
using Rafy.Web.ClientMetaModel;
using Rafy.Web.EntityDataPortal;
using Rafy.Web.Json;

namespace Rafy.Web
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

            var js = res.ToJsonString();

            return Compress(js);
        }

        private JsonModel SaveEntityList(HttpRequest request, EntityMeta meta)
        {
            var clientResult = new ClientResult();

            var entityListJson = request.Form["entityList"];
            if (!string.IsNullOrWhiteSpace(entityListJson))
            {
                var repo = RF.Find(meta.EntityType);

                JObject jEntityList = JObject.Parse(entityListJson);

                var list = EntityJsonConverter.JsonToEntityList(jEntityList, repo);

                if (RafyEnvironment.IsDebuggingEnabled)
                {
                    SaveList(repo, list);

                    clientResult.Success = true;
                }
                else
                {
                    try
                    {
                        SaveList(repo, list);

                        clientResult.Success = true;
                    }
                    catch (Exception ex)
                    {
                        clientResult.Message = ex.Message
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
            using (var tran = RF.TransactionScope(repo))
            {
                repo.Save(list);

                tran.Complete();
            }
        }

        private JsonModel QueryEntityList(HttpRequest request, EntityViewMeta evm)
        {
            var repo = RF.Find(evm.EntityType);

            var pagingInfo = ParsePagingInfo(request, repo);

            var entities = QueryEntityListCore(request, repo, pagingInfo);

            var list = new EntityJsonList { model = evm.EntityType };

            if (repo.SupportTree)
            {
                var roots = entities;
                foreach (var rootItem in roots)
                {
                    var i = ConvertRecur(rootItem, evm);
                    i.expanded = true;
                    list.entities.Add(i);
                }

                list.total = (entities as ITreeComponent).CountNodes();
            }
            else
            {
                //如果此时，还需要进行统计，表示在数据层查询时，并没有对分页进行处理。这时，只能在内存中对实体进行分页。
                if (pagingInfo.IsNeedCount)
                {
                    entities = JumpToPageInMemory(repo, entities, pagingInfo);
                }

                EntityJsonConverter.EntityToJson(evm, entities, list.entities);

                list.total = pagingInfo.TotalCount;
            }

            return list;

            #region //暂时不用

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

            #endregion
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

        private static EntityList QueryEntityListCore(HttpRequest request, EntityRepository repo, PagingInfo pagingInfo)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 支持三种方法来查询数据库：
             * * 直接使用 Id 来查询
             * * 指定方法名来查询
             * * 指定查询条件进行查询。
             * 
            **********************************************************************/

            EntityList entities = null;

            var filter = request.GetQueryStringOrDefault("filter", string.Empty);
            if (!string.IsNullOrEmpty(filter))
            {
                var filters = JArray.Parse(filter);

                //GetByParentId（客户端所有的查询都应该通过 GetByCriteria，只有一个过滤条件时，必然是 GetByParentId）
                if (filters.Count == 1)
                {
                    var parentId = ParseParentId(filters);

                    //查询数据库
                    entities = repo.GetByParentId(parentId, pagingInfo);
                }
                else
                {
                    var type = filters[0] as JObject;
                    var useType = type.Property("property").Value.ToString();
                    switch (useType)
                    {
                        case "_useMethod":
                            //使用 指定的方法 查询数据库
                            var method = type.Property("value").Value.ToString();
                            var parameters = ParseParameters(filters);
                            entities = QueryByMethod(repo, method, parameters, pagingInfo);
                            break;
                        case "_useCriteria":
                            //使用 Criteria 查询数据库
                            var criteria = ParseCriteria(filters, pagingInfo);

                            entities = MethodCaller.CallMethod(repo, EntityConvention.GetByCriteriaMethod, criteria) as EntityList;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                entities = repo.GetAll(pagingInfo);
            }

            return entities;
        }

        private static PagingInfo ParsePagingInfo(HttpRequest request, EntityRepository repo)
        {
            //如果不是树实体时，则需要进行分页处理。
            var pagingInfo = PagingInfo.Empty;
            if (!repo.SupportTree)
            {
                var page = request.GetQueryStringOrDefault("page", 1);
                var limit = request.GetQueryStringOrDefault("limit", 10);
                pagingInfo = new PagingInfo(page, limit, true);
            }
            return pagingInfo;
        }

        #region 指定 Id 的查询

        private static int ParseParentId(JArray propertyJObjects)
        {
            JObject item = propertyJObjects[0] as JObject;
            var property = item.Property("property").Value.ToString();

            //20130810 GetByParentId 传回来的值不带 Id 了，原因不明。（Ext 4.1 时就存在，与 4.2 版本无关。）
            if (!property.Contains("Id")) throw new NotSupportedException();

            var value = item.Property("value").Value.ToString();
            var parentId = int.Parse(value);
            return parentId;
        }

        #endregion

        #region 指定查询条件的查询

        private static Criteria ParseCriteria(JArray filters, PagingInfo pagingInfo)
        {
            JObject jUseCriteriaType = filters[0] as JObject;
            var clientTypeName = jUseCriteriaType.Property("value").Value.ToString();
            var criteriaMeta = ClientEntities.Find(clientTypeName);
            if (criteriaMeta == null) throw new NotSupportedException("criteriaType");

            var jCriteria = filters[1] as JObject;
            if (!jCriteria.Property("property").Value.ToString().Contains("_criteria")) throw new NotSupportedException();
            var jCriteriaValues = jCriteria.Property("value").Value as JObject;

            //创建一个 Criteria 并设置它的相关属性值
            var criteria = Entity.New(criteriaMeta.EntityType) as Criteria;
            if (criteria == null) throw new InvalidProgramException("在 Web 开发模式下，查询条件类，必须继承自 Criteria 类型。");

            var setter = new EntityPropertySetter(criteriaMeta);
            setter.SetEntity(criteria, jCriteriaValues);

            criteria.PagingInfo = pagingInfo;

            return criteria;
        }

        #endregion

        #region 指定方法名查询

        private static object[] ParseParameters(JArray filters)
        {
            var pObject = filters[1] as JObject;
            if (!pObject.Property("property").Value.ToString().Contains("_params")) throw new NotSupportedException();
            var pValues = pObject.Property("value").Value as JArray;
            var res = pValues.Select(t => (t as JValue).Value).ToArray();
            return res;
        }

        private static EntityList QueryByMethod(EntityRepository repo, string methodName, object[] parameters, PagingInfo pagingInfo)
        {
            //找到对应的查询方法
            MethodInfo queryMethod = null;
            var methods = repo.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod);
            foreach (var method in methods)
            {
                if (method.Name.EqualsIgnoreCase(methodName))
                {
                    if (queryMethod != null)
                    {
                        throw new InvalidProgramException(string.Format("Web 客户端调用的查询方法名称必须唯一：仓库 {0} 中存在多个名为 ‘{1}’ 的方法。", repo.GetType(), methodName));
                    }
                    queryMethod = method;
                }
            }

            //如果查询方法的参数比 parameters 的长度大一，那么再加上分页参数再尝试。
            var methodParameters = queryMethod.GetParameters();
            if (methodParameters.Length == parameters.Length + 1)
            {
                var newList = parameters.ToList();
                newList.Add(pagingInfo);
                parameters = newList.ToArray();
            }

            //对参数长度做限定。
            if (methodParameters.Length != parameters.Length) throw new InvalidProgramException(string.Format("Web 客户端调用 {0}.{1} 方法时，提供的参数个数不符。", repo.GetType(), methodName));

            //对于每一个参数，尝试类型转换。
            ConvertParametersType(methodParameters, parameters);

            var res = queryMethod.Invoke(repo, parameters);
            if (!(res is EntityList)) throw new InvalidProgramException(string.Format("Web 客户端调用的 {0}.{1} 方法必须返回实体的列表类型。", repo.GetType(), methodName));

            return res as EntityList;
        }

        private static void ConvertParametersType(ParameterInfo[] methodParameters, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var mp = methodParameters[i];
                var p = parameters[i];

                var mpType = mp.ParameterType;

                //任意参数都不能为 null
                if (p == null)
                {
                    if (mpType == typeof(string))
                    {
                        p = string.Empty;
                    }
                    else
                    {
                        p = TypeHelper.GetDefaultValue(mpType);
                    }

                    parameters[i] = p;
                }

                //如果需要的参数是一个类型，而提供的参数是一个字符串，则通过字符串查找并转换为对应的实体类型。
                if (mpType == typeof(Type) && p is string)
                {
                    var em = ClientEntities.Find(p as string);
                    if (em != null)
                    {
                        parameters[i] = em.EntityType;
                    }
                }
            }
        }

        #endregion

        private static EntityList JumpToPageInMemory(EntityRepository repo, EntityList raw, PagingInfo pageInfo)
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
            list.MarkSaved();

            return list;
        }
    }
}