using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;

namespace Raven.DBUtil
{
	internal class Program
	{
		internal static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				HelpText.Output();
				return (int) ExitCode.InvalidCommand;
			}

			try
			{
				var argsQueue = new Queue<string>(args);
				var nextArg = argsQueue.Dequeue();

				// get the url from the command line
				string url = "http://localhost:8080";
				if (nextArg.ToUpper() == "/U")
				{
					nextArg = argsQueue.Dequeue();
					if (!nextArg.StartsWith("http", true, null))
					{
						HelpText.Output();
						return (int) ExitCode.InvalidCommand;
					}

					url = nextArg;
					nextArg = argsQueue.Dequeue();
				}

				// get a document store to work with
				var documentStore = GetDocumentStore(url);

				// handle each command
				switch (nextArg.ToUpper())
				{
					case "TOUCH":
						if (argsQueue.Count == 0)
							goto default;

						// get the database names from the command line
						var dbNames = argsQueue.ToList();

						// if we specified all databases with all -all or /all then touch all databases
						if (dbNames.Any(x => x.Equals("/ALL", StringComparison.OrdinalIgnoreCase)))
						{
							Toucher.TouchAllDatabases(documentStore);
							return (int) ExitCode.Success;
						}

						// touch just the databases we specified
						Toucher.TouchDatabases(documentStore, dbNames);
						return (int) ExitCode.Success;

					default:
						HelpText.Output();
						return (int) ExitCode.InvalidCommand;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex.Message);
				return (int) ExitCode.Failure;
			}
		}

		private static IDocumentStore GetDocumentStore(string url)
		{
			var documentStore = new DocumentStore { Url = url };
			documentStore.Initialize();
			return documentStore;
		}

		internal enum ExitCode
		{
			Success = 0,
			InvalidCommand = 1,
			Failure = 2
		}
	}
}
