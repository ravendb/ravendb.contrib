using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Connection;

namespace Raven.DBUtil
{
	public static class Extensions
	{
		public static IEnumerable<string> GetAllDatabaseNames(this IDocumentStore documentStore)
		{
			const int pageSize = 1024;
			int start = 0;

			while (true)
			{
				var results = documentStore.DatabaseCommands.GetDatabaseNames(pageSize, start);

				foreach (var result in results)
					yield return result;

				if (results.Length < pageSize)
					break;

				start += pageSize;
			}
		}

		public static bool DatabaseExists(this IDocumentStore documentStore, string databaseName)
		{
			var key = "Raven/Databases/" + databaseName;
			return documentStore.DatabaseCommands.ForDefaultDatabase().DocumentExists(key);
		}

		public static bool DocumentExists(this IDatabaseCommands databaseCommands, string key)
		{
			var metadata = databaseCommands.Head(key);
			return metadata != null;
		}
	}
}
