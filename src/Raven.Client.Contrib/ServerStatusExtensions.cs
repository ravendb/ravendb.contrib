using Raven.Abstractions.Data;

namespace Raven.Client
{
	public static class ServerStatusExtensions
	{
		/// <summary>
		/// Tries to get the version information from the RavenDB server associated with the document store.
		/// </summary>
		/// <param name="documentStore">The document store.</param>
		/// <param name="buildNumber">If the server is reachable, this will output the data containing the product version and build number.</param>
		/// <param name="timeoutMilliseconds">Timeout in milliseconds.  Defaults to 5 seconds.</param>
		/// <returns>True if the server is online, false otherwise.</returns>
		public static bool TryGetServerVersion(this IDocumentStore documentStore, out BuildNumber buildNumber, int timeoutMilliseconds = 5000)
		{
			try
			{
				var task = documentStore.AsyncDatabaseCommands.GetBuildNumberAsync();
				var success = task.Wait(timeoutMilliseconds);
				buildNumber = task.Result;
				return success;
			}
			catch
			{
				buildNumber = null;
				return false;
			}
		}

		/// <summary>
		/// Checks to see if the RavenDB Server associated with the document store is accessible.
		/// </summary>
		/// <param name="documentStore">The document store.</param>
		/// <param name="timeoutMilliseconds">Timeout in milliseconds.  Defaults to 5 seconds.</param>
		/// <returns>True if the server is online, false otherwise.</returns>
		public static bool IsServerOnline(this IDocumentStore documentStore, int timeoutMilliseconds = 5000)
		{
			BuildNumber buildNumber;
			return documentStore.TryGetServerVersion(out buildNumber, timeoutMilliseconds);
		}
	}
}
