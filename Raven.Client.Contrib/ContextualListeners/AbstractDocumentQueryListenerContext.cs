using Raven.Client.Listeners;

namespace Raven.Client.Contrib.ContextualListeners
{
	public abstract class AbstractDocumentQueryListenerContext : AbstractDocumentListenerContext, IDocumentQueryListener
	{
		void IDocumentQueryListener.BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
		{
			BeforeQueryExecuted(queryCustomization);
		}

		protected abstract void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization);
	}
}