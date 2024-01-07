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
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.Reflection;
using Rafy.Web.ClientMetaModel;
using Rafy.Web.EntityDataPortal;
using Rafy.Web.Json;

namespace Rafy.Web
{
    /// <summary>
    /// 所有 JsonService 的管理类
    /// </summary>
    internal class JsonServiceRepository
    {
        private static Dictionary<string, Type> _services = new Dictionary<string, Type>();

        internal static Type Find(string clientName)
        {
            Type res = null;
            _services.TryGetValue(clientName, out res);
            return res;
        }

        /// <summary>
        /// Invokes the specified service name.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="jsonInput">The json input.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static string Invoke(string serviceName, string jsonInput)
        {
            var serviceType = Find(serviceName);
            if (serviceType == null) throw new InvalidOperationException();

            var service = ServiceFactory.Create(serviceType);
            var properties = serviceType.GetProperties();

            SetInputProperties(service, jsonInput, properties);

            var res = new DynamicJsonModel();
            try
            {
                //调用服务
                service.Invoke();
            }
            catch (Exception ex)
            {
                res.SetProperty(ClientResult.SuccessProperty, false);
                res.SetProperty(ClientResult.MessageProperty, ex.Message);
                return res.ToJsonString();
            }

            //结果输出
            ReadOutputProperties(service, properties, res);

            return res.ToJsonString();
        }

        /// <summary>
        /// 参数输入
        /// </summary>
        /// <param name="service"></param>
        /// <param name="jsonInput"></param>
        /// <param name="properties"></param>
        private static void SetInputProperties(IService service, string jsonInput, System.Reflection.PropertyInfo[] properties)
        {
            var jInput = JObject.Parse(jsonInput);
            foreach (var property in properties)
            {
                if (ServiceHelper.IsInput(property))
                {
                    JToken jToken = null;
                    if (jInput.TryGetValue(property.Name, out jToken))
                    {
                        //如果是一般属性，则直接赋值。
                        var jValue = jToken as JValue;
                        if (jValue != null)
                        {
                            var value = TypeHelper.CoerceValue(property.PropertyType, jValue.Value);
                            property.SetValue(service, value, null);
                        }
                        else
                        {
                            //如果不是一般的属性，则表示这是一个引用属性，
                            //目前，引用属性只支持实体及实体列表。
                            var jEntityList = jToken as JObject;
                            var list = EntityJsonConverter.JsonToEntityList(jEntityList);

                            //如果有这个属性，表明这只是一个用实体列表包装起来的单个实体。
                            var isEntity = jEntityList.Property(Consts.isEntityProperty) != null;
                            if (isEntity)
                            {
                                if (list.Count > 0)
                                {
                                    var entity = list[0];
                                    property.SetValue(service, entity, null);
                                }
                            }
                            else
                            {
                                property.SetValue(service, list, null);
                            }
                        }
                    }
                }
            }
        }

        private static readonly PropertyInfo FlowServiceResultName = typeof(FlowService).GetProperty("Result");

        /// <summary>
        /// 结果输出
        /// </summary>
        /// <param name="service"></param>
        /// <param name="properties"></param>
        /// <param name="res"></param>
        private static void ReadOutputProperties(IService service, System.Reflection.PropertyInfo[] properties, DynamicJsonModel res)
        {
            foreach (var property in properties)
            {
                if (ServiceHelper.IsOutput(property))
                {
                    var value = property.GetValue(service, null);

                    //如果是 FlowService.Result ，则使用小写直接把 Result 输出。
                    //这里的判断不能直接使用 property == FlowServiceResultName，这是因为它们是两个不同的运行时对象。
                    if (property.DeclaringType == FlowServiceResultName.DeclaringType && property.Name == FlowServiceResultName.Name)
                    {
                        var result = (Result)value;
                        res.SetProperty(ClientResult.SuccessProperty, BooleanBoxes.Box(result.Success));
                        res.SetProperty(ClientResult.MessageProperty, result.Message);
                        continue;
                    }

                    //进行格式转换。
                    value = ConvertOutputComponent(value);

                    res.SetProperty(property.Name, value);
                }
            }
        }

        private static object ConvertOutputComponent(object value)
        {
            //服务的输出属性如果类型是一个实体或者实体列表，则需要进行格式转换。
            if (value is IDomainComponent)
            {
                var model = (value as IDomainComponent).GetRepository().EntityType;

                //TODO：这里可能存在问题：当一个非默认的视图请求这个服务得到一个默认视图的实体数据时，可能会因为列不一致而出现问题。
                var defaultVM = UIModel.Views.CreateBaseView(model);

                if (value is IEntityList)
                {
                    var listRes = new EntityJsonList { model = model };

                    EntityJsonConverter.EntityToJson(defaultVM, value as IEntityList, listRes.entities);
                    listRes.total = listRes.entities.Count;

                    value = listRes;
                }
                else if (value is Entity)
                {
                    var entityJson = new EntityJson();
                    EntityJsonConverter.EntityToJson(defaultVM, value as Entity, entityJson);

                    //在纯数据的基础上添加以下两个约定的属性：标记这是一个实体以及它在客户端的类型名称。
                    entityJson.SetProperty(Consts.isEntityProperty, BooleanBoxes.True);
                    entityJson.SetProperty(Consts.modelProperty, ClientEntities.GetClientName(model));

                    value = entityJson;
                }
                else
                {
                    throw new NotSupportedException("只支持对实体、实体列表进行格式转换。");
                }
            }

            return value;
        }

        #region 加载所有可用的类型

        internal static void LoadAllServices()
        {
            //这个类型没有定义在插件程序集中，所以这里需要手动加入到仓库中。
            Add(typeof(GetCustomDataSourceService));

            foreach (var plugin in RafyEnvironment.Plugins)
            {
                foreach (var type in plugin.Assembly.GetTypes())
                {
                    if (!type.IsGenericType && !type.IsAbstract &&
                        typeof(IService).IsAssignableFrom(type) &&
                        type.HasMarked<JsonServiceAttribute>())
                    {
                        Add(type);
                    }
                }
            }
        }

        internal static void Clear()
        {
            _services.Clear();
        }

        private static void Add(Type serviceType)
        {
            var attri = serviceType.GetSingleAttribute<JsonServiceAttribute>();

            var name = attri.ClientName;
            if (string.IsNullOrWhiteSpace(name)) name = serviceType.FullName;

            _services[name] = serviceType;
        }

        #endregion
    }
}