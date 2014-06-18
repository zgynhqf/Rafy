/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121106 20:28
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121106 20:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace MP
{
    public static class TimeHelper
    {
        /// <summary>
        /// 获取现在的周的索引
        /// </summary>
        public static int CurrentWeekIndex
        {
            get { return CalcCurrentWeekIndex(); }
        }

        /// <summary>
        /// 获取某月共有多少周。
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static int CountWeeksInMonth(DateTime month)
        {
            var days = DateTime.DaysInMonth(month.Year, month.Month);
            return CalcWeekIndex(month, days) + 1;
        }

        /// <summary>
        /// 获取现在的周的索引
        /// </summary>
        /// <returns></returns>
        private static int CalcCurrentWeekIndex()
        {
            var today = DateTime.Today;
            return CalcWeekIndex(today, today.Day);
        }

        public static int CalcWeekIndex(DateTime month, int days)
        {
            var monthFirstDay = new DateTime(month.Year, month.Month, 1);

            var dayOrWeek = DayOrWeek(monthFirstDay);

            var firstWeekDays = 7 - dayOrWeek;//第一周天数

            days -= firstWeekDays;
            if (days <= 0) return 0;

            //从第二周开始计算。
            var weekIndex = 1;
            while (true)
            {
                days -= 7;

                if (days > 0)
                {
                    weekIndex++;
                }
                else
                {
                    break;
                }
            }

            return weekIndex;
        }

        /// <summary>
        /// 返回从星期一开始的索引
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private static int DayOrWeek(DateTime day)
        {
            var dayOrWeek = (int)day.DayOfWeek;
            if (dayOrWeek == 0) dayOrWeek = 7;
            dayOrWeek--;
            return dayOrWeek;
        }
    }
}
