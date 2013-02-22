using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Raven.Contrib.Analyzers.StandardNoStops
{
	/// <summary>
	/// This implements the Lucene StandardAnalyzer, however no Stop Words.
	/// Use this in cases where you need all the functionality of the StandardAnalyzer
	/// however you do not want anything disregarded (stop words) in indexing.
	/// </summary>
	public class StandardNoStopsAnalyzer : StandardAnalyzer
	{
		public static readonly ISet<string> StopWordsSet;

		public StandardNoStopsAnalyzer( Version matchVersion ) : base( matchVersion, StopWordsSet ) { }
		public StandardNoStopsAnalyzer( Version matchVersion, ISet<string> stopWords ) : base( matchVersion, stopWords ) { }
		public StandardNoStopsAnalyzer( Version matchVersion, FileInfo stopwords ) : base( matchVersion, stopwords ) { }
		public StandardNoStopsAnalyzer( Version matchVersion, TextReader stopwords ) : base( matchVersion, stopwords ) { }

		static StandardNoStopsAnalyzer() { StopWordsSet = new HashSet<string>(); }
	}
}
