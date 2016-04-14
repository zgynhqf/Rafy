/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160413 16:59
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Accounts.Controllers
{
    /// <summary>
    /// 本插件中所用到的所有状态码。
    /// </summary>
    public static class ResultCodes
    {
        /// <summary>
        /// 获取或设置本插件中所有状态码的基础状态码。
        /// 其它的状态码都会在这个基础上进行添加。
        /// </summary>
        public static int BaseCode = 100000;

        public static int RegisterEmailDuplicated { get { return BaseCode + 1; } }

        public static int RegisterEmailInvalid { get { return BaseCode + 2; } }

        public static int RegisterUserNameDuplicated { get { return BaseCode + 3; } }

        public static int RegisterUserNameInvalid { get { return BaseCode + 4; } }

        public static int RegisterPropertiesInvalid { get { return BaseCode + 5; } }

        public static int LoginPasswordError { get { return BaseCode + 11; } }

        public static int LoginExceedMaxFailedTimes { get { return BaseCode + 12; } }

        public static int LoginUserNotExists { get { return BaseCode + 13; } }
    }
}
