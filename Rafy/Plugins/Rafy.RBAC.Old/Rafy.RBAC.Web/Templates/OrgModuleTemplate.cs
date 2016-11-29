/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120427
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 组织权限模块
    /// </summary>
    public class OrgModuleTemplate : BlocksTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            return new AggtBlocks
            {
                MainBlock = new Block(typeof(Org)),
                Layout = new LayoutMeta("Rafy.rbac.org.OrgModuleLayout"),
                Children = 
                {
                    new AggtBlocks
                    {
                        MainBlock = new ChildBlock("岗位", Org.OrgPositionListProperty),
                        Children = 
                        {
                            new ChildBlock("岗位成员", OrgPosition.OrgPositionUserListProperty),
                            //new ChildBlock("岗位功能权限", OrgPosition.OrgPositionOperationDenyListProperty),
                        }
                    }
                }
            };
        }
    }
}