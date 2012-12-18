using System.Collections.Generic;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Raven.Client.ContextualListeners
{
	public class ContextualDocumentStoreListener<T> : IDocumentStoreListener
		where T : AbstractDocumentStoreListenerContext
	{
		public virtual bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original)
		{
			Stack<object> context;
			if (!LocalStorageProvider.Get().Contexts.TryGetValue(typeof (T), out context))
			{
				return false;
			}
			return ((IDocumentStoreListener) context.Peek()).BeforeStore(key, entityInstance, metadata, original);
		}

		public virtual void AfterStore(string key, object entityInstance, RavenJObject metadata)
		{
			Stack<object> context;
			if (LocalStorageProvider.Get().Contexts.TryGetValue(typeof (T), out context))
			{
				((IDocumentStoreListener) context.Peek()).AfterStore(key, entityInstance, metadata);
			}
		}
	}
}