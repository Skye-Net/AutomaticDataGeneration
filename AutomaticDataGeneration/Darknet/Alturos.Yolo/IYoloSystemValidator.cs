#region

using AutomaticDataGeneration.Darknet.Alturos.Yolo.Model;

#endregion

namespace AutomaticDataGeneration.Darknet.Alturos.Yolo
{
    public interface IYoloSystemValidator
    {
        SystemValidationReport Validate();
    }
}