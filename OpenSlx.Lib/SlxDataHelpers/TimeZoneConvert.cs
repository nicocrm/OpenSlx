using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Application;

namespace OpenSlx.Lib.SlxDataHelpers
{
    /// <summary>
    /// Utility methods for converting timezones.
    /// For web users, the timezone will be stored in the HttpContext.  We can retrieve it via
    /// Sage's ContextService.
    /// For Unit-Test, this can be set explicitely in the suite setup.
    /// </summary>
    public static class TimeZoneConvert
    {
        /// <summary>
        /// Convert the datetime to UTC.
        /// The input datetime will be assumed to be in the local user timezone
        /// (not necessarily the same as the server's).
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Converted to utc (null if input is null)</returns>
        public static DateTime? ToUtc(DateTime? input)
        {
            if (!input.HasValue)
                return null;
            var tz = GetTimeZoneContext();
            DateTime output = tz.LocalDateTimeToUTCTime(input.Value);
            // work around the fact that Sage tz does not set the datetime kind
            return DateTime.SpecifyKind(output, DateTimeKind.Utc);
        }

        /// <summary>
        /// Convert the datetime (assumed to be UTC) to the user's local time zone.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Converted to local time (null if input is null)</returns>
        public static DateTime? ToLocal(DateTime? input)
        {
            if (!input.HasValue)
                return null;
            var tz = GetTimeZoneContext();
            DateTime output = tz.UTCDateTimeToLocalTime(input.Value);
            // work around the fact that Sage tz does not set the datetime kind
            return DateTime.SpecifyKind(output, DateTimeKind.Unspecified);
        }

        private static Sage.Platform.TimeZone GetTimeZoneContext()
        {
            var tz = (Sage.Platform.TimeZone)ApplicationContext.Current.Services.Get<Sage.Platform.Application.IContextService>().GetContext("TimeZone");
            if (tz == null)
            {
                tz = new Sage.Platform.TimeZones().CurrentTimeZone;
                ApplicationContext.Current.Services.Get<Sage.Platform.Application.IContextService>().SetContext("TimeZone", tz);
            }
            return tz;
        }
    }
}
