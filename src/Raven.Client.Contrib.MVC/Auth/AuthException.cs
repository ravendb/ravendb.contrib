using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Raven.Client.Contrib.MVC.Auth
{
    [Serializable]
    public abstract class AuthException : Exception
    {
        protected AuthException()
        {

        }

        protected AuthException(string message)
            : base(message)
        {

        }

        protected AuthException(string message, Exception inner)
            : base(message, inner)
        {

        }

        protected AuthException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}