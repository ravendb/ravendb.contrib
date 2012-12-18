using System;
using System.Collections.Generic;

namespace Raven.Client.Contrib.Listeners
{
	/// <summary>
	/// Base listener context class. Inherit from one of <see cref="AbstractDocumentConversionListenerContext"/>,
	/// <see cref="AbstractDocumentDeleteListenerContext"/> , <see cref="AbstractDocumentQueryListenerContext"/> 
	/// <see cref="AbstractDocumentStoreListenerContext"/> insead.
	/// </summary>
	public abstract class AbstractDocumentListenerContext : IDisposable
	{
		internal AbstractDocumentListenerContext()
		{
			Dictionary<Type, Stack<object>> contexts = LocalStorageProvider.Get().Contexts;
			Type type = GetType();
			if (!contexts.ContainsKey(type))
			{
				contexts.Add(type, new Stack<object>());
			}
			contexts[type].Push(this);
		}

		public void Dispose()
		{
			Dictionary<Type, Stack<object>> contexts = LocalStorageProvider.Get().Contexts;
			Type type = GetType();
			contexts[type].Pop();
			if (contexts[type].Count == 0)
			{
				contexts.Remove(type);
			}
		}
	}
}