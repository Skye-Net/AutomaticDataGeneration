namespace AutomaticDataGeneration.Darknet.Alturos.Yolo.Model
{
    /// <summary>
    ///     C++ Communication object
    /// </summary>
    internal struct BboxT
    {
        internal uint X, Y, W, H; // (x,y) - top-left corner, (w, h) - width & height of bounded box
        internal float Prob; // confidence - probability that the object was found correctly
        internal uint ObjId; // class of object - from range [0, classes-1]
        internal uint TrackId; // tracking id for video (0 - untracked, 1 - inf - tracked object)
        internal uint FramesCounter;
        internal float X3D, Y3D, Z3D; // 3-D coordinates, if there is used 3D-stereo camera
    }
}