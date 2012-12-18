using System.Collections.Generic;
using Raven.Json.Linq;

namespace Raven.Client.Listeners
{
	public class ContextualDocumentConversionListener<T> : IDocumentConversionListener
		where T : AbstractDocumentConversionListenerContext
	{
		public virtual void EntityToDocument(string key, object entity, RavenJObject document, RavenJObject metadata)
		{
			Stack<object> context;
			if (LocalStorageProvider.Get().Contexts.TryGetValue(typeof (T), out context))
			{
				((IDocumentConversionListener) context.Peek()).EntityToDocument(key, entity, document, metadata);
			}
		}

		public virtual void DocumentToEntity(string key, object entity, RavenJObject document, RavenJObject metadata)
		{
			Stack<object> context;
			if (LocalStorageProvider.Get().Contexts.TryGetValue(typeof (T), out context))
			{
				((IDocumentConversionListener) context.Peek()).DocumentToEntity(key, entity, document, metadata);
			}
		}
	}
}