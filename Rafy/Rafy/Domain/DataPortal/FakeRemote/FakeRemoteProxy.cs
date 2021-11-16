/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211115
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211115 12:24
 * 
*******************************************************/

using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /*********************** 代码块解释 *********************************
     * 
     * 由于开发人员平时会使用单机版本开发，而正式部署时，又会选用 C/S 架构。
     * 所以需要保证单机版本和 C/S 架构版本的模式是一样的。也就是说，在单机模式下，
     * 在通过门户访问时，模拟网络版，clone 出一个新的对象。
     * 这样，在底层 Update 更改 obj 时，不会影响上层的实体。
     * 而是以返回值的形式把这个被修改的实体返回给上层。
     * 
     * 20120828 
     * 但是，当在服务端本地调用时，不需要此模拟功能。
     * 这是因为在服务端本地调用时（例如服务端本地调用 RF.Save），
     * 在开发体验上，数据层和上层使用的实体应该是同一个，数据层的修改应该能够带回到上层，不需要克隆。
     * 
    **********************************************************************/
    public class FakeRemoteProxy : IDataPortalProxy
    {
        public DataPortalResult Call(object obj, string method, object[] parameters, DataPortalContext context)
        {
            obj = BinarySerializer.Clone(obj);

            if (parameters.Length > 0)
            {
                var copiedParameters = new object[parameters.Length];
                for (int i = 0, c = parameters.Length; i < c; i++)
                {
                    var item = parameters[i];
                    if (item != null && item.GetType().IsClass && !(item is string))
                    {
                        item = BinarySerializer.Clone(item);
                    }

                    copiedParameters[i] = item;
                }
                parameters = copiedParameters;
            }

            var result = FinalDataPortal.DoCall(obj, method, parameters);

            result = BinarySerializer.Clone(result);

            return result;
        }
    }
}
