using System;
using System.Collections.Generic;

#if CLIENT
namespace Raven.Client.Bundles.ClockDocs
#else
namespace Raven.Bundles.ClockDocs
#endif
{
    public class ClocksConfig
    {
        public const string Id = "Raven/Clocks/Config";
        
        public IList<ClockConfig> Clocks { get; set; }

        public ClocksConfig()
        {
            Clocks = new List<ClockConfig>();
        }

        public void AddClock(string name, TimeSpan interval, TimeSpan offset)
        {
            var clock = new ClockConfig
                        {
                            Name = name,
                            IntervalTicks = interval.Ticks,
                            OffsetTicks = offset.Ticks
                        };

            Clocks.Add(clock);
        }

        public class ClockConfig
        {
            public string Name { get; set; }
            public long IntervalTicks { get; set; }
            public long OffsetTicks { get; set; }

            public void Update(string name, TimeSpan interval, TimeSpan offset)
            {
                Name = name;
                IntervalTicks = interval.Ticks;
                OffsetTicks = offset.Ticks;
            }
        }
    }
}
