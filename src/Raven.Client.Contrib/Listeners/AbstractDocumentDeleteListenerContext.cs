using Raven.Json.Linq;

namespace Raven.Client.Listeners
{
	/// <summary>
	/// Hook for users to provide additional logic on delete operations with context.
	/// </summary>
	public abstract class AbstractDocumentDeleteListenerContext : AbstractDocumentListenerContext, IDocumentDeleteListener
	{
		void IDocumentDeleteListener.BeforeDelete(string key, object entityInstance, RavenJObject metadata)
		{
			BeforeDelete(key, entityInstance, metadata);
		}

		/// <summary>
		/// Invoked before the delete request is sent to the server.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entityInstance">The entity instance.</param>
		/// <param name="metadata">The metadata.</param>
		protected abstract void BeforeDelete(string key, object entityInstance, RavenJObject metadata);
	}
}