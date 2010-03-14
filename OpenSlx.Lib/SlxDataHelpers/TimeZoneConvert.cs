using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Application;

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Strategic Sales Systems

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
