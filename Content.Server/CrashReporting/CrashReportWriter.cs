using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Content.Server.CrashReporting;

internal static class CrashReportWriter
{
    private const string DataDirOption = "--data-dir";
    private const string CrashReportsDirectoryName = "crash-reports";
    private const string LastCrashFileName = "last-crash.log";

    private static readonly DateTimeOffset StartedAt = DateTimeOffset.Now;
    private static string[] _args = [];
    private static int _written;

    public static void Install(string[] args)
    {
        _args = (string[]) args.Clone();
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    public static void Write(string reason, object? exceptionObject)
    {
        if (Interlocked.Exchange(ref _written, 1) != 0)
            return;

        try
        {
            var report = BuildReport(reason, exceptionObject);
            var reportDirectory = GetCrashReportsDirectory(_args);
            Directory.CreateDirectory(reportDirectory);

            var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var reportPath = Path.Combine(reportDirectory, $"crash_{timestamp}.log");
            var lastReportPath = Path.Combine(reportDirectory, LastCrashFileName);

            File.WriteAllText(reportPath, report, Encoding.UTF8);
            File.WriteAllText(lastReportPath, report, Encoding.UTF8);
        }
        catch
        {
            // Process is already crashing. Avoid throwing from the crash reporter.
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Write($"Unhandled exception. IsTerminating={args.IsTerminating}.", args.ExceptionObject);
    }

    private static string BuildReport(string reason, object? exceptionObject)
    {
        var now = DateTimeOffset.Now;
        var dataDirectory = GetDataDirectory(_args);
        var builder = new StringBuilder();

        builder.AppendLine("Dead Space 14 server crash report");
        builder.AppendLine("===============================");
        builder.AppendLine($"Reason: {reason}");
        builder.AppendLine($"Created: {now:O}");
        builder.AppendLine($"Started: {StartedAt:O}");
        builder.AppendLine($"Uptime: {now - StartedAt}");
        builder.AppendLine($"Process ID: {Environment.ProcessId}");
        builder.AppendLine($"Process name: {Process.GetCurrentProcess().ProcessName}");
        builder.AppendLine($"Executable: {Environment.ProcessPath ?? "<unknown>"}");
        builder.AppendLine($"Base directory: {AppContext.BaseDirectory}");
        builder.AppendLine($"Working directory: {Environment.CurrentDirectory}");
        builder.AppendLine($"Data directory: {dataDirectory}");
        builder.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        builder.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"Command args: {FormatArgs(_args)}");
        builder.AppendLine();
        builder.AppendLine("Exception");
        builder.AppendLine("---------");
        builder.AppendLine(FormatException(exceptionObject));

        return builder.ToString();
    }

    private static string GetCrashReportsDirectory(IReadOnlyList<string> args)
    {
        return Path.Combine(GetDataDirectory(args), CrashReportsDirectoryName);
    }

    private static string GetDataDirectory(IReadOnlyList<string> args)
    {
        var dataDirectory = TryReadOption(args, DataDirOption)
                            ?? Path.Combine(Environment.CurrentDirectory, "data");

        return Path.GetFullPath(dataDirectory);
    }

    private static string? TryReadOption(IReadOnlyList<string> args, string optionName)
    {
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg == optionName && i + 1 < args.Count)
                return args[i + 1];

            var prefix = optionName + "=";
            if (arg.StartsWith(prefix, StringComparison.Ordinal))
                return arg[prefix.Length..];
        }

        return null;
    }

    private static string FormatArgs(IReadOnlyList<string> args)
    {
        var result = new List<string>(args.Count);
        var redactNext = false;

        foreach (var arg in args)
        {
            if (redactNext)
            {
                result.Add("<redacted>");
                redactNext = false;
                continue;
            }

            if (IsSensitiveOption(arg))
            {
                result.Add(RedactValue(arg));
                redactNext = !arg.Contains('=', StringComparison.Ordinal);
                continue;
            }

            result.Add(arg);
        }

        return string.Join(' ', result);
    }

    private static bool IsSensitiveOption(string value)
    {
        return value.Contains("token", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("apikey", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("api_key", StringComparison.OrdinalIgnoreCase);
    }

    private static string RedactValue(string value)
    {
        var equals = value.IndexOf('=');
        return equals < 0
            ? value
            : value[..(equals + 1)] + "<redacted>";
    }

    private static string FormatException(object? exceptionObject)
    {
        return exceptionObject switch
        {
            null => "<null>",
            Exception exception => exception.ToString(),
            _ => exceptionObject.ToString() ?? "<unknown>",
        };
    }
}
