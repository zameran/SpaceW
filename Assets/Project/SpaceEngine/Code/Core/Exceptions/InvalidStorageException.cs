using System;

namespace SpaceEngine.Core.Exceptions
{
    public class InvalidStorageException : BaseException
    {
        public InvalidStorageException()
        {

        }

        public InvalidStorageException(string message) : base(message)
        {

        }

        public InvalidStorageException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}