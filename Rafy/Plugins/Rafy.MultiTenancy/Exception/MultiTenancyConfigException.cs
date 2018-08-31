/*******************************************************
 * 
 * 作者：王国超
 * 创建日期：20180522
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 王国超 20180522 16:53
 * 
*******************************************************/

namespace Rafy.MultiTenancy.Exception
{
    /// <summary>
    /// 多租户数据分片配置异常类
    /// </summary>
    public class MultiTenancyConfigException : System.Exception
    {
        public MultiTenancyConfigException() : base("读取多租户数据分片配置文件失败")
        {}

        public MultiTenancyConfigException(string message) : base(message)
        { }
    }
}
