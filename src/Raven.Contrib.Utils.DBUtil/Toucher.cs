using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Raven.Client;

namespace Raven.DBUtil
{
	public static class Toucher
	{
		private static readonly object Lock = new object();

		public static void TouchAllDatabases(IDocumentStore documentStore)
		{
			var dbNames = documentStore.GetAllDatabaseNames();
			TouchDatabases(documentStore, dbNames);
		}

		public static void TouchDatabases(IDocumentStore documentStore, IEnumerable<string> dbNames)
		{
			Parallel.ForEach(dbNames, dbName => TouchDatabase(documentStore, dbName));
		}

		public static void TouchDatabase(IDocumentStore documentStore, string dbName)
		{
			if (!documentStore.DatabaseExists(dbName))
				throw new ArgumentException("Database \"" + dbName + "\" was not found!");

			var msg = string.Format("Touching database \"{0}\"... ", dbName);
			Console.WriteLine(msg);
			var top = Console.CursorTop - 1;
			var left = msg.Length;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var cmds = documentStore.DatabaseCommands.ForDatabase(dbName);
			cmds.DocumentExists("TOUCH"); // doesn't matter if it exists or not

			stopwatch.Stop();
			msg = string.Format("done. ({0} ms)", stopwatch.ElapsedMilliseconds);
			Console_WriteAt(msg, left, top);
		}

		private static void Console_WriteAt(string str, int left, int top)
		{
			lock (Lock)
			{
				Console.CursorVisible = false;
				var x = Console.CursorLeft;
				var y = Console.CursorTop;
				Console.SetCursorPosition(left, top);
				Console.Write(str);
				Console.SetCursorPosition(x, y);
				Console.CursorVisible = true;
			}
		}
	}
}
