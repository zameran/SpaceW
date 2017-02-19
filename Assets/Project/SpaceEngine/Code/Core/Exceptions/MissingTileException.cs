using System;

namespace SpaceEngine.Core.Exceptions
{
    public class MissingTileException : BaseException
    {
        public MissingTileException()
        {

        }

        public MissingTileException(string message) : base(message)
        {

        }

        public MissingTileException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}