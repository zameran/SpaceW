using System;

namespace SpaceEngine.Core.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException()
        {

        }

        public BaseException(string message) : base(message)
        {

        }

        public BaseException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}