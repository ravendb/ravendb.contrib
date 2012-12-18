using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.ContextualListeners
{
	/// <summary>
	/// Hook for users to provide additional logic on store operations with context.
	/// </summary>
	public abstract class AbstractDocumentStoreListenerContext : AbstractDocumentListenerContext, IDocumentStoreListener
	{
		bool IDocumentStoreListener.BeforeStore(string key, object entityInstance, RavenJObject metadata,
		                                        RavenJObject original)
		{
			return BeforeStore(key, entityInstance, metadata);
		}
		
		void IDocumentStoreListener.AfterStore(string key, object entityInstance, RavenJObject metadata)
		{
			AfterStore(key, entityInstance, metadata);
		}

		/// <summary>
		/// Invoked before the store request is sent to the server.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entityInstance">The entity instance.</param>
		/// <param name="metadata">The metadata.</param>
		/// <param name="original">The original document that was loaded from the server</param>
		/// <returns>
		/// Whatever the entity instance was modified and requires us re-serialize it.
		/// Returning true would force re-serialization of the entity, returning false would
		/// mean that any changes to the entityInstance would be ignored in the current SaveChanges call.
		/// </returns>
		protected abstract void AfterStore(string key, object entityInstance, RavenJObject metadata);

		/// <summary>
		/// Invoked after the store request is sent to the server.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entityInstance">The entity instance.</param>
		/// <param name="metadata">The metadata.</param>
		protected abstract bool BeforeStore(string key, object entityInstance, RavenJObject metadata);
	}
}