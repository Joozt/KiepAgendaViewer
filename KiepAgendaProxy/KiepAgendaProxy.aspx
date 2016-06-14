<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="KiepAgendaProxy.aspx.cs" Inherits="KiepAgendaProxy.KiepAgendaProxy" %>

<%
    // TODO Insert URL to private Google Calendar feed, go to Google Calendar settings -> choose calendar -> copy private address ical
    const string url = "https://www.google.com/calendar/ical/<my-gmail-address>/private-<my-private-url>/basic.ics";

    try
    {
        string day = "";
        if (Request.QueryString["day"] != null)
        {
            day = Request.QueryString["day"];
        }
        Response.Write(KiepAgendaProxy.KiepAgendaProxy.getDayEvents(url, day));
    }
    catch (Exception) { }
%>