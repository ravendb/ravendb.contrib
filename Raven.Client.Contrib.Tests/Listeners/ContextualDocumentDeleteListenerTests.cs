using System;
using Raven.Client.Contrib.Listeners;
using Raven.Client.Contrib.Tests.Listeners.Contexts;
using Xunit;

namespace Raven.Client.Contrib.Tests.Listeners
{
	public class ContextualDocumentDeleteListenerTests : AbstractContextualDocumentListenerTests
	{
		public ContextualDocumentDeleteListenerTests()
		{
			DocumentStore.RegisterListener(new ContextualDocumentDeleteListener<DeleteContext>());
		}

		[Fact]
		public void Should_use_context()
		{
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var doc = new Doc {Id = "Doc"};
				session.Store(doc);
				session.SaveChanges();
			}

			using (var context = new DeleteContext())
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var doc = session.Load<Doc>("Doc");
				session.Delete(doc);
				session.SaveChanges();
				Assert.True(context.BeforeDeleteCalled);
			}
		}

		[Fact]
		public void When_have_duplicate_context_Then_should_not_throw()
		{
			using (new DeleteContext())
			{
				Assert.DoesNotThrow(() => new DeleteContext());
			}
		}
	}

	public class HttpContextualDocumentDeleteListenerTests : ContextualDocumentDeleteListenerTests
	{
		private readonly IDisposable _request;

		public HttpContextualDocumentDeleteListenerTests()
		{
			_request = HttpContextHelper.SimulateRequest();
		}

		public override void Dispose()
		{
			_request.Dispose();
			base.Dispose();
		}
	}
}