using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace JXC.Commands
{
    [Command(Label = "添加附件", GroupType = CommandGroupType.Edit)]
    public class AddFileAttachement : ListViewCommand
    {
        public override void Execute(ListObjectView view)
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
        public override bool CanExecute(ListObjectView view)
        {
            return view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            var fa = view.Current as FileAttachement;
            var tmpFile = Path.Combine(Path.GetTempPath(), fa.FileName);
            File.WriteAllBytes(tmpFile, fa.ContentBytes);
            Process.Start(tmpFile);
        }
    }
}