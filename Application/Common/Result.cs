
using Microsoft.AspNetCore.Mvc;

namespace simple_rank_backend.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; } // Using a dedicated Error class/record
        public string Message { get; protected set; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException("Successful result cannot have an error.");
            }
            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException("Failed result must have an error.");
            }

            IsSuccess = isSuccess;
            Message = string.Empty;
            Error = error;
        }

        protected Result(bool isSuccess, string msg, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException("Successful result cannot have an error.");
            }
            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException("Failed result must have an error.");
            }

            IsSuccess = isSuccess;
            Message = msg;
            Error = error;
        }

        public static Result Success() => new Result(true, Error.None);
        public static Result Success(string msg) => new Result(true, msg, Error.None);
        public static Result Failure(Error error) => new Result(false, error);
        public static Result Failure(Error error, string msg) => new Result(false, msg, error);

        public static Result HandleSupabase(HttpResponseMessage response, string msg = "")
        {
            if(response.IsSuccessStatusCode)
            {
                if(string.IsNullOrEmpty(msg))
                {
                    return Result.Success();
                }
                else
                {
                    return Result.Success(msg);
                }
            }
            else
            {
                if(string.IsNullOrEmpty(msg))
                {
                    return Result.Failure(new Error(response.StatusCode.ToString(), response.ToString()));
                }
                else
                {
                    return Result.Failure(new Error(response.StatusCode.ToString(), response.ToString()), msg);
                }
                
            }
        }
        // Implicit conversion for convenience in controllers
        public static implicit operator Result(Error error) => Failure(error);
    }

    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");

        protected Result(TValue value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        protected Result(TValue value, string msg, bool isSuccess, Error error)
            : base(isSuccess, msg, error)
        {
            _value = value;
        }


        public static Result<TValue> Success(TValue value, string msg) => new Result<TValue>(value, msg, true, Error.None);
        public static Result<TValue> Success(TValue value) => new Result<TValue>(value, true, Error.None);
        public new static Result<TValue> Failure(Error error) => new Result<TValue>(default!, false, error); // `default` for TValue as it's irrelevant on failure
        public new static Result<TValue> Failure(Error error, string msg) => new Result<TValue>(default!, msg, false, error);


        public static Result<TValue> HandleSupabase(HttpResponseMessage response, TValue value, string msg = "")
        {
            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(msg))
                {
                    return Result<TValue>.Success(value);
                }
                else
                {
                    return Result<TValue>.Success(value, msg);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(msg))
                {
                    return Result<TValue>.Failure(new Error(response.StatusCode.ToString(), response.ToString()));
                }
                else
                {
                    return Result<TValue>.Failure(new Error(response.StatusCode.ToString(), response.ToString()), msg);
                }

            }
        }

        // Implicit conversion for convenience
        public static implicit operator Result<TValue>(TValue value) => Success(value);
        public static implicit operator Result<TValue>(Error error) => Failure(error);

        
    }

}
