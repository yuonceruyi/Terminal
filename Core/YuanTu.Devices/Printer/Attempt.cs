using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.Printer
{
    public static class Attempt
    {
        public static Attempt<T> Succeed<T>(T result)
        {
            return Attempt<T>.Succeed(result);
        }

        public static Attempt<T> Succeed<T>(T result, string message)
        {
            return Attempt<T>.Succeed(result, message);
        }

        public static Attempt<T> Fail<T>(T result)
        {
            return Attempt<T>.Fail(result);
        }

        public static Attempt<T> Fail<T>(T result, string message)
        {
            return Attempt<T>.Fail(result, message);
        }

        public static Attempt<T> Fail<T>(T result, Exception exception)
        {
            return Attempt<T>.Fail(result, exception);
        }

        public static Attempt<T> If<T>(bool success, T result, string message = null)
        {
            return Attempt<T>.SucceedIf(success, result, message);
        }

        public static Outcome Try<T>(Attempt<T> attempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            if (attempt.Success)
            {
                onSuccess(attempt.Result);
                return Outcome.Success;
            }

            if (onFail != null)
            {
                onFail(attempt.Exception);
            }

            return Outcome.Failure;
        }

        public struct Outcome
        {
            private readonly bool _success;

            public static readonly Outcome Success = new Outcome(true);

            public static readonly Outcome Failure = new Outcome(false);

            private Outcome(bool success)
            {
                _success = success;
            }

            public Outcome OnFailure<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                return _success
                    ? Success
                    : ExecuteNextFunction(nextFunction, onSuccess, onFail);
            }

            public Outcome OnSuccess<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                return _success
                    ? ExecuteNextFunction(nextFunction, onSuccess, onFail)
                    : Failure;
            }

            private static Outcome ExecuteNextFunction<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                var attempt = nextFunction();

                if (attempt.Success)
                {
                    onSuccess(attempt.Result);
                    return Success;
                }

                if (onFail != null)
                {
                    onFail(attempt.Exception);
                }

                return Failure;
            }
        }
    }

    [Serializable]
    public struct Attempt<T>
    {
        private readonly bool _success;
        private readonly T _result;
        private readonly Exception _exception;
        private readonly string _message;

        public bool Success
        {
            get { return _success; }
        }

        public Exception Exception { get { return _exception; } }

        public string Message { get { return _message; } }

        public T Result
        {
            get { return _result; }
        }

        private static readonly Attempt<T> Failed = new Attempt<T>(false, default(T), null);

        public static readonly Attempt<T> False = Failed;

        private Attempt(bool success, T result, Exception exception = null, string message = null)
        {
            _success = success;
            _result = result;
            _exception = exception;
            _message = message;
        }

        public static Attempt<T> Succeed()
        {
            return new Attempt<T>(true, default(T), null, null);
        }

        public static Attempt<T> Succeed(T result)
        {
            return new Attempt<T>(true, result, null, null);
        }

        public static Attempt<T> Succeed(T result, string message)
        {
            return new Attempt<T>(true, result, null, message);
        }

        public static Attempt<T> Fail()
        {
            return Failed;
        }

        public static Attempt<T> Fail(Exception exception)
        {
            return new Attempt<T>(false, default(T), exception, exception == null ? "" : exception.Message);
        }

        public static Attempt<T> Fail(string message)
        {
            return new Attempt<T>(false, default(T), null, message);
        }

        public static Attempt<T> Fail(T result)
        {
            return new Attempt<T>(false, result, null, null);
        }

        public static Attempt<T> Fail(T result, Exception exception)
        {
            return new Attempt<T>(false, result, exception, exception == null ? "" : exception.Message);
        }

        public static Attempt<T> Fail(T result, string message)
        {
            return new Attempt<T>(false, result, null, message);
        }

        public static Attempt<T> SucceedIf(bool condition)
        {
            return condition ? new Attempt<T>(true, default(T), null) : Failed;
        }

        public static Attempt<T> SucceedIf(bool condition, T result, string message = null)
        {
            return new Attempt<T>(condition, result, null, message);
        }

    }
}
