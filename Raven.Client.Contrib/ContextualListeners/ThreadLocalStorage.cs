using System;
using System.Collections.Generic;

namespace Raven.Client.Contrib.ContextualListeners
{
	internal class ThreadLocalStorage : AbstractLocalStorage
	{
		[ThreadStatic] private static Dictionary<Type, Stack<object>> ContextsThreadStatic;

		protected override Dictionary<Type, Stack<object>> GetContexts()
		{
			return ContextsThreadStatic ?? (ContextsThreadStatic = new Dictionary<Type, Stack<object>>());
		}
	}
}