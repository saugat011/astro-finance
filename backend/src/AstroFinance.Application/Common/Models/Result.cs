namespace AstroFinance.Application.Common.Models
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
        }

        public bool Succeeded { get; set; }

        public string[] Errors { get; set; }

        public static Result Success()
        {
            return new Result(true, Array.Empty<string>());
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }
        
        public static Result Failure(string error)
        {
            return new Result(false, new[] { error });
        }
    }
    
    public class Result<T> : Result
    {
        private Result(bool succeeded, IEnumerable<string> errors, T? value) : base(succeeded, errors)
        {
            Value = value;
        }
        
        public T? Value { get; set; }
        
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, Array.Empty<string>(), value);
        }
        
        public new static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, errors, default);
        }
        
        public new static Result<T> Failure(string error)
        {
            return new Result<T>(false, new[] { error }, default);
        }
    }
}