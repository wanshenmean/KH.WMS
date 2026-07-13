using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.License.Results
{
    /// <summary>
    /// 操作结果
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public string? Message { get; }

        protected Result(bool isSuccess, string? error, string? message = null)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new ArgumentException("Success result cannot have an error message");
            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new ArgumentException("Failure result must have an error message");

            IsSuccess = isSuccess;
            Error = error;
            Message = message;
        }

        public static Result Success(string? message = null) => new(true, null, message);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value, string? message = null) => new(value, true, null, message);
        public static Result<T> Failure<T>(string error) => new(default, false, error);
    }

    /// <summary>
    /// 带值的操作结果
    /// </summary>
    public class Result<T> : Result
    {
        public T? Value { get; }

        protected internal Result(T? value, bool isSuccess, string? error, string? message = null)
            : base(isSuccess, error, message)
        {
            Value = value;
        }

        public static new Result<T> Success(T value, string? message = null) => new(value, true, null, message);
        public static new Result<T> Failure(string error) => new(default, false, error);
    }
}
