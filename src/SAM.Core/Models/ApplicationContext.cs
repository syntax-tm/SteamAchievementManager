using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SAM.Core;

public class ApplicationContext
{
    private static readonly Lock syncLock = new ();

    public bool IsUwpApp { get; set; }

    public static ApplicationContext Default
    {
        get
        {
            if (field != null) return field;

            lock (syncLock)
            {
                field = new ();
            }

            return field;
        }
    }

    public Version Version
    {
        get
        {
            if (field != null) return field;

            field = Assembly.GetEntryAssembly()?.GetName().Version;

            return field;
        }
    }
}
