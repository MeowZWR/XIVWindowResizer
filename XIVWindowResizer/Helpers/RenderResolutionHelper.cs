using System.Drawing;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;

namespace XIVWindowResizer.Helpers;

public unsafe class RenderResolutionHelper
{
    public void SetRenderResolution(int width, int height)
    {
        Device* device = Device.Instance();
        device->NewWidth = (uint)width;
        device->NewHeight = (uint)height;
        device->RequestResolutionChange = 1;
    }

    public Size GetRenderResolution()
    {
        Device* device = Device.Instance();
        return new Size((int)device->Width, (int)device->Height);
    }
}
