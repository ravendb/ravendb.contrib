using Raven.Client.ContextualListeners;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Raven.Client.Contrib.Tests.ContextualListeners
{
	public class Sample
	{
		public class MyApp
		{
			public MyApp()
			{
				var documentStore = new DocumentStore
					                    {
						                    Url = "http://server"
					                    };

				documentStore.RegisterListener(new ContextualDocumentStoreListener<UserNameContext>());
					// can be called after .Initialize(), doesn't matter
				documentStore.Initialize();

				using (new UserNameContext("Damian Hickey"))
				using (IDocumentSession session = documentStore.OpenSession())
				{
					session.Store(new Doc());
					session.SaveChanges();
				}
			}
		}

		public class UserNameContext : AbstractDocumentStoreListenerContext
		{
			private readonly string _userName;

			public UserNameContext(string userName)
			{
				_userName = userName;
			}

			protected override void AfterStore(string key, object entityInstance, RavenJObject metadata)
			{
			}

			protected override bool BeforeStore(string key, object entityInstance, RavenJObject metadata)
			{
				metadata.Add("UserName", RavenJToken.FromObject(_userName));
				return false; //return true if you modify the entityInstance
			}
		}
	}
}