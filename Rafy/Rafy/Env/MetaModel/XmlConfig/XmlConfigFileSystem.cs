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
    /// 此类为 XML 文件提供路径算法
    /// </summary>
    internal static class XmlConfigFileSystem
    {
        internal static bool IsCustomizing
        {
            get { return RafyEnvironment.BranchProvider.HasBranch; }
        }

        internal static string GetBlockConfigFilePath(BlockConfigKey key)
        {
            if (!IsCustomizing && key.Type == BlockConfigType.Customization) { throw new InvalidProgramException("当前不在客户化状态中，无法找到客户化文件：" + key.GetDescription()); }

            var sb = new StringBuilder();
            if (key.IsDefaultView())
            {
                sb.Append("MetaModel/Block/Default/");
                sb.Append(key.EntityType.FullName);
            }
            else
            {
                sb.Append("MetaModel/Block/Extend/");
                sb.Append(key.EntityType.FullName);
                sb.Append(" ");
                sb.Append(key.ExtendView);
            }

            sb.Append(".xml");

            var versionPath = sb.ToString();

            //暂时只支持一个主干版本和一个客户化版本：common, customer
            var pathes = RafyEnvironment.BranchProvider.MapAllPathes(versionPath, true);

            if (key.Type == BlockConfigType.Customization) { return pathes[1]; }

            return pathes[0];
        }

        internal static string GetCompositeBlocksFilePath(string definedViewName)
        {
            var versionPath = "MetaModel/AggtBlocks/" + definedViewName + ".xml";

            //暂时只支持一个主干版本和一个客户化版本：common, customer
            var pathes = RafyEnvironment.BranchProvider.MapAllPathes(versionPath, true);

            return pathes.Last();
        }
    }
}
