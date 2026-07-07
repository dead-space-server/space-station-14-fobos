using System;
using System.Collections.Generic;
using Content.Server.CrashReporting;
using Robust.Server;

namespace Content.Server
{
    internal static class Program
    {
        private const string NetReceiveBufferSize = "net.receivebuffersize";
        private const string NetSendBufferSize = "net.sendbuffersize";
        private const string NetSocketBufferSize = "16777216";

        public static void Main(string[] args)
        {
            CrashReportWriter.Install(args);

            try
            {
                ContentStart.Start(ApplyServerCVarDefaults(args));
            }
            catch (Exception e)
            {
                CrashReportWriter.Write("ContentStart.Start threw an unhandled exception.", e);
                throw;
            }
        }

        private static string[] ApplyServerCVarDefaults(string[] args)
        {
            var result = new List<string>(args);

            AddCVarDefault(result, args, NetReceiveBufferSize, NetSocketBufferSize);
            AddCVarDefault(result, args, NetSendBufferSize, NetSocketBufferSize);

            return result.ToArray();
        }

        private static void AddCVarDefault(List<string> result, IReadOnlyList<string> args, string name, string value)
        {
            if (HasCVar(args, name))
                return;

            result.Add("--cvar");
            result.Add($"{name}={value}");
        }

        private static bool HasCVar(IReadOnlyList<string> args, string name)
        {
            for (var i = 0; i < args.Count - 1; i++)
            {
                if (args[i] != "--cvar")
                    continue;

                var cvar = args[i + 1];
                var equals = cvar.IndexOf('=');

                if (equals > 0 &&
                    cvar.AsSpan(0, equals).Equals(name.AsSpan(), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
