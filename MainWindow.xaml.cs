using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Net;
using System.IO;
using System.Timers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Collections.Generic;

namespace KiepAgendaViewer
{
    public partial class MainWindow : Window
    {
        private const string CACHEFILE = "Agenda";
        private const string LOGFILE = "KiepAgendaViewer.log";
        private int[] CATCH_KEYCODES = { 107, 111 };
        private string day = "";

#if !DEBUG
        // TODO Change to your website running the ASP proxy
        private const string URL = "http://localhost:57705/KiepAgendaProxy.aspx";
#else
        private const string URL = "http://localhost:57705/KiepAgendaProxy.aspx";
#endif

        private delegate void DummyDelegate();

        private int currentPage = 1;
        private int numberOfPages = 1;
        private string currentText = "";

        public MainWindow()
        {
            InitializeComponent();

            // Cannot debug when application has topmost
#if !DEBUG
            this.Topmost = true;
#endif

            // Wait 1 second before subscribing to keypresses
            WaitSubscribeKeypresses();

            // Log startup
            Log("Start");

            // Apply rotation animation to status image
            DoubleAnimation da = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(3)));
            RotateTransform rt = new RotateTransform();
            imgStatus.RenderTransform = rt;
            imgStatus.RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);

            // Read command line
            ReadCommandline();

            // Start reading
            ShowPage(TryReadFromFile(), currentPage);
            TryReadFromWeb();
        }

        private void WaitSubscribeKeypresses()
        {
            try
            {
                Timer timer = new Timer(1000);
                timer.Elapsed += delegate
                {
                    this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (DummyDelegate)
                    delegate 
                    {
                        timer.Enabled = false;

                        // Block keys from being received by other applications
                        List<int> blockedKeys = new List<int>(CATCH_KEYCODES);
                        LowLevelKeyboardHook.Instance.SetBlockedKeys(blockedKeys);

                        // Subscribe to low level keypress events
                        LowLevelKeyboardHook.Instance.KeyboardHookEvent += new LowLevelKeyboardHook.KeyboardHookEventHandler(Instance_KeyboardHookEvent);
                    });
                };
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Log("Error attaching keyboard hook\t" + ex.Message);
            }
        }

        private void ReadCommandline()
        {
#if !DEBUG
            try
            {
#endif
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                switch (commandLineArgs[i])
                {
                    case "-day":
                        i++;
                        if (commandLineArgs.Length > i)
                        {
                            try
                            {
                                day = commandLineArgs[i];
                            }
                            catch (Exception) { }
                        }
                        break;

                    default:
                        // Unknown argument: do nothing
                        break;
                }
            }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Log("Error reading command line\t" + ex.Message);
            }
#endif
        }

        void Instance_KeyboardHookEvent(int keycode)
        {
            if (new List<int>(CATCH_KEYCODES).Contains(keycode))
            {
                Log("Keypress\t" + keycode);
                Click();
            }
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            Log("MouseClick");
            Click();
        }

        private void Click()
        {
            currentPage++;
            if (currentPage > numberOfPages)
            {
                Log("Exit");
                Application.Current.Shutdown();
            }
            else
            {
                Log("Next page\t" + currentPage);
                ShowPage(currentText, currentPage);
            }
        }

        private void ShowPage(string text, int page)
        {
            currentText = text;
            currentPage = page;
            
            string[] pages = Regex.Split(text, "\r\n<page-break>\r\n");

            numberOfPages = pages.Length;

            if (currentPage > numberOfPages)
            {
                currentPage = numberOfPages;
            }

            ShowText(pages[currentPage - 1]);
        }

        private void ShowText(string text)
        {
            // Clear screen
            spViewer.Children.Clear();

            string[] textblocks = Regex.Split(text, "\r\n<new-block>\r\n");
            int i = 0;
            foreach (string textblock in textblocks)
            {
               

                TextBox tbTextBlock = new TextBox();
                tbTextBlock.PreviewMouseDown += MouseDownHandler;
                tbTextBlock.IsReadOnly = true;
                tbTextBlock.BorderThickness = new Thickness(0);
                tbTextBlock.Foreground = Brushes.Black;
                tbTextBlock.Text = textblock;
                
                if (i == 0)
                {
                    tbTextBlock.TextAlignment = TextAlignment.Center;
                }

                if (i % 2 == 0)
                {
                    tbTextBlock.Background = Brushes.Transparent;
                }
                else
                {
                    tbTextBlock.Background = Brushes.LightGray;
                }
                i++;


                // Add text block to screen
                spViewer.Children.Add(tbTextBlock);
            }
        }

        private string TryReadFromFile()
        {
            string result = "";
            try
            {
                string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string filenameandpath = baseDir + "\\" + CACHEFILE;
                if (day != "")
                {
                    filenameandpath += "-" + day;
                }
                filenameandpath += ".txt";
                if (File.Exists(filenameandpath))
                {
                    StreamReader cachefile = File.OpenText(filenameandpath);
                    result = cachefile.ReadToEnd();
                    cachefile.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error reading from file\t" + ex.Message);
            }

            return result;
        }

        private void TryReadFromWeb()
        {
            try
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                string url = URL;
                if (day != "")
                {
                    url += "?day=" + day;
                }
                bw.RunWorkerAsync(url);
            }
            catch (Exception ex)
            {
                Log("Error running download thread\t" + ex.Message);
            }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            if (result != "")
            {
                ShowPage(result, currentPage);
                WriteToFile(result);
                imgStatus.Source = new BitmapImage(new Uri("Ok.png", UriKind.Relative));
                imgStatus.RenderTransform = null;
            }
            else
            {
                imgStatus.Source = new BitmapImage(new Uri("Error.png", UriKind.Relative));
                imgStatus.RenderTransform = null;
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = "";
            try 
            {
                string url = (string)e.Argument;
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Proxy = null;
                webRequest.Timeout = 5000;
                Stream responseStream = webRequest.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();
                e.Result = responseFromServer;	
            }
            catch (Exception ex)
            {
                Log("Error downloading URL\t" + ex.Message);
            }
        }

        private void WriteToFile(string text)
        {
            try
            {
                if (text != "")
                {
                    string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    string filenameandpath = baseDir + "\\" + CACHEFILE;
                    if (day != "")
                    {
                        filenameandpath += "-" + day;
                    }
                    filenameandpath += ".txt";
                    StreamWriter cachefile = File.CreateText(filenameandpath);
                    cachefile.Write(text);
                    cachefile.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error writing to file\t" + ex.Message);
            }
        }

        private void Log(string text)
        {
            try
            {
                if (text != "")
                {
                    string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    StreamWriter cachefile = File.AppendText(baseDir + "\\" + LOGFILE);
                    cachefile.WriteLine(DateTime.Now + "\t" + text);
                    cachefile.Close();
                }
            }
            catch (Exception) { }
        }
    }
}
