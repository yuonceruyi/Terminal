using System;
using System.Text;

namespace YuanTu.Consts.FrameworkBase
{
    /// <summary>
    ///     封装操作返回结果和错误信息
    /// </summary>
    public struct Result
    {
        public Result(bool success, long resultCode, string error)
        {
            IsSuccess = success;
            ResultCode = resultCode;
            Message = error;
            Exception = null;
        }

        public Result(bool success, long resultCode, string error, Exception exception)
            : this(success, resultCode, error)
        {
            Exception = exception;
        }

        public bool IsSuccess { get; set; }
        public long ResultCode { get; set; }
        public string Message { get; set; }

        public Exception Exception { get; set; }

        public static implicit operator bool(Result result)
        {
            return result.IsSuccess;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsSuccess ? "Success" : "Fail");
            if (!string.IsNullOrEmpty(Message))
                sb.Append(" " + Message);
            return sb.ToString();
        }

        public static Result Success()
        {
            return new Result(true, 0, string.Empty);
        }

        public static Result Fail(string message)
        {
            return new Result(false, 0, message);
        }

        public static Result Fail(long resultCode, string message)
        {
            return new Result(false, resultCode, message);
        }

       
        public static Result Fail(long resultCode, string message, Exception exception)
        {
            return new Result(false, resultCode, message, exception);
        }

        public static Result Fail(string message, Exception exception)
        {
            return Fail(0,message,exception);
        }
    }

    /// <summary>
    ///     封装操作结果 返回数据和错误信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Result<T>
    {
        public bool IsSuccess { get; set; }
        public long ResultCode { get; set; }
        public string Message { get; set; }

        public Exception Exception { get; set; }

        public static implicit operator bool(Result<T> result)
        {
            return result.IsSuccess;
        }

        public Result(bool success, long resultCode, string error, T value)
            : this(success, resultCode, error, null, value)
        {
            Value = value;
        }

        public Result(bool success, long resultCode, string error, Exception exception = null, T value = default(T))
        {
            IsSuccess = success;
            ResultCode = resultCode;
            Message = error;
            Exception = exception;
            Value = value;
        }

        public T Value { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsSuccess ? "Success" : "Fail");
            if (!string.IsNullOrEmpty(Message))
                sb.Append(" " + Message);
            sb.Append($"Value={Value}");
            return sb.ToString();
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, 0, string.Empty, value);
        }

        public static Result<T> Fail(string message)
        {
            return new Result<T>(false, 0, message);
        }

        public static Result<T> Fail(long resultCode, string message)
        {
            return new Result<T>(false, resultCode, message);
        }

        public static Result<T> Fail(long resultCode, string message, Exception exception)
        {
            return new Result<T>(false, resultCode, message, exception);
        }

        public static Result<T> Fail(string message, Exception exception)
        {
           return Fail(0,message,exception);
        }
    }

    public static class ResultExtension
    {
        public static Result Convert<T>(this Result<T> ori)
        {
            return new Result(ori.IsSuccess, ori.ResultCode, ori.Message, ori.Exception);
        }

        public static Result<T> Convert<T>(this Result ori)
        {
            return new Result<T>(ori.IsSuccess, ori.ResultCode, ori.Message, ori.Exception, default(T));
        }
    }
}