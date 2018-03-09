using DDay.iCal;
using System;
using System.Collections.Generic;
using System.Text;

namespace KiepAgendaProxy
{
    public partial class KiepAgendaProxy : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public static string getDayEvents(string url, string day)
        {
            IICalendar calendar = iCalendar.LoadFromUri(new Uri(url))[0];
            int daysToAdd;
            switch (day)
            {
                case "mon":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Monday);
                    iCalDateTime nextMonday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextMonday);
                case "tue":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Tuesday);
                    iCalDateTime nextTuesday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextTuesday);
                case "wed":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Wednesday);
                    iCalDateTime nextWednesday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextWednesday);
                case "thu":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Thursday);
                    iCalDateTime nextThursday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextThursday);
                case "fri":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Friday);
                    iCalDateTime nextFriday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextFriday);
                case "sat":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Saturday);
                    iCalDateTime nextSaturday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextSaturday);
                case "sun":
                    daysToAdd = DaysToAdd(iCalDateTime.Today.DayOfWeek, DayOfWeek.Sunday);
                    iCalDateTime nextSunday = (iCalDateTime)iCalDateTime.Today.AddDays(daysToAdd);
                    return getDayEvents(calendar, nextSunday);
                default:
                    return getDayEvents(calendar, iCalDateTime.Today);
            }
        }

        private static string getDayEvents(IICalendar calendar, iCalDateTime day)
        {
            StringBuilder result = new StringBuilder(day.ToString("D").ToUpper());
            result.AppendLine();

            int eventCounter = 0;
            bool hasEndTimes = false;
            Occurrence previous;
            IList<Occurrence> occurrences = calendar.GetOccurrences<IEvent>(day);
            foreach (Occurrence occurrence in occurrences)
            {
                IEvent evt = occurrence.Source as IEvent;
                if (evt != null)
                {
                    if (evt.IsActive())
                    {
                        result.AppendLine("<new-block>");
                        if (evt.IsAllDay)
                        {
                            result.Append("all day");
                        }
                        else
                        {
                            if (previous.Period != null && occurrence.Period.StartTime.Equals(previous.Period.StartTime) && occurrence.Period.EndTime.Equals(previous.Period.EndTime))
                            {
                                result.Append("\t");
                            }
                            else
                            {
                                if (occurrence.Period.Duration.Equals(new TimeSpan(0)))
                                {
                                    result.Append(occurrence.Period.StartTime.Value.ToLocalTime().ToString("t"));
                                    result.Append("\t");
                                }
                                else
                                {
                                    hasEndTimes = true;
                                    result.Append(occurrence.Period.StartTime.Value.ToLocalTime().ToString("t"));
                                    result.Append(" - ");
                                    result.Append(occurrence.Period.EndTime.Value.ToLocalTime().ToString("t"));
                                }
                            }
                        }
                        result.Append("\t");
                        result.AppendLine(evt.Summary);
                        previous = occurrence;
                        eventCounter++;
                    }
                }
            }

            if (!hasEndTimes)
            {
                result.Replace("\t\t", "\t");
            }

            if (eventCounter == 0)
            {
                result.AppendLine("<new-block>");
                result.AppendLine("<no appointments>");
            }
            result.Remove(result.Length - 2, 2);
            result.Replace("\\", "");
            return result.ToString();
        }

        private static int DaysToAdd(DayOfWeek current, DayOfWeek desired)
        {
            int currentInt = (int)current;
            int desiredInt = (int)desired;

            int result = (desiredInt - currentInt);
            if (result < 0) result += 7;

            return result;
        }
    }
}