/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.XmlConfig
{
    /// <summary>
    /// UI 块配置文件的主键
    /// </summary>
    public class BlockConfigKey
    {
        /// <summary>
        /// 对应的实体类型
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 如果是扩展视图，则这个属性是扩展视图的名称
        /// </summary>
        public string ExtendView { get; set; }

        /// <summary>
        /// 判断当前配置是否为默认视图
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultView()
        {
            return string.IsNullOrEmpty(this.ExtendView);
        }

        /// <summary>
        /// 友好描述信息
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return this.EntityType.FullName + " " + (this.ExtendView ?? "DefaultView");
        }

        /// <summary>
        /// 对应的 XML 文件路径地址。
        /// </summary>
        /// <returns></returns>
        public string[] GetFilePathes(BranchDestination destination)
        {
            return XmlConfigFileSystem.GetBlockConfigFilePath(this, destination);
        }

        /// <summary>
        /// 获取激活的分支的 XML 文件路径地址。
        /// </summary>
        /// <returns></returns>
        public string GetActiveBranchFilePath()
        {
            return this.GetFilePathes(BranchDestination.ActiveBranch).Last();
        }
    }
}