using System;

#if CLIENT
namespace Raven.Client.Bundles.ClockDocs
#else
namespace Raven.Bundles.ClockDocs
#endif
{
    public class Clock
    {
        public const string IdBase = "Raven/Clocks/";
      
        public DateTime UtcTime { get; set; }
        public DateTimeOffset ServerLocalTime { get; set; }
        public string ServerTimeZone { get; set; }
    }
}
