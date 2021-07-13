using System;
using System.Collections.Generic;
using System.Drawing;
using AutomaticDataGeneration.Darknet.Model;

namespace AutomaticDataGeneration.Darknet
{
    public interface IDarknet : IDisposable
    {
        List<Detection> Detect(IntPtr image, Rectangle region);
        List<Detection> Detect(byte[] image);
    }
}