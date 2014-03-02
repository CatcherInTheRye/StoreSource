using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace StoreLib.Modules.Application
{
    /// <summary>
    /// This class Provides logger subsystem
    /// </summary>
    /// 
    public class Logger
    {
        private static readonly string InnerXmlFileName = AppDomain.CurrentDomain.BaseDirectory + "Pool\\Log\\XmlLogInner.txt";

        #region Variable definitions

        private static Mutex _Mutex = new Mutex();
        private static Mutex _HtmlFileMutex = new Mutex();
        private static Mutex _XmlFileMutex = new Mutex();
        private static Mutex _innerXmlTextFileMutex = new Mutex();
        private static DateTime _LastBackupDate = DateTime.Now;

        #endregion

        #region Enums

        public enum Category
        {
            Exception = 2,
            Warning = 4,
            Info = 8,
            Verbose = 16
        }

        #endregion

        #region Events

        public static event ExceptionRaisedEventHandler ExceptionRaisedEvent;

        protected static void OnExceptionRaised(LoggerEventArgs e)
        {
            if (ExceptionRaisedEvent != null)
                ExceptionRaisedEvent(e);
        }

        #endregion

        #region Constructor
        public Logger() { }
        #endregion

        #region Additional routines


        public static FileInfo LogFile
        {
            get { return new FileInfo(InnerXmlFileName); }
        }

        public static void CheckDirectory(string _FileName)
        {
            string _DirectoryName = Path.GetDirectoryName(_FileName);
            if (!Directory.Exists(_DirectoryName))
            {
                Directory.CreateDirectory(_DirectoryName);
            }
        }

        #endregion

        #region Public routines for message(warnings, infos, verboses) logging

        /// <summary>
        /// Public routines for message(warnings, infos, verboses) logging
        /// </summary>
        public static void LogException(Exception _ex)
        {
            LogException(string.Empty, _ex);
        }
        /// <summary>
        /// Public routines for message(warnings, infos, verboses) logging
        /// </summary>
        public static void LogException(string pComment, Exception _ex)
        {
            if (_ex.InnerException == null)
            {
                LogMessage(string.Format("{3}\nIn {0} application exception {1} occured.\nStackTrace:\n{2}", _ex.Source, _ex.Message, _ex.ToString(), pComment),
                  Category.Exception);
                OnExceptionRaised(new LoggerEventArgs(string.Format("{3}\nIn {0} application exception {1} occured.\nStackTrace:\n{2}", _ex.Source, _ex.Message, _ex.ToString(), pComment)));
            }
            else
            {
                LogMessage(string.Format("{2}\nIn {0} application exception {1} occured.\nStackTrace:\n{3}", _ex.InnerException.Source, _ex.InnerException.Message, pComment, _ex.InnerException.StackTrace),
          Category.Exception);
                OnExceptionRaised(new LoggerEventArgs(string.Format("{3}\nIn {0} application exception {1} occured.\nStackTrace:\n{2}", _ex.Source, _ex.Message, _ex.ToString(), pComment)));
            }
        }

        public static void LogException(string _Str)
        {
            LogMessage(_Str, Category.Exception);
            OnExceptionRaised(new LoggerEventArgs(_Str));
        }

        public static void LogWarning(string _Str)
        {
            LogMessage(_Str, Category.Warning);
        }

        public static void LogInfo(string _Str)
        {
            LogMessage(_Str, Category.Info);
        }

        public static void LogVerbose(string _Str)
        {
            LogMessage(_Str, Category.Verbose);
        }

        #endregion

        #region Public routines for message(warnings, infos, verboses) custom logging

        /// <summary>
        /// Public routines for message(warnings, infos, verboses) custom logging
        /// </summary>
        public static void LogException(string _Str, bool _LogToEmail, bool _LogToEventlog, bool _LogToFile)
        {
            Category _Category = Category.Exception;
            LogMessageFile(_Str, _Category);
        }

        public static void LogWarning(string _Str, bool _LogToEmail, bool _LogToEventlog, bool _LogToFile)
        {
            Category _Category = Category.Warning;
            LogMessageFile(_Str, _Category);
        }

        public static void LogInfo(string _Str, bool _LogToEmail, bool _LogToEventlog, bool _LogToFile)
        {
            Category _Category = Category.Info;
            LogMessageFile(_Str, _Category);
        }

        public static void LogVerbose(string _Str, bool _LogToEmail, bool _LogToEventlog, bool _LogToFile)
        {
            Category _Category = Category.Verbose;
            LogMessageFile(_Str, _Category);
        }

        #endregion

        #region Routines for message logging

        /// <summary>
        /// Routines for message(warnings, infos, verboses) logging
        /// </summary>
        private static void LogMessage(string _Str, Category _Category)
        {
            LogMessageFile(_Str, _Category);
            _LastBackupDate = DateTime.Now;
        }

        private static void LogMessageFile(string _Str, Category _Category)
        {
            CheckDirectory(InnerXmlFileName);
            _innerXmlTextFileMutex.WaitOne();
            StreamWriter _SW;

            if (_LastBackupDate.Day != DateTime.Now.Day)
            {
                if (LogFile.Exists)
                {
                    File.Move(LogFile.FullName, LogFile.FullName.Replace(".txt", string.Format("{0}.{1}.{2}.txt", _LastBackupDate.Day, _LastBackupDate.Month, _LastBackupDate.Year)));
                }
            }
            try
            {
                if (!LogFile.Exists)
                {
                    _SW = LogFile.CreateText();
                }
                else
                {
                    _SW = LogFile.AppendText();
                }
                _SW.WriteLine("<Log>");
                _SW.WriteLine(string.Format("\t<Category>{0}</Category>", _Category));
                _SW.WriteLine(string.Format("\t<DateTime>{0}</DateTime>", DateTime.Now));
                _SW.WriteLine("\t<Message><![CDATA[");
                _SW.WriteLine("\t" + _Str);
                _SW.WriteLine("\t]]></Message>");
                _SW.WriteLine("</Log>");
                _SW.Flush();
                _SW.Close();
            }
            catch
            {
            }

            _innerXmlTextFileMutex.ReleaseMutex();
        }

        #endregion

        #region Routines for error logging

        /// <summary>
        /// Routines for error logging
        /// </summary>
        public static void LogError(XmlDataDocument _XmlDataDocument)
        {
            _Mutex.WaitOne();
            LogErrorXml(_XmlDataDocument);
            LogErrorHtml(_XmlDataDocument);
            LogErrorFile(_XmlDataDocument);
            _Mutex.ReleaseMutex();

        }


        private static void LogErrorXml(XmlDataDocument _XmlDataDocument)
        {
            _XmlFileMutex.WaitOne();

            string m_LogName = InnerXmlFileName.Replace(".txt", DateTime.Now.Ticks.ToString()) + ".html";
            CheckDirectory(m_LogName);
            XmlTextWriter xmlWriter = new XmlTextWriter(m_LogName, Encoding.ASCII);
            xmlWriter.Formatting = Formatting.Indented;
            _XmlDataDocument.WriteTo(xmlWriter);
            xmlWriter.Flush();
            xmlWriter.Close();
            _XmlFileMutex.ReleaseMutex();
        }


        private static void LogErrorHtml(XmlDataDocument _XmlDataDocument)
        {
            _HtmlFileMutex.WaitOne();
            XslTransform _XslTransform = new XslTransform();
            _XslTransform.Load(AppDomain.CurrentDomain.BaseDirectory + "/Log/ReportTemplate.xslt");
            XPathNavigator _Navigator = _XmlDataDocument.CreateNavigator();
            string m_LogName = InnerXmlFileName.Replace(".txt", DateTime.Now.Ticks.ToString()) + ".html";
            CheckDirectory(m_LogName);
            StreamWriter _SW = new StreamWriter(m_LogName);
            _XslTransform.Transform(_Navigator, null, _SW, null);
            _SW.Flush();
            _SW.Close();
            _HtmlFileMutex.ReleaseMutex();
        }


        private static void LogErrorFile(XmlDataDocument _XmlDataDocument)
        {
            CheckDirectory(InnerXmlFileName);
            _innerXmlTextFileMutex.WaitOne();
            FileInfo _LogFile = new FileInfo(InnerXmlFileName);
            StreamWriter _SW;
            if (!_LogFile.Exists)
            {
                _SW = _LogFile.CreateText();
            }
            else
            {
                _SW = _LogFile.AppendText();
            }
            _SW.WriteLine("<Log>");
            _SW.WriteLine("\t<Category>");
            _SW.WriteLine("\tError");
            _SW.WriteLine("\t</Category>");
            _SW.WriteLine("\t<DateTime>");
            _SW.WriteLine("\t" + DateTime.Now.ToString());
            _SW.WriteLine("\t</DateTime>");
            _SW.WriteLine("\t<Message>");
            XPathNavigator navigator = _XmlDataDocument.CreateNavigator();
            navigator.MoveToFirst();
            navigator.MoveToFirstChild();
            navigator.MoveToFirstChild();
            navigator.MoveToFirstChild();
            //Format :
            //Application name
            //Desription
            //Details
            //LineNumber
            //SourceLine
            for (int i = 0; i < 5; i++)
            {
                _SW.WriteLine("\t\t<" + navigator.LocalName + ">");
                navigator.MoveToFirstChild();
                _SW.WriteLine("\t\t" + navigator.Value);
                navigator.MoveToParent();
                _SW.WriteLine("\t\t</" + navigator.LocalName + ">");
                navigator.MoveToNext();
            }
            _SW.WriteLine("\t</Message>");
            _SW.WriteLine("</Log>");
            _SW.Flush();
            _SW.Close();
            _innerXmlTextFileMutex.ReleaseMutex();
        }

        private static void LogErrorEventlog(XmlDataDocument _XmlDataDocument)
        {
            if (!EventLog.SourceExists("NetVerk.fo"))
            {
                EventLog.CreateEventSource("NetVerk.fo", "Application");
            }
            EventLog myLog = new EventLog();
            myLog.Source = "NetVerk.fo";
            string _ErrorString = "Error:";
            XPathNavigator navigator = _XmlDataDocument.CreateNavigator();
            navigator.MoveToFirst();
            navigator.MoveToFirstChild();
            navigator.MoveToFirstChild();
            navigator.MoveToFirstChild();
            //Format :
            //Application name, Desription 
            for (int i = 0; i < 2; i++)
            {
                _ErrorString += navigator.LocalName + " - ";
                navigator.MoveToFirstChild();
                _ErrorString += navigator.Value + "   ";
                navigator.MoveToParent();
                navigator.MoveToNext();
            }

            myLog.WriteEntry(_ErrorString);
        }


        private static void LogErrorDb(XmlDataDocument _XmlDataDocument)
        {
            // insert code here
        }

        #endregion
    }
}
