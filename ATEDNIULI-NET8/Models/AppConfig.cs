namespace ATEDNIULI_NET8
{
    public class AppConfig
    {
        public string AccessKey { get; set; } = string.Empty;
        public ModelPathsConfig ModelPaths { get; set; } = new();
    }

    public class ModelPathsConfig
    {
        public string Whisper { get; set; } = string.Empty;
        public string Vosk { get; set; } = string.Empty;
        public string SileroVAD { get; set; } = string.Empty;
        public string Intent { get; set; } = string.Empty;
        public string ShapePredictor { get; set; } = string.Empty;
    }
}
