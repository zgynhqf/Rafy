/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141121
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141121 12:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.Web.Mvc
{
    /// <summary>
    /// MVC 中能直接识别 Rafy Entity 模型绑定器。
    /// MVC 中直接使用 Entity 作为视图模型时，为了能在更新数据时更好地结合框架，需要使用此绑定器来更新实体。
    /// </summary>
    public class EntityAwareBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            /*********************** 代码块解释 *********************************
             * 如果要绑定的模型是一个实体，而且已经提供了这个实体的Id，说明是在对旧实体的更新，
             * 这时需要把旧实体从数据库中获取出来再设置新的值。这样可以解决两个问题：
             * * 在更新时，不需要再主动设置实体的 PersistenceStatus 为 Modified。
             * * 在更新树节点时，不会因为在 TreePId 没有变化的情况下更新 TreePId，而造成 TreeIndex 出错的问题。
            **********************************************************************/
            if (modelType.IsSubclassOf(typeof(Entity)) &&
                !string.Equals(controllerContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                var idValue = bindingContext.ValueProvider.GetValue(Entity.IdProperty.Name);
                if (idValue != null)
                {
                    var id = (string)idValue.ConvertTo(typeof(string));
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var repository = RF.Find(modelType);
                        var entity = repository.GetById(id);
                        if (entity != null) { return entity; }
                    }
                }
            }

            return base.CreateModel(controllerContext, bindingContext, modelType);
        }
    }
}