using Microsoft.ML;
using Microsoft.ML.Data;
using System.Windows.Forms;

namespace ATEDNIULI_NET8.Services
{
    public class IntentService
    {
        // Mga instances
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<InputData, IntentPrediction> _predictor;

        public IntentService(string intentModelPath)
        {
            _mlContext = new MLContext();

            var loaded = _mlContext.Model.Load(intentModelPath, out var schema);
            _predictor = _mlContext.Model.CreatePredictionEngine<InputData, IntentPrediction>(_mlContext.Model.Load(intentModelPath, out _));
        }

        public string PredictIntent(string inputText)
        {
            
            var result = _predictor.Predict(new InputData { Text = inputText });

            return result.PredictedIntent;
        }
    }

    public class InputData
    {
        [LoadColumn(0)]
        public string Text { get; set; }

        [LoadColumn(1)]
        public string Intent { get; set; }
    }

    public class IntentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedIntent;
    }
}
