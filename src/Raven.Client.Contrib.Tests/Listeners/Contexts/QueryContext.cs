using Raven.Client.Listeners;

namespace Raven.Client.Contrib.Tests.Listeners.Contexts
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