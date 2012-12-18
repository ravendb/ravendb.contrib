using System;
using Raven.Client.Contrib.Listeners;
using Raven.Client.Contrib.Tests.Listeners.Contexts;
using Xunit;

namespace Raven.Client.Contrib.Tests.Listeners
{
	public class ContextualDocumentConversionListenerTests : AbstractContextualDocumentListenerTests
	{
		public ContextualDocumentConversionListenerTests()
		{
			DocumentStore.RegisterListener(new ContextualDocumentConversionListener<ConversionContext>());
		}

		[Fact]
		public void Should_use_context()
		{
			using (var context = new ConversionContext())
			{
				using (IDocumentSession session = DocumentStore.OpenSession())
				{
					var doc = new Doc {Id = "Doc"};
					session.Store(doc);
					session.SaveChanges();
					Assert.True(context.EntityToDocumentCalled);
				}
			}

			using (var context = new ConversionContext())
			{
				using (IDocumentSession session = DocumentStore.OpenSession())
				{
					session.Load<Doc>("Doc");
					Assert.True(context.DocumentToEntityCalled);
				}
			}
		}

		[Fact]
		public void When_have_duplicate_context_Then_should_not_throw()
		{
			using (new ConversionContext())
			{
				Assert.DoesNotThrow(() => new ConversionContext());
			}
		}
	}

	public class HttpContextContextualDocumentConversionListenerTests : ContextualDocumentConversionListenerTests
	{
		private readonly IDisposable _request;

		public HttpContextContextualDocumentConversionListenerTests()
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