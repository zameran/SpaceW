using System;

namespace SpaceEngine.Core.Exceptions
{
    public class InvalidParameterException : BaseException
    {
        public InvalidParameterException()
        {

        }

        public InvalidParameterException(string message) : base(message)
        {

        }

        public InvalidParameterException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}