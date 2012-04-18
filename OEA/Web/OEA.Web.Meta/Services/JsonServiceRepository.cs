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

namespace OEA.Web.Services
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

            //参数输入
            var jInput = JObject.Parse(jsonInput);
            var properties = service.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.HasMarked<ServiceInputAttribute>())
                {
                    JToken jValue = null;
                    if (jInput.TryGetValue(property.Name, out jValue))
                    {
                        var value = (jValue as JValue).Value;
                        property.SetValue(service, value, null);
                    }
                }
            }

            service.Invoke();

            //结果输出
            var res = new DynamicJsonModel();
            foreach (var property in properties)
            {
                if (property.HasMarked<ServiceOutputAttribute>())
                {
                    var value = property.GetValue(service, null);
                    res.SetProperty(property.Name, value);
                }
            }

            return res.ToJsonString();
        }

        public static void LoadAllServices()
        {
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

            var name = attri != null ? attri.Name : serviceType.Name;

            _services[name] = serviceType;
        }
    }
}