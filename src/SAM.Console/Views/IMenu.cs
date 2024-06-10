using System.Collections.Generic;
using Spectre.Console;

namespace SAM.Console.Views;

public interface IMenu<T>
{
    string Title { get; set; }
    List<IMenu<T>> Children { get; set; }
}

public interface IMenuItem
{
    string Name { get; set; }
}
