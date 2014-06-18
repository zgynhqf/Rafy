using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command.UI;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace JXC.Commands
{
    [Command(Label = "添加附件", GroupType = CommandGroupType.Edit)]
    public class AddFileAttachement : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            var dg = new OpenFileDialog();
            if (dg.ShowDialog() == true)
            {
                var fileName = dg.FileName;
                var attachement = view.AddNew(true) as FileAttachement;
                attachement.FileName = Path.GetFileName(fileName);
                attachement.UploadDate = DateTime.Today;
                attachement.ContentBytes = File.ReadAllBytes(fileName);
            }
        }
    }

    [Command(Label = "打开附件", GroupType = CommandGroupType.View)]
    public class OpenFileAttachement : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Current != null;
        }

        public override void Execute(ListLogicalView view)
        {
            var fa = view.Current as FileAttachement;
            var tmpFile = Path.Combine(Path.GetTempPath(), fa.FileName);
            File.WriteAllBytes(tmpFile, fa.ContentBytes);
            Process.Start(tmpFile);
        }
    }
}