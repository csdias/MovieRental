namespace MovieRental.Common
{
    public class Response<T>
    {
        public bool isSuccess { get; set; }
        private T? Data { get; set; }
        private IList<Error>? Errors { get; set; }

        public void SetData(T data)
        {
            this.Data = data;
            isSuccess = true;
        }

        public T? GetData()
        {
            return Data;
        }

        public void AddError(string message)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }
            Errors.Add(new Error { Message = message });
            isSuccess = false;
        }

        public IList<Error>? GetErrors()
        {
            return Errors;
        }
    }
}