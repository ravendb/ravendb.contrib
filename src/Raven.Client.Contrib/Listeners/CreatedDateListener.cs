using Raven.Abstractions;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace Raven.Client.Listeners
{
	/// <summary>
	/// A client-side listener that adds a Created metadata value to every document being stored.
	/// Uses the clock on the client.  For better precision, use a server-side trigger instead.
	/// </summary>
	public class CreatedDateListener : IDocumentStoreListener
	{
		public bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original)
		{
			RavenJToken lastModified;
			if (!metadata.TryGetValue(Constants.LastModified, out lastModified))
				metadata.Add("Created", SystemTime.UtcNow);

			return true;
		}

		public void AfterStore(string key, object entityInstance, RavenJObject metadata) { }
	}
}
