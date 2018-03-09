using System.Diagnostics;

namespace KiepAgendaProxy
{
    class AgendaDownloaderTest
    {
        //const string url = "https://www.google.com/calendar/ical/kiepklaassen%40gmail.com/private-8461c5a5abfda32ee4f1289bde190ccf/basic.ics";
        const string url = "https://calendar.google.com/calendar/ical/mail%40joozt.nl/private-fa1672e148e765e5ac6da5f2465f5f6a/basic.ics";

        static void Main(string[] args)
        {
            Debug.WriteLine(AgendaDownloader.getDayEvents(url, "mon"));
        }
    }
}
