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

        public void AddClock(string name, TimeSpan interval, TimeSpan sync)
        {
            var clock = new ClockConfig
                        {
                            Name = name,
                            IntervalTicks = interval.Ticks,
                            SyncTicks = sync.Ticks
                        };

            Clocks.Add(clock);
        }

        public class ClockConfig
        {
            public string Name { get; set; }
            public long IntervalTicks { get; set; }
            public long SyncTicks { get; set; }

            public void Update(string name, TimeSpan interval, TimeSpan sync)
            {
                Name = name;
                IntervalTicks = interval.Ticks;
                SyncTicks = sync.Ticks;
            }
        }
    }
}
