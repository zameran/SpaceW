using System;

namespace SpaceEngine.Core.Exceptions
{
    public class CacheCapacityException : BaseException
    {
        public CacheCapacityException()
        {

        }

        public CacheCapacityException(string message) : base(message)
        {

        }

        public CacheCapacityException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}