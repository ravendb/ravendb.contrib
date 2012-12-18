using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.ContextualListeners
{
	/// <summary>
	/// Hook for users to provide additional logic for converting to / from entities to document/metadata pairs with context.
	/// </summary>
	public abstract class AbstractDocumentConversionListenerContext : AbstractDocumentListenerContext,
	                                                                  IDocumentConversionListener
	{
		void IDocumentConversionListener.EntityToDocument(string key, object entity, RavenJObject document,
		                                                  RavenJObject metadata)
		{
			EntityToDocument(key, entity, document, metadata);
		}
		
		void IDocumentConversionListener.DocumentToEntity(string key, object entity, RavenJObject document, RavenJObject metadata)
		{
			DocumentToEntity(key, entity, document, metadata);
		}

		/// <summary>
		/// Called when converting an entity to a document and metadata
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entity">The entity instance.</param>
		/// <param name="document">The document instance.</param>
		/// <param name="metadata">The metadata.</param>
		protected abstract void EntityToDocument(string key, object entity, RavenJObject document, RavenJObject metadata);

		/// <summary>
		/// Called when converting a document and metadata to an entity
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entity">The entity instance.</param>
		/// <param name="document">The document instance.</param>
		/// <param name="metadata">The metadata.</param>
		protected abstract void DocumentToEntity(string key, object entity, RavenJObject document, RavenJObject metadata);
	}
}