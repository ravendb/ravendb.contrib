using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Abstractions;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Database;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace Raven.Bundles.ClockDocs
{
    public class ClockDocsMaintainer : IStartupTask, IDisposable
    {
        private readonly IList<Timer> _timers = new List<Timer>();
        private DocumentDatabase _database;

        public void Dispose()
        {
            ClearTimers();
        }

        public void Execute(DocumentDatabase database)
        {
            _database = database;

            var configDoc = _database.Get(ClocksConfig.Id, null);
            if (configDoc == null)
                return;

            var config = configDoc.DataAsJson.JsonDeserialization<ClocksConfig>();
            Configure(config);
        }

        private void ClearTimers()
        {
            foreach (var timer in _timers)
                timer.Dispose();
            _timers.Clear();
        }

        internal void Configure(ClocksConfig config)
        {
            // kill any existing timers
            ClearTimers();

            // Always update the clocks on startup or configuration change.
            // The system may have been down, or configuration may be different now.
            foreach (var clockConfig in config.Clocks)
            {
                var state = clockConfig;
                Task.Factory.StartNew(() => TimerElapsed(state));
            }

            // add new timers from the config
            foreach (var clockConfig in config.Clocks)
            {
                // Determine the delay before starting the timer.  It should line up with the interval.
                TimeSpan delay;
                var ticks = clockConfig.IntervalTicks;
                if (ticks == 0)
                {
                    delay = TimeSpan.Zero;
                }
                else
                {
                    // round now to interval
                    var now = SystemTime.UtcNow;
                    var nextStart = ((now.Ticks + ticks) / ticks) * ticks;
                    delay = new TimeSpan(nextStart - now.Ticks);
                    if (delay < TimeSpan.Zero)
                        delay = TimeSpan.Zero;
                }

                // create and add the timer
                var interval = new TimeSpan(clockConfig.IntervalTicks);
                var timer = new Timer(TimerElapsed, clockConfig, delay, interval);
                _timers.Add(timer);
            }
        }

        private void TimerElapsed(object state)
        {
            var config = (ClocksConfig.ClockConfig) state;

            var utcTime = SystemTime.UtcNow;

            // round now to interval
            var ticks = config.IntervalTicks;
            if (ticks > 0)
                utcTime = new DateTime((utcTime.Ticks / ticks) * ticks, DateTimeKind.Utc);

            // apply any offset specified
            utcTime = utcTime.AddTicks(config.OffsetTicks);

            var clock = new Clock
                        {
                            UtcTime = utcTime,
                            ServerLocalTime = new DateTimeOffset(utcTime).ToLocalTime(),
                            ServerTimeZone = TimeZoneInfo.Local.Id,
                        };

            var id = Clock.IdBase + config.Name;

            using (_database.DisableAllTriggersForCurrentThread())
            {
                var document = RavenJObject.FromObject(clock);
                var metadata = new RavenJObject { { Constants.NotForReplication, true } };
                _database.Put(id, null, document, metadata, null);
            }
        }
    }
}
