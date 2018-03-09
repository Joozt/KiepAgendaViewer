using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace KiepAgendaProxy
{
    public class AgendaDownloader
    {
        public static string getDayEvents(string url, string day)
        {
            int daysToAdd;
            switch (day)
            {
                case "mon":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Monday);
                    IDateTime nextMonday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextMonday);
                case "tue":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Tuesday);
                    IDateTime nextTuesday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextTuesday);
                case "wed":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Wednesday);
                    IDateTime nextWednesday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextWednesday);
                case "thu":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Thursday);
                    IDateTime nextThursday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextThursday);
                case "fri":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Friday);
                    IDateTime nextFriday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextFriday);
                case "sat":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Saturday);
                    IDateTime nextSaturday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextSaturday);
                case "sun":
                    daysToAdd = DaysToAdd(CalDateTime.Today.DayOfWeek, DayOfWeek.Sunday);
                    IDateTime nextSunday = CalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(url, nextSunday);
                default:
                    return getDayEvents(url, CalDateTime.Today);
            }
        }

        private static string getDayEvents(string url, IDateTime day)
        {
            StringBuilder result = new StringBuilder(day.ToString("D", System.Globalization.CultureInfo.CurrentCulture).ToUpper());
            result.AppendLine();

            int eventCounter = 0;
            bool hasEndTimes = false;
            Occurrence previous = null;

            Calendar calendar = LoadFromUri(new Uri(url));
            var relevantOccurrences = calendar.GetOccurrences(day).OrderBy(o => o.Period.StartTime).ToList();
            foreach (var occurrence in relevantOccurrences)
            {
                var startTime = occurrence.Period.StartTime;
                var endTime = occurrence.Period.EndTime;
                var evt = (CalendarEvent)occurrence.Source;

                if (evt.IsAllDay || (startTime.Date < DateTime.Now.Date && endTime.Date > DateTime.Now.Date))
                {
                    result.AppendLine("<new-block>");
                    result.Append("Hele dag");
                }
                else
                {
                    if (previous != null && startTime.Equals(previous.Period.StartTime) && evt.End.Equals(previous.Period.EndTime))
                    {
                        result.Append("\t");
                    }
                    else
                    {
                        result.AppendLine("<new-block>");
                        if (evt.Duration.Equals(new TimeSpan(0)))
                        {
                            result.Append(getTimeStringForCurrentTimezone(startTime, day));
                            result.Append("\t");
                        }
                        else
                        {
                            hasEndTimes = true;
                            result.Append(getTimeStringForCurrentTimezone(startTime, day));
                            result.Append(" - ");
                            result.Append(getTimeStringForCurrentTimezone(endTime, day));
                        }
                    }
                }
                result.Append("\t");
                result.AppendLine(evt.Summary);
                previous = occurrence;
                eventCounter++;
            }

            if (!hasEndTimes)
            {
                result.Replace("\t\t", "\t");
            }

            if (eventCounter == 0)
            {
                result.AppendLine("<new-block>");
                result.AppendLine("Geen afspraken");
            }
            result.Remove(result.Length - 2, 2);
            result.Replace("\\", "");
            return result.ToString();
        }

        private static string getTimeStringForCurrentTimezone(IDateTime time, IDateTime compareDay)
        {
            if (time.Date == compareDay.Date.AddDays(-1))
            {
                return "Gisteren";
            }
            if (time.Date == compareDay.Date.AddDays(1))
            {
                return "Morgen";
            }
            if (time.Date == compareDay.Date.AddDays(-2))
            {
                return "Eergisteren";
            }
            if (time.Date == compareDay.Date.AddDays(1))
            {
                return "Overmorgen";
            }
            if (time.Date < compareDay.Date)
            {
                return "Eerder";
            }
            if (time.Date > compareDay.Date)
            {
                return "Later";
            }

            return time.AsSystemLocal.ToString("H:mm");
        }

        private static int DaysToAdd(DayOfWeek current, DayOfWeek desired)
        {
            int currentInt = (int)current;
            int desiredInt = (int)desired;

            int result = (desiredInt - currentInt);
            if (result < 0) result += 7;

            return result;
        }

        private static Calendar LoadFromUri(Uri uri)
        {
            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(uri).Result)
                {
                    response.EnsureSuccessStatusCode();
                    var result = response.Content.ReadAsStringAsync().Result;
                    return Calendar.Load(result);
                }
            }
        }
    }
}