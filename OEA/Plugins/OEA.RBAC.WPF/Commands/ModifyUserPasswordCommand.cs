using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

using OEA.MetaModel.Attributes;
using OEA.MetaModel;
using OEA.MetaModel.View;

using OEA.WPF.Command;
using OEA;
using OEA.Module.WPF;
using OEA.RBAC;

namespace RBAC
{
    /// <summary>
    /// 修改用户密码
    /// </summary>
    [Command(Label = "设置密码")]
    public class ModifyUserPasswordCommand : ViewCommand
    {
        public override bool CanExecute(ObjectView view)
        {
            return (null != view.Current);
        }
        public override void Execute(ObjectView view)
        {
            ChangePwd.Execute(view.Current as User);
        }
    }
}
