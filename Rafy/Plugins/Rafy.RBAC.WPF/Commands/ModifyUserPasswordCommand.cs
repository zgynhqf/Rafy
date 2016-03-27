using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.RBAC.Old.WPF
{
    /// <summary>
    /// 修改用户密码
    /// </summary>
    [Command(Label = "设置密码")]
    public class ModifyUserPasswordCommand : ViewCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            return (null != view.Current);
        }
        public override void Execute(LogicalView view)
        {
            ChangePwd.Execute(view.Current as User);
        }
    }
}
