using System.Diagnostics;

namespace monogame_project;

public static class DebugTools
{
    public static void WriteLine(object? value)
    {
        Debug.WriteLine(value);
    }
}