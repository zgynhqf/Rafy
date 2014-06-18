using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rafy.Domain;
using Rafy.Threading;

namespace Rafy.RBAC.WPF
{
    /// <summary>
    /// 统计登录记录的窗口
    /// </summary>
    public partial class LoginLogStatistic : UserControl
    {
        public LoginLogStatistic(IEnumerable<UserLoginLog> allLogs)
        {
            InitializeComponent();

            this.BindData(allLogs);
        }

        private void BindData(IEnumerable<UserLoginLog> allLogs)
        {
            //设置序号
            int index = 1;
            var viewModels = allLogs.GroupBy(l => l.UserId)
                .Select(group =>
                {
                    ViewModel result = new ViewModel();

                    var models = group.OrderBy(l => l.LogTime).ToArray();

                    TimeSpan allTime = new TimeSpan();
                    int loginCount = 0;

                    //统计所有的登录时间
                    DateTime? logInTime = null;
                    for (int i = 0, c = models.Length; i < c; i++)
                    {
                        var item = models[i];
                        if (item.IsIn)
                        {
                            logInTime = item.LogTime;
                            loginCount++;
                        }
                        else
                        {
                            if (logInTime != null)
                            {
                                allTime += item.LogTime - logInTime.Value;
                            }
                        }
                    }

                    result.UserId = models[0].UserId;
                    result.UserName = models[0].UserName;
                    result.LoginCount = loginCount;
                    result.TotalMinutes = Math.Round(allTime.TotalMinutes).ToString();
                    result.NO = index++;
                    return result;
                }).ToArray();

            lvStatistic.ItemsSource = viewModels;

            this.AsyncLoadPositions(viewModels);
        }

        private void AsyncLoadPositions(ViewModel[] viewModels)
        {
            AsyncHelper.InvokeSafe(() =>
            {
                foreach (var model in viewModels)
                {
                    var positions = (RF.Find<OrgPosition>() as OrgPositionRepository)
                        .GetList(model.UserId).Cast<OrgPosition>().ToList();
                    if (positions.Count > 0)
                    {
                        var positionResult = new StringBuilder();
                        for (int i = 0, c = positions.Count; i < c; i++)
                        {
                            var item = positions[i];
                            if (i != 0)
                            {
                                positionResult.Append('/');
                            }
                            positionResult.Append(item.View_Name);
                        }

                        model.Position = positionResult.ToString();
                    }
                }
            });
        }

        private class ViewModel : INotifyPropertyChanged
        {
            public int UserId { get; set; }

            public int NO { get; set; }
            public string UserName { get; set; }
            public int LoginCount { get; set; }
            public string TotalMinutes { get; set; }

            private string _Position;

            /// <summary>
            /// 这个属性是异步加载的，所以使用要触发PropertyChanged
            /// </summary>
            public string Position
            {
                get
                {
                    return this._Position;
                }
                set
                {
                    if (this._Position != value)
                    {
                        this._Position = value;

                        if (this.PropertyChanged != null)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs("Position"));
                        }
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}