using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;

namespace ATEDNIULI_NET8.Services
{
    public class SileroVADService
    {
        private readonly InferenceSession _session;
        private Tensor<float> _state;

        // necessary constants for the model
        private const int NumLayers = 2;
        private const int BatchSize = 1;
        private const int StateSize = 128;
        public const int ChunkSize = 512;
        private const long SampleRate = 16000;

        public SileroVADService(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Model file not found", modelPath);

            var sessionOptions = new SessionOptions();
            _session = new InferenceSession(modelPath, sessionOptions);

            // Initialize the state tensor with the correct 3D shape [2, 1, 128]
            var stateShape = new int[] { NumLayers, BatchSize, StateSize };
            _state = new DenseTensor<float>(new float[NumLayers * BatchSize * StateSize], stateShape);
        }

        public bool Predict(float[] audioChunk)
        {
            // The model expects a precise chunk size.
            if (audioChunk.Length != ChunkSize)
                throw new ArgumentException($"Audio chunk must have exactly {ChunkSize} samples for 16kHz.");

            var inputTensor = new DenseTensor<float>(audioChunk, new[] { BatchSize, ChunkSize });
            var srTensor = new DenseTensor<long>(new long[] { SampleRate }, ReadOnlySpan<int>.Empty);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor),
                NamedOnnxValue.CreateFromTensor("state", _state),
                NamedOnnxValue.CreateFromTensor("sr", srTensor)
            };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

            var outputTensor = results.First(r => r.Name == "output").AsTensor<float>();
            float speechProbability = outputTensor.GetValue(0);

            _state = results.First(r => r.Name == "stateN").AsTensor<float>();

            return speechProbability > 0.9f;
        }
    }
}
