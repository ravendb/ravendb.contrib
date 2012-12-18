using System.Linq;
using Raven.Client.Contrib.Listeners;
using Raven.Client.Contrib.Tests.Listeners.Contexts;
using Xunit;

namespace Raven.Client.Contrib.Tests.Listeners
{
	public class MultipleContextualDocumentListenerTests : AbstractContextualDocumentListenerTests
	{
		public MultipleContextualDocumentListenerTests()
		{
			DocumentStore.RegisterListener(new ContextualDocumentDeleteListener<DeleteContext>());
			DocumentStore.RegisterListener(new ContextualDocumentStoreListener<StoreContext>());
			DocumentStore.RegisterListener(new ContextualDocumentQueryListener<QueryContext>());
			DocumentStore.RegisterListener(new ContextualDocumentConversionListener<ConversionContext>());
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

			using (var storeContext = new StoreContext())
			using (var deleteContext = new DeleteContext())
			using (var queryContext = new QueryContext())
			using (var conversionContext = new ConversionContext())
			using (IDocumentSession session = DocumentStore.OpenSession())
			{
				var doc1 = new Doc {Id = "Doc1"};
				session.Store(doc1);
				Doc doc2 = session.Query<Doc>().Customize(c => c.WaitForNonStaleResults()).Single();
				session.Delete(doc2);
				session.SaveChanges();
				Assert.True(storeContext.AfterStoreCalled);
				Assert.True(storeContext.BeforeStoreCalled);
				Assert.True(deleteContext.BeforeDeleteCalled);
				Assert.True(queryContext.BeforeQueryExecutedCalled);
				Assert.True(conversionContext.DocumentToEntityCalled);
				Assert.True(conversionContext.EntityToDocumentCalled);
			}
		}
	}
}