# KiepAgendaViewer
### A C# WPF application for displaying a fullscreen Google Calendar proxied through ASP
This application is a fullscreen UI to read Google Calendar, for visual impaired. It is proxied through an [ASP.NET website](KiepAgendaProxy/), so the content can be optimized and adjusted without redeploying the application. It is similar to the [KiepVisitorRegistration](https://github.com/Joozt/KiepVisitorRegistration) project.

Features:
- Show ASP content fullscreen
- Pass command line option `-day` to ASP request string
- Cache ASP content for offline use
- Page split with `<page-break>` tag in ASP content
- Divide in blocks with `<new-block>` tag in ASP content
- Next page or exit with mouse click, numpad `+` or numpad `/`

This application uses a [low level keyboard hook](LowLevelKeyboardHook.cs), in order to catch the key presses even if another application (like [Tobii Dynavox Communicator](http://www.tobiidynavox.com/)) is preventing the keys to reach the application.
