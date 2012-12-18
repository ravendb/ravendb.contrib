using System;
using System.Collections.Generic;

namespace Raven.Client.Contrib.ContextualListeners
{
	internal abstract class AbstractLocalStorage
	{
		public Dictionary<Type, Stack<object>> Contexts
		{
			get { return GetContexts(); }
		}

		protected abstract Dictionary<Type, Stack<object>> GetContexts();
	}
}