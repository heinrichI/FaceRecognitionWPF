namespace FaceRecognitionBusinessLogic
{
    public interface IConfiguration
    {
        string TrainPath { get; set; }
        string SearchPath { get; set; }
        string ModelsDirectory { get; set; }
        double DistanceThreshold { get; set; }
        int ThreadCount { get; set; }

        void Save();
    }
}