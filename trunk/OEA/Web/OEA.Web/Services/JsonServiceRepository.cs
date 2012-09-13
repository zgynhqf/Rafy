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
using OEA;
using Newtonsoft.Json.Linq;
using OEA.Web.Json;
using OEA.Web.EntityDataPortal;
using OEA.Library;
using OEA.Reflection;
using OEA.MetaModel.View;

namespace OEA.Web
{
    /// <summary>
    /// 所有 JsonService 的管理类
    /// </summary>
    public class JsonServiceRepository
    {
        private static Dictionary<string, Type> _services = new Dictionary<string, Type>();

        internal static Type Find(string clientName)
        {
            Type res = null;
            _services.TryGetValue(clientName, out res);
            return res;
        }

        public static string Invoke(string serviceName, string jsonInput)
        {
            var serviceType = Find(serviceName);
            if (serviceType == null) throw new InvalidOperationException();

            var service = Activator.CreateInstance(serviceType) as IService;
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
                res.SetProperty("success", false);
                res.SetProperty("msg", ex.Message);
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
                if (property.HasMarked<ServiceInputAttribute>())
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
                            var modelProperty = jEntityList.Property("_model");
                            if (modelProperty == null) { throw new NotSupportedException("目前，服务输入中的引用属性只支持实体及实体列表。"); }
                            var list = EntityJsonConverter.JsonToEntityList(jEntityList);

                            //如果有这个属性，表明这只是一个用实体列表包装起来的单个实体。
                            var isEntity = jEntityList.Property("_isEntityHost") != null;
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
                var output = property.GetSingleAttribute<ServiceOutputAttribute>();
                if (output != null && output.OutputToWeb)
                {
                    var value = property.GetValue(service, null);

                    if (value is EntityList)
                    {
                        var list = value as EntityList;

                        //TODO：这里可能存在问题：当一个非默认的视图请求这个服务得到一个默认视图的实体数据时，可能会因为列不一致而出现问题。
                        var defaultVM = UIModel.Views.CreateBaseView(list.EntityType);

                        var listRes = new EntityJsonList();
                        EntityJsonConverter.EntityToJson(defaultVM, list, listRes.entities);
                        listRes.total = listRes.entities.Count;
                        value = listRes;
                    }
                    else if (value is Entity)
                    {
                        throw new NotImplementedException();//huqf
                    }

                    res.SetProperty(property.Name, value);
                }
            }
        }

        #region 加载所有可用的类型

        internal static void LoadAllServices()
        {
            Add(typeof(GetCustomDataSourceService));

            foreach (var lib in OEAEnvironment.GetAllLibraries()) { AddByAssembly(lib.Assembly); }
        }

        private static void AddByAssembly(System.Reflection.Assembly assembly)
        {
            var serviceTypes = assembly.GetTypes()
                    .Where(t => !t.IsGenericType && !t.IsAbstract && typeof(IService).IsAssignableFrom(t))
                    .ToArray();

            foreach (var serviceType in serviceTypes) { Add(serviceType); }
        }

        private static void Add(Type serviceType)
        {
            var attri = serviceType.GetSingleAttribute<ClientServiceNameAttribute>();

            var name = attri != null ? attri.Name : serviceType.FullName;

            _services[name] = serviceType;
        }

        #endregion
    }
}