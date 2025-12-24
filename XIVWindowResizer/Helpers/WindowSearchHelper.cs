using System;
using System.Runtime.InteropServices;

namespace XIVWindowResizer.Helpers;

public class WindowSearchHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string? windowTitle);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    public IntPtr FindWindowHandle()
    {
        IntPtr hWnd = IntPtr.Zero;
        int currentPid = Environment.ProcessId;

        while ((hWnd = FindWindowEx(IntPtr.Zero, hWnd, "FFXIVGAME", null)) != IntPtr.Zero)
        {
            _ = GetWindowThreadProcessId(hWnd, out int windowPid);

            if (windowPid == currentPid && IsWindowVisible(hWnd))
            {
                return hWnd;
            }
        }

        return IntPtr.Zero;
    }
}