using System;

namespace Lever.Common
{
    public class CustomException : Exception
    {
        public CustomException(int code, string message) : base(message)
        {
            this.Code = code;
        }

        public CustomException(int code, string message, Exception innerException) : base(message, innerException)
        {
            this.Code = code;
        }

        public int Code { get; set; }

    }
}
