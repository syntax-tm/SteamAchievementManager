using System.ComponentModel;
using System.Windows;

namespace SAM;

/// <summary>
/// Used during inter-process communication via named pipes
/// </summary>
[DefaultValue(None)]
public enum ApplicationMessage
{
    /// <summary>
    /// Message failed due to an unexpected error.
    /// </summary>
    Error = -1,
    /// <summary>
    /// Used for testing purposes only.
    /// </summary>
    None = 0,
    /// <summary>
    /// Activate the <see cref="Application.MainWindow"/>.
    /// </summary>
    Activate = 1,
    /// <summary>
    /// Exit the application.
    /// </summary>
    Exit = 2
}
