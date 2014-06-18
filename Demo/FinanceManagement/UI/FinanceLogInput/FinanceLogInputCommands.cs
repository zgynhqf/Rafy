using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Threading;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Command.UI;
using Rafy.WPF.Controls;

namespace FM.Commands
{
    [Command(Label = "添加", ToolTip = "Ctrl+回车 直接添加", GroupType = CommandGroupType.Business)]
    public class ContinueAddFinanceLog : ClientCommand<DetailLogicalView>
    {
        public override void Execute(DetailLogicalView view)
        {
            var log = view.Current.CastTo<FinanceLog>();

            var brokenRules = log.Validate();
            if (brokenRules.Count > 0)
            {
                App.MessageBox.Show(brokenRules.ToString(), "添加出错");
                return;
            }

            RF.Save(log);

            var listView = view.Relations["list"];
            listView.DataLoader.ReloadDataAsync();

            App.MessageBox.Show("添加成功。");

            log.MarkNew();
            log.Amount = 0;

            //定位焦点到数量上
            var element = view.FindPropertyEditor(FinanceLog.AmountProperty).Control as FrameworkElement;
            element.Focus();

            SyncBasicData(log);
        }

        /// <summary>
        /// 在异步队列中，把手动输入的 Tag、Person 添加到数据库中。
        /// </summary>
        /// <param name="log"></param>
        private static void SyncBasicData(FinanceLog log)
        {
            var userArray = log.Users.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var tagArray = log.Tags.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            AsyncHelper.InvokeSafe(() =>
            {
                var userRepo = RF.Concrete<PersonRepository>();
                var users = userRepo.CacheAll().Concrete();
                foreach (var userName in userArray)
                {
                    var user = users.FirstOrDefault(u => u.Name == userName);
                    if (user == null)
                    {
                        user = new Person
                        {
                            Name = userName
                        };
                        userRepo.Save(user);
                    }
                }

                var tagRepo = RF.Concrete<TagRepository>();
                var tags = tagRepo.CacheAll().Concrete();
                foreach (var tagName in tagArray)
                {
                    var tag = tags.FirstOrDefault(u => u.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName
                        };
                        userRepo.Save(tag);
                    }
                }
            });
        }
    }
}