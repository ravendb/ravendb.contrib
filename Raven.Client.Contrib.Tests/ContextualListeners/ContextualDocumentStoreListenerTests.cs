using System;
using Raven.Client.Contrib.ContextualListeners;
using Raven.Client.Contrib.Tests.ContextualListeners.Contexts;
using Xunit;

namespace Raven.Client.Contrib.Tests.ContextualListeners
{
	public class ContextualDocumentStoreListenerTests : AbstractContextualDocumentListenerTests
	{
		public ContextualDocumentStoreListenerTests()
		{
			DocumentStore.RegisterListener(new ContextualDocumentStoreListener<StoreContext>());
		}

		[Fact]
		public void Should_use_context()
		{
			using (var context = new StoreContext())
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var doc = new Doc {Id = "Doc"};
				session.Store(doc);
				session.SaveChanges();
				Assert.True(context.BeforeStoreCalled);
				Assert.True(context.AfterStoreCalled);
			}
		}

		[Fact]
		public void When_have_duplicate_context_Then_should_not_throw()
		{
			using (new StoreContext())
			{
				Assert.DoesNotThrow(() => new StoreContext());
			}
		}

		[Fact]
		public void When_no_context_Then_should_not_throw()
		{
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var doc = new Doc {Id = "Doc"};
				session.Store(doc);
				Assert.DoesNotThrow(() => session.SaveChanges());
			}
		}

		[Fact]
		public void Should_support_nested_sessions()
		{
			using (var context1 = new StoreContext())
			using (IDocumentSession session1 = DocumentStore.OpenSession())
			{
				var doc1 = new Doc {Id = "Doc1"};
				session1.Store(doc1);

				using (var context2 = new StoreContext())
				using (IDocumentSession session2 = DocumentStore.OpenSession())
				{
					var doc2 = new Doc {Id = "Doc2"};
					session2.Store(doc2);
					session2.SaveChanges();
					Assert.True(context2.BeforeStoreCalled);
					Assert.True(context2.AfterStoreCalled);
				}

				session1.SaveChanges();
				Assert.True(context1.BeforeStoreCalled);
				Assert.True(context1.AfterStoreCalled);
			}
		}
	}

	public class HttpContextualDocumentStoreListenerTests : ContextualDocumentStoreListenerTests
	{
		private readonly IDisposable _request;

		public HttpContextualDocumentStoreListenerTests()
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