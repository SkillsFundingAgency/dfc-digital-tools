﻿using DFC.Digital.Tools.Data.Interfaces;
using NLog;
using System;

namespace DFC.Digital.Tools.Core
{
    public class DFCLogger : IApplicationLogger
    {
        private readonly ILogger logService;

        public DFCLogger(ILogger logService)
        {
            this.logService = logService;
        }

        public void Error(string message, Exception ex)
        {
            if (ex is LoggedException)
            {
                throw ex;
            }
            else
            {
                logService.Error(ex, message);
                if (ex != null)
                {
                    throw new LoggedException($"Logged exception with message: {message}", ex);
                }
            }
        }

        public void ErrorJustLogIt(string message, Exception ex)
        {
            logService.Log(LogLevel.Error, ex, message);
        }

        public void Info(string message)
        {
            logService.Info(message);
        }

        public void Trace(string message)
        {
            logService.Trace(message);
        }

        public void Warn(string message, Exception ex)
        {
            if (ex != null)
            {
                if (ex is LoggedException)
                {
                    throw ex;
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = ex.Message;
                }

                if (message.Contains("An exception of type 'DFC.Integration.AVFeed.Core.Logging.LoggedException'"))
                {
                    //This is an application exception of known type.
                    //This exception has already been logged by the application, hence can be ignored from sitefinity logs.
                }
                else
                {
                    logService.Warn(ex, message);
                        throw new LoggedException($"Logged exception with message: {message}", ex);
                }
            }
        }
    }
}
