using NAudio.Wave;
using Pv;

namespace ATEDNIULI_NET8.Services
{
    public class PorcupineService
    {
        // Mga instances
        private readonly Porcupine? _porcupine;
        private WaveInEvent? _waveIn;

        // Mga audio stream stuff
        private readonly List<short> _buffer = new List<short>();
        private readonly int _sampleRate;
        private readonly int _frameLength;

        // Mga flags
        private bool _isPaused;

        // Mga events
        public event Action? WakeWordDetected;

        // Para usa la an thread na gamiton para memory safe
        // tbh di ko sure paano ini pero nayakan mas gucci adi
        private readonly object _bufferLock = new();

        public PorcupineService(string accessKey)
        {
            _porcupine = Porcupine.FromBuiltInKeywords(
                accessKey,
                new List<BuiltInKeyword> { BuiltInKeyword.COMPUTER });

            // mas better ig base ha porcupine defaults kaysa ig paagi pa class arguments
            _sampleRate = _porcupine.SampleRate;
            _frameLength = _porcupine.FrameLength;

            _isPaused = false;

            SetupAudioInput();
            StartWakeWordDetection();
        }

        // Pan start
        public void StartWakeWordDetection()
        {
            if (_waveIn != null) _waveIn.StartRecording();
        }

        // Pan stop. pero for cleanup purposes la ini
        public void StopWakeWordDetection()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
                _buffer.Clear();
            }
        }

        // Para ma stop an processing while not reinitializing an mga instances
        public void PauseWakeWordDetection()
        {
            _isPaused = true;
        }

        public void ResumeWakeWordDetection()
        {
            _isPaused = false;
        }

        // Setup the mic for audio streaming
        // Set this up later as a reusable module
        private int SetupAudioInput()
        {
            try
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(_sampleRate, 1)
                };

                _waveIn.DataAvailable += OnDataAvailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up audio input: {ex.Message}");
                return 1;
            }

            return 0;
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_isPaused) return;

            try
            {
                short[] pcm = new short[e.BytesRecorded / 2];
                Buffer.BlockCopy(e.Buffer, 0, pcm, 0, e.BytesRecorded);

                lock (_bufferLock)
                {
                    _buffer.AddRange(pcm);

                    while (_buffer.Count >= _frameLength)
                    {
                        short[] frame = _buffer.Take(_frameLength).ToArray();
                        _buffer.RemoveRange(0, _frameLength);

                        int result = _porcupine.Process(frame);

                        if (result >= 0)
                        {
                            Console.WriteLine(result);
                            WakeWordDetected?.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing audio data: {ex.Message}");
            }
        }
    }
}
