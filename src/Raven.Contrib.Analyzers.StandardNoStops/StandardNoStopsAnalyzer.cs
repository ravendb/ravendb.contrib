using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Raven.Contrib.Analyzers.StandardNoStops
{
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
