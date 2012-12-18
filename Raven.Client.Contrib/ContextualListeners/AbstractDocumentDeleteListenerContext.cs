using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace Raven.Client.ContextualListeners
{
	public abstract class AbstractDocumentDeleteListenerContext : AbstractDocumentListenerContext, IDocumentDeleteListener
	{
		void IDocumentDeleteListener.BeforeDelete(string key, object entityInstance, RavenJObject metadata)
		{
			BeforeDelete(key, entityInstance, metadata);
		}

		protected abstract void BeforeDelete(string key, object entityInstance, RavenJObject metadata);
	}
}