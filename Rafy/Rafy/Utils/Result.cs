/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2007
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2007
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Rafy
{
    /// <summary>
    /// indicates the result of a normal invoking the "service"
    /// </summary>
    [DataContract, Serializable]
    [DebuggerDisplay("Success:{Success}, StatusCode:{StatusCode}, Message:{Message}")]
    public struct Result
    {
        /// <summary>
        /// A string message used by the success result.
        /// </summary>
        public static string SuccessMessage = "操作成功！";

        /// <summary>
        /// A string message used by the failed result.
        /// </summary>
        public static string FailedMessage = "操作失败！";

        #region fields

        private bool _success;

        private int _statusCode;

        private string _message;

        private object _data;

        #endregion

        #region Constructors

        /// <summary>
        /// Message=string.Empty
        /// </summary>
        /// <param name="success"></param>
        public Result(bool success)
        {
            _success = success;
            _statusCode = 0;
            _message = success ? SuccessMessage : FailedMessage;
            _data = null;
        }

        /// <summary>
        /// Success = false
        /// </summary>
        /// <param name="message"></param>
        public Result(string message)
        {
            _success = false;
            _statusCode = 0;
            _message = message;
            _data = null;
        }

        /// <summary>
        /// create a error message with its status.
        /// </summary>
        /// <param name="statusCode"></param>
        public Result(int statusCode)
        {
            _success = false;
            _statusCode = statusCode;
            _message = FailedMessage;
            _data = null;
        }

        /// <summary>
        /// create a successful result with corresponding data.
        /// </summary>
        /// <param name="data">The data.</param>
        public Result(object data)
        {
            _success = true;
            _statusCode = 0;
            _message = SuccessMessage;
            _data = data;
        }

        /// <summary>
        /// create a result with its message.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public Result(bool success, string message)
        {
            _success = success;
            _statusCode = 0;
            _message = message;
            _data = null;
        }

        /// <summary>
        /// create a result by specifing its success status and a message.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="statusCode"></param>
        public Result(bool success, int statusCode)
        {
            _success = success;
            _statusCode = statusCode;
            _message = success ? SuccessMessage : FailedMessage;
            _data = null;
        }

        /// <summary>
        /// create a failed result by its statusCode and a error message.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public Result(int statusCode, string message)
        {
            _success = false;
            _statusCode = statusCode;
            _message = message;
            _data = null;
        }

        /// <summary>
        /// create a result by specifing all its status.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public Result(bool success, int statusCode, string message)
        {
            _success = success;
            _statusCode = statusCode;
            _message = message;
            _data = null;
        }

        #endregion

        /// <summary>
        /// Indicates this invoking is success or failed.
        /// When the StatusCode equals 1, Success equals true.
        /// </summary>
        [DataMember]
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        /// <summary>
        /// status code indecates the result type of this invoking.
        /// other:  other customized types.
        /// </summary>
        [DataMember]
        public int StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        /// <summary>
        /// Represents the message from this invoking.
        /// if successed, this property represent some useful string.(eg. a html document.).
        /// if failed, this property represent a message that can be used to show to the end users.
        /// 
        /// (this property doesn't return null.)
        /// </summary>
        [DataMember]
        public string Message
        {
            get
            {
                if (_message == null) { return string.Empty; }
                return _message;
            }
            set
            {
                if (value == null) value = string.Empty;
                _message = value;
            }
        }

        /// <summary>
        /// the data result.
        /// </summary>
        [DataMember]
        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Reset this result to a unsuccessful status.
        /// </summary>
        public void Reset()
        {
            _success = false;
            _statusCode = 0;
            _message = string.Empty;
        }

        /// <summary>
        /// Indicates whether this instance's StatusCode and a specified object's StatusCode are equal.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public bool StatusEquals(Result another)
        {
            return _success == another.Success && _statusCode == another.StatusCode;
        }

        #region implicit operators

        public static implicit operator Result(bool value)
        {
            return new Result(value);
        }

        public static implicit operator Result(int statusCode)
        {
            return new Result(statusCode);
        }

        public static implicit operator Result(Enum statusCode)
        {
            return new Result(Convert.ToInt32(statusCode));
        }

        public static implicit operator Result(string error)
        {
            return new Result(error);
        }

        public static implicit operator bool(Result res)
        {
            return res.Success;
        }

        #endregion
    }
}