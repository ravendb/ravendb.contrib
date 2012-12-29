using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Session
{
    internal class Disposable : IDisposable
    {
        private Action _action;

        /// <summary>
        /// Creates a new IDisposable object that executes the specified action when disposed.
        /// </summary>
        /// <param name="action">The action to execute on dispose.</param>
        private Disposable(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        void IDisposable.Dispose()
        {
            if (_action != null)
            {
                _action.Invoke();
                _action = null;
            }
        }

        /// <summary>
        /// Creates a new IDisposable object that executes the specified action when disposed.
        /// </summary>
        /// <param name="action">The action to execute on dispose.</param>
        public static IDisposable Create(Action action)
        {
            return new Disposable(action);
        }
    }
}
