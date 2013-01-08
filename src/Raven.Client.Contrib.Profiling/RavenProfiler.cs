using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace Raven.Client.Contrib.Profiling
{
    public static class RavenProfiler
    {
        private static readonly MethodInfo ElapsedTicks;

        static RavenProfiler()
        {
            ElapsedTicks = typeof(MiniProfiler).GetProperty("ElapsedTicks", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(nonPublic: true);
        }

        public static void InitializeFor(DocumentStore store)
        {
            if (store != null && store.JsonRequestFactory != null)
                store.JsonRequestFactory.LogRequest += (sender, r) => IncludeTiming(JsonFormatter.FormatRequest(r));
        }

        private static void IncludeTiming(RequestResultArgs request)
        {
            if (MiniProfiler.Current == null)
                return;

            var elapsedTicks       = (long)ElapsedTicks.Invoke(MiniProfiler.Current, null);
            var elapsedSeconds     = elapsedTicks / (double)Stopwatch.Frequency;
            var profilingStartedAt = DateTime.UtcNow.AddSeconds(-elapsedSeconds);

#pragma warning disable 612,618
            var secs = (request.At - profilingStartedAt).TotalSeconds;
            var timing = new SqlTiming // The default constructor is obsolete. We know about that; this is a hack anyway.
            {
                Id                             = Guid.NewGuid(),
                CommandString                  = FormatQuery(request.Url),
                StartMilliseconds              = (decimal)secs * 1000,
                DurationMilliseconds           = (decimal)request.DurationMilliseconds,
                FirstFetchDurationMilliseconds = (decimal)request.DurationMilliseconds,
                ExecuteType                    = ToExecuteType(request.Status),
            };
#pragma warning restore 612,618

            MiniProfiler.Current.Head.AddSqlTiming(timing);
            MiniProfiler.Current.HasSqlTimings = true;
        }

        private static ExecuteType ToExecuteType(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.ErrorOnServer:
                    return ExecuteType.None;
                    
                case RequestStatus.Cached:
                    return ExecuteType.Scalar;

                case RequestStatus.SentToServer:
                    return ExecuteType.Reader;

                case RequestStatus.AggressivelyCached:
                    return ExecuteType.NonQuery;

                default:
                    return ExecuteType.None;
            }
        }

        private static string FormatQuery(string url)
        {
            var results = url.Split('?');

			if (results.Length > 1)
			{
				string[] items = results[1].Split('&');
                string query   = String.Join("\r\n", items).Trim();

                var match = Regex.Match(results[0], @"/indexes/[A-Za-z/]+");
                if (match.Success)
                {
                    string index = match.Value.Replace("/indexes/", "");

                    if (!String.IsNullOrEmpty(index))
                        query = String.Format("index={0}\r\n", index) + query;
                }

                return Uri.UnescapeDataString(Uri.UnescapeDataString(query));
			}

            return String.Empty;
        }
    }
}
