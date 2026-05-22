using System;

namespace Suity.Editor.AIGC;

public static class DisplayFormatter
{
    public static string GetFileSizeDisplay(long sizeInBytes)
    {
        if (sizeInBytes < 1024)
        {
            return $"{sizeInBytes} Bytes";
        }
        else if (sizeInBytes < 1024 * 1024)
        {
            return $"{sizeInBytes / 1024} KB";
        }
        else if (sizeInBytes < 1024 * 1024 * 1024)
        {
            return $"{sizeInBytes / (1024 * 1024)} MB";
        }
        else if (sizeInBytes < 1024L * 1024 * 1024 * 1024)
        {
            return $"{sizeInBytes / (1024 * 1024 * 1024)} GB";
        }
        else
        {
            return $"{sizeInBytes / (1024L * 1024 * 1024 * 1024)} TB";
        }
    }
}