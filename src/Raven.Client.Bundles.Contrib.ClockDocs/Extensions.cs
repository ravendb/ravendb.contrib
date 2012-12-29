using System;
using System.Linq;

namespace Raven.Client.Bundles.ClockDocs
{
    public static class Extensions
    {
        /// <summary>
        /// Configures a clock document, using the specified name and interval.
        /// </summary>
        /// <param name="advancedSession">The advanced session, obtained from session.Advanced.</param>
        /// <param name="name">The name of the clock.</param>
        /// <param name="interval">The interval to update the clock.</param>
        public static void ConfigureClockDoc(this ISyncAdvancedSessionOperation advancedSession, string name, TimeSpan interval)
        {
            advancedSession.ConfigureClockDoc(name, interval, TimeSpan.Zero);
        }

        /// <summary>
        /// Configures a clock document, using the specified name, interval, and sync period.
        /// </summary>
        /// <param name="advancedSession">The advanced session, obtained from session.Advanced.</param>
        /// <param name="name">The name of the clock.</param>
        /// <param name="interval">The interval to update the clock.</param>
        /// <param name="offset">The offset to align the clock to.  For example "run every hour, on the *half* hour", interval=1hour, offset=30min.</param>
        public static void ConfigureClockDoc(this ISyncAdvancedSessionOperation advancedSession, string name, TimeSpan interval, TimeSpan offset)
        {
            if (interval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("interval", @"The interval cannot be negative.");

            var session = (IDocumentSession) advancedSession;

            var config = session.Load<ClocksConfig>(ClocksConfig.Id);
            if (config == null)
            {
                config = new ClocksConfig();
                config.AddClock(name, interval, offset);
                session.Store(config, ClocksConfig.Id);
            }

            var clock = config.Clocks.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (clock != null)
            {
                clock.Update(name, interval, offset);
                return;
            }
            
            config.AddClock(name, interval, offset);
        }
    }
}
