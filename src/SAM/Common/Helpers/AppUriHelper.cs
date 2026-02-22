using System;
using Wpf.Ui.Controls;

namespace SAM;

public static class AppUriHelper
{
    private const string URI_SCHEME = @"sam";

    private static readonly Uri baseUri = new ($"{URI_SCHEME}:");

    /// <summary>
    /// Attempts to convert an <see cref="Uri"/> formatted string to command line arguments.
    /// </summary>
    /// <param name="text">The <see langword="string"/> value to parse.</param>
    /// <param name="args">The result, if successful.</param>
    /// <returns><see langword="true"/> if <param name="text"></param> was a valid <see cref="Uri"/>.</returns>
    public static bool TryParseUri(string text, out string[] args)
    {
        args = [];

        if (string.IsNullOrEmpty(text)) return false;

        if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
        {
            return false;
        }

        var uri = new Uri(text, UriKind.Absolute);

        // checks to make sure this is the correct scheme (e.g. sam:/...)
        if (!baseUri.IsBaseOf(uri))
        {
            var message = $"Invalid uri scheme '{uri.Scheme}' (expected '{URI_SCHEME}').";
            throw new ArgumentException(message);
        }

        var path = uri.PathAndQuery;
        args = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return true;
    }

    /// <summary>
    /// Converts any valid <see cref="Uri"/> <see langword="string"/>s to command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments used to start the application.</param>
    /// <returns>The <see cref="Uri.Segments"/> as a <see langword="string[]"/>.</returns>
    public static string[] ParseArguments(string[] args)
    {
        if (args == null) return null;
        if (args.Length == 0) return args;

        foreach (var arg in args)
        {
            if (!Uri.IsWellFormedUriString(arg, UriKind.Absolute))
            {
                continue;
            }

            var uri = new Uri(arg, UriKind.Absolute);

            // checks to make sure this is the correct scheme (e.g. sam:/...)
            if (!baseUri.IsBaseOf(uri))
            {
                continue;
            }
            
            var path = uri.PathAndQuery;
            var parts = path.Split('/',  StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts;
        }

        return args;
    }
}
