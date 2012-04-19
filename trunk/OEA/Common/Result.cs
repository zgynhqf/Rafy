using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace hxy.Common
{
    /// <summary>
    /// indicates the result of a normal invoking the "service"
    /// </summary>
    [DataContract, Serializable]
    public struct Result
    {
        /// <summary>
        /// Message=string.Empty
        /// </summary>
        /// <param name="success"></param>
        public Result(bool success)
        {
            if (success)
            {
                this._statusCode = 1;
                this._message = "操作成功！";
            }
            else
            {
                this._statusCode = 0;
                this._message = "操作失败！";
            }
        }

        /// <summary>
        /// Success = false
        /// </summary>
        /// <param name="message"></param>
        public Result(string message)
        {
            this._statusCode = 0;
            this._message = message;
        }

        public Result(bool success, string message)
        {
            this._statusCode = success ? 1 : 0;
            this._message = message;
        }

        /// <summary>
        /// status code indecates the result type of this invoking.
        /// 1:      Success.
        /// 0:      Failed.
        /// other:  other customized types.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public Result(int statusCode, string message)
        {
            this._statusCode = statusCode;
            this._message = message;
        }

        private int _statusCode;

        private string _message;

        /// <summary>
        /// Indicates this invoking is success or failed.
        /// When the StatusCode equals 1, Success equals true.
        /// </summary>
        [DataMember]
        public bool Success
        {
            get
            {
                return _statusCode == 1;
            }
            set
            {
                _statusCode = value ? 1 : 0;
            }
        }

        /// <summary>
        /// status code indecates the result type of this invoking.
        /// 1:      Success.
        /// 0:      Failed.
        /// other:  other customized types.
        /// </summary>
        [DataMember]
        public int StatusCode
        {
            get
            {
                return _statusCode;
            }
            set
            {
                _statusCode = value;
            }
        }

        /// <summary>
        /// represent the message from this invoking.
        /// if successed, this property represent some useful string.(eg. a html document.).
        /// if failed, this property represent a message that can be used to show to the end users.
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return _message == null ? string.Empty : _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Reset this result to a unsuccessful status.
        /// </summary>
        public void Reset()
        {
            this._message = string.Empty;
            this._statusCode = 0;
        }

        public static implicit operator Result(bool value)
        {
            return new Result(value);
        }

        public static implicit operator Result(string error)
        {
            return new Result(false, error);
        }

        public static implicit operator bool(Result res)
        {
            return res.Success;
        }
    }
}
