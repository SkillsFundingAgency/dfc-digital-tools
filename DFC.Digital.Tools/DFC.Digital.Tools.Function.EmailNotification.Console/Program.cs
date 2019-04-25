using DFC.Digital.Tools.Core;
using System;
using System.Reflection;
using ConsoleOld = System.Console;

namespace DFC.Digital.Tools.Function.EmailNotification.Console
{
    internal class Program
    {
        private static void Main()
        {
            ConsoleOld.WriteLine("=================THE BEGINNING==============");
            try
            {
                Startup.RunAsync(RunMode.Console).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    for (int ii = 0; ii < loaderExceptions.Length; ii++)
                    {
                        ConsoleOld.WriteLine(loaderExceptions[ii]);
                    }
                }
                else
                {
                    ConsoleOld.WriteLine(ex);
                }
            }
            finally
            {
                ConsoleOld.WriteLine("=================THE END==============");
                ConsoleOld.ReadLine();
            }
        }
    }
}
