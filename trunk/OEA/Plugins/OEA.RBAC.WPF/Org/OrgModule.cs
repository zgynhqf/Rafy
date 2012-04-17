/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF;
using OEA.MetaModel.View;
using OEA.RBAC;

namespace RBAC
{
    /// <summary>
    /// 组织权限模块
    /// </summary>
    public class OrgModule : CustomModule
    {
        protected override AggtBlocks DefineBlocks()
        {
            return new AggtBlocks
            {
                MainBlock = new Block(typeof(Org)),
                Layout = new LayoutMeta()
                {
                    IsLayoutChildrenHorizonal = true,
                    ParentChildProportion = new ParentChildProportion(20, 80)
                },
                Children = 
                {
                    new AggtBlocks
                    {
                        MainBlock = new ChildBlock("岗位", Org.OrgPositionListProperty),
                        Children = 
                        {
                            new ChildBlock("岗位成员", OrgPosition.OrgPositionUserListProperty),
                            new ChildBlock("岗位功能权限", OrgPosition.OrgPositionOperationDenyListProperty)
                            {
                                CustomViewType = typeof(OperationSelectionView).AssemblyQualifiedName
                            },
                        }
                    }
                }
            };
        }
    }
}
