﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Common info to be logged during the startup of a process or AppDomain. And the date format prefers ISO8601.
    /// </summary>
    internal static class StartupInfo
    {
        /// <summary>
        /// To be used to write the first trace when a process starts, with info of the process signature.
        /// </summary>
        /// <param name="basicMessage"></param>
        public static void WriteLine(string basicMessage)
        {
            Trace.TraceInformation(GetMessageWithProcessSignature(basicMessage));
        }

        /// <summary>
        /// Message suffixed with process info: machine name, user name, process name along with arguments, app domain name and local time.
        /// </summary>
        /// <param name="basicMessage"></param>
        /// <returns></returns>
        /// <remarks>Though a trace option or format might give a time stamp prefix to a message, StartupInfo enforces the info about local time in ISO8601 format.</remarks>
        public static string GetMessageWithProcessSignature(string basicMessage)
        {
            return String.Format("{0} -- Machine: {1}; User: {2}/{3}; Process: {4}; AppDomain: {5} Local Time: {6}.",
                basicMessage,
                Environment.MachineName,
                Environment.UserDomainName,
                Environment.UserName,
                Environment.CommandLine,
                AppDomain.CurrentDomain.ToString(),
                GetISO8601Text(DateTime.Now));
        }

        /// <summary>
        /// StartupParagraph plus basic message.
        /// </summary>
        /// <param name="basicMessage"></param>
        /// <returns></returns>
        public static string GetParagraph(string basicMessage)
        {
            return String.Format("{0}+Message:\n{1}", Paragraph, basicMessage);
        }

        /// <summary>
        /// Text lines presenting time, machine name, user name, process name and app domain.
        /// </summary>
        public static string Paragraph
        {
            get
            {
                return String.Format("Time        : {0}\n" +
                                     "Machine     : {1}\n" +
                                     "User        : {2}\\{3}\n" +
                                     "Process     : {4}\n" +
                                     "AppDomain   : {5}\n",
                    NowText,
                    Environment.MachineName,
                    Environment.UserDomainName,
                    Environment.UserName,
                    Environment.CommandLine,
                    AppDomain.CurrentDomain.ToString()
                    );
            }
        }

        public static string GetISO8601Text(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string NowShotText
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd HHmmss");
            }
        }

        /// <summary>
        /// Now in ISO8601 
        /// </summary>
        public static string NowText
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

    }



    internal static class MailMessageHelper
    {
        internal static string ExtractSubject(string message)
        {
            Regex regex = new Regex(@"((\d{1,4}[\:\-\s/]){2,3}){1,2}");//timestamp in trace
            Match match = regex.Match(message);
            if (match.Success)
            {
                message = message.Substring(match.Length);//so remove the timestamp
            }

            string[] ss = message.Split(new string[] { ";", ", ", ". " }, 2, StringSplitOptions.None);
            return StartupInfo.GetMessageWithProcessSignature(ss[0]);
        }

        internal static string SanitiseSubject(string subject)
        {
            const int subjectMaxLength = 254; //though .NET lib does not place any restriction, and the recent standard of Email seems to be 254, which sounds safe.
            if (subject.Length > 254)
                subject = subject.Substring(0, subjectMaxLength);

            try
            {
                for (int i = 0; i < subject.Length; i++)
                {
                    if (Char.IsControl(subject[i]))
                    {
                        return subject.Substring(0, i);
                    }
                }
                return subject;

            }
            catch (ArgumentException)
            {
                return "Invalid subject removed by TraceListener";
            }

        }

        /// <summary>
        /// Email subject generated by other part of the systems might contain invalid characters, or be too long. This function 
        /// will clean up.
        /// </summary>
        /// <param name="mailMessage"></param>
        /// <param name="subject"></param>
        internal static void SanitiseEmailSubject(MailMessage mailMessage, string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return;

            if (mailMessage == null)
                throw new ArgumentNullException("mailMessage");

            mailMessage.Subject = SanitiseSubject(subject);
        }

    }


}
