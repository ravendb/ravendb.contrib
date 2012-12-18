using Raven.Client.Contrib.ContextualListeners;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.Tests.ContextualListeners.Contexts
{
	internal class DeleteContext : AbstractDocumentDeleteListenerContext
	{
		internal bool BeforeDeleteCalled { get; private set; }

		protected override void BeforeDelete(string key, object entityInstance, RavenJObject metadata)
		{
			BeforeDeleteCalled = true;
		}
	}
}