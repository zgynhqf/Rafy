/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151207
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151207 15:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;

namespace Rafy.Domain.Stamp
{
    /// <summary>
    /// 此类会为所有的实体都添加一个 IsPhantom 的运行时属性。
    /// </summary>
    [CompiledPropertyDeclarer]
    public static class EntityStampExtension
    {
        #region CreatedTime

        /// <summary>
        /// 实体的创建时间。
        /// </summary>
        public static readonly Property<DateTime> CreatedTimeProperty =
            P<Entity>.RegisterExtension<DateTime>("CreatedTime", typeof(EntityStampExtension));
        /// <summary>
        /// 获取实体的创建时间。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static DateTime GetCreatedTime(this Entity me)
        {
            return me.GetProperty(CreatedTimeProperty);
        }
        /// <summary>
        /// 设置实体的创建时间。
        /// </summary>
        /// <param name="me"></param>
        /// <param name="value"></param>
        public static void SetCreatedTime(this Entity me, DateTime value)
        {
            me.SetProperty(CreatedTimeProperty, value);
        }

        #endregion

        #region UpdatedTime

        /// <summary>
        /// 实体的最后更新时间。
        /// </summary>
        public static readonly Property<DateTime> UpdatedTimeProperty =
            P<Entity>.RegisterExtension<DateTime>("UpdatedTime", typeof(EntityStampExtension));
        /// <summary>
        /// 获取实体的最后更新时间。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static DateTime GetUpdatedTime(this Entity me)
        {
            return me.GetProperty(UpdatedTimeProperty);
        }
        /// <summary>
        /// 设置实体的最后更新时间。
        /// </summary>
        /// <param name="me"></param>
        /// <param name="value"></param>
        public static void SetUpdatedTime(this Entity me, DateTime value)
        {
            me.SetProperty(UpdatedTimeProperty, value);
        }

        #endregion

        #region CreatedUser

        /// <summary>
        /// 实体的创建用户。
        /// </summary>
        public static readonly Property<string> CreatedUserProperty =
            P<Entity>.RegisterExtension<string>("CreatedUser", typeof(EntityStampExtension));
        /// <summary>
        /// 获取实体的创建用户。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static string GetCreatedUser(this Entity me)
        {
            return me.GetProperty(CreatedUserProperty);
        }
        /// <summary>
        /// 设置实体的创建用户。
        /// </summary>
        /// <param name="me"></param>
        /// <param name="value"></param>
        public static void SetCreatedUser(this Entity me, string value)
        {
            me.SetProperty(CreatedUserProperty, value);
        }

        #endregion

        #region UpdatedUser

        /// <summary>
        /// 实体的更新用户。
        /// </summary>
        public static readonly Property<string> UpdatedUserProperty =
            P<Entity>.RegisterExtension<string>("UpdatedUser", typeof(EntityStampExtension));
        /// <summary>
        /// 获取实体的更新用户。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static string GetUpdatedUser(this Entity me)
        {
            return me.GetProperty(UpdatedUserProperty);
        }
        /// <summary>
        /// 设置实体的更新用户。
        /// </summary>
        /// <param name="me"></param>
        /// <param name="value"></param>
        public static void SetUpdatedUser(this Entity me, string value)
        {
            me.SetProperty(UpdatedUserProperty, value);
        }

        #endregion
    }
}