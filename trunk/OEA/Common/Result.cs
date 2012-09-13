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
        public static int SuccessStatusCode = -12345;

        public static int FailedStatusCode = -54321;

        public static string SuccessMessage = "操作成功！";

        public static string FailedMessage = "操作失败！";

        /// <summary>
        /// Message=string.Empty
        /// </summary>
        /// <param name="success"></param>
        public Result(bool success)
        {
            if (success)
            {
                this._statusCode = SuccessStatusCode;
                this._message = SuccessMessage;
            }
            else
            {
                this._statusCode = FailedStatusCode;
                this._message = FailedMessage;
            }
        }

        /// <summary>
        /// Success = false
        /// </summary>
        /// <param name="message"></param>
        public Result(string message)
        {
            this._statusCode = FailedStatusCode;
            this._message = message;
        }

        public Result(bool success, string message)
        {
            this._statusCode = success ? SuccessStatusCode : FailedStatusCode;
            this._message = message;
        }

        /// <summary>
        /// status code indecates the result type of this invoking.
        /// other:  other customized types.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public Result(int statusCode, string message)
        {
            this._statusCode = statusCode;
            this._message = message;
        }

        public Result(Enum statusCode, string message)
        {
            this._statusCode = Convert.ToInt32(statusCode);
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
                return _statusCode == SuccessStatusCode;
            }
            set
            {
                _statusCode = value ? SuccessStatusCode : FailedStatusCode;
            }
        }

        /// <summary>
        /// status code indecates the result type of this invoking.
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
            this._statusCode = FailedStatusCode;
        }

        public static implicit operator Result(bool value)
        {
            return new Result(value);
        }

        public static implicit operator Result(int statusCode)
        {
            var msg = statusCode == SuccessStatusCode ? SuccessMessage : FailedMessage;
            return new Result(statusCode, msg);
        }

        public static implicit operator Result(Enum statusCode)
        {
            var iStatusCode = Convert.ToInt32(statusCode);
            var msg = iStatusCode == SuccessStatusCode ? SuccessMessage : FailedMessage;
            return new Result(statusCode, msg);
        }

        public static implicit operator Result(string error)
        {
            return new Result(false, error);
        }

        public static implicit operator bool(Result res)
        {
            return res.Success;
        }

        /// <summary>
        /// Indicates whether this instance's StatusCode and a specified object's StatusCode are equal.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public bool StatusEquals(Result another)
        {
            return this._statusCode == another._statusCode;
        }
    }
}
