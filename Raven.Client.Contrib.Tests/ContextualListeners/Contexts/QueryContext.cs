using Raven.Client.Contrib.ContextualListeners;

namespace Raven.Client.Contrib.Tests.ContextualListeners.Contexts
{
	internal class QueryContext : AbstractDocumentQueryListenerContext
	{
		internal bool BeforeQueryExecutedCalled { get; private set; }

		protected override void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
		{
			BeforeQueryExecutedCalled = true;
		}
	}
}