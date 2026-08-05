using System;
using SAM.Core;

namespace SAM;

public interface IOptionUri
{
    ApplicationMode Mode { get; }

    string[] GetArgs();
    Uri ToUri();
}
