using System;

namespace Raven.DBUtil
{
	public static class HelpText
	{
		public static void Output()
		{
			Console.WriteLine();
			Console.WriteLine("Database Utility for RavenDB");
			Console.WriteLine("RavenDBUtil [/U <url>] <command> [options]");
			Console.WriteLine();
			Console.WriteLine("  /U - Specifies the URL to the RavenDB server.");
			Console.WriteLine("       The default is http://localhost:8080");
			Console.WriteLine();
			Console.WriteLine("Available Commands:");
			Console.WriteLine();
			Console.WriteLine("  TOUCH <dbname> - Touches the named database.");
			Console.WriteLine("  TOUCH <dbname> <dbname> <dbname3 - Touches multiple named databases.");
			Console.WriteLine("  TOUCH /ALL - Touches all databases.");
			Console.WriteLine();
		}
	}
}
