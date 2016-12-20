/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 15:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Accounts;

namespace Rafy.RBAC.RoleManagement
{
    public interface IUserRoleFinder
    {
        RoleList FindByUser(User user);
    }
}