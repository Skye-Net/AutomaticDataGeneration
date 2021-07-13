namespace AutomaticDataGeneration.Config
{
    public class PreviewConfig
    {
        public bool EnablePreview { get; set; }
        public bool ShowAnnotations { get; set; }
        public bool ShowConfidence { get; set; }

        public PreviewConfig()
        {
            EnablePreview = false;
            ShowAnnotations = false;
            ShowConfidence = false;
        }
        
        public PreviewConfig(bool enablePreview, bool showAnnotations, bool showConfidence)
        {
            EnablePreview = enablePreview;
            ShowAnnotations = showAnnotations;
            ShowConfidence = showConfidence;
        }
    }
}