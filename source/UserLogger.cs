using System;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Logger for user-facing diagnostics. Unlike Logger.cs, this is not DEBUG-only.
    /// </summary>
    public static class UserLogger
    {
        private const string Prefix = "[CheatMenu] ";

        public static void Message(string message)
        {
            Log.Message(Prefix + message);
        }

        public static void Warning(string message)
        {
            Log.Warning(Prefix + message);
        }

        public static void Error(string message)
        {
            Log.Error(Prefix + message);
        }

        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            string prefix = string.IsNullOrWhiteSpace(context) ? Prefix : Prefix + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
