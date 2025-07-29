using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Whisper.net;

namespace ATEDNIULI_NET8.Services
{
    public class WhisperService
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveFileWriter;
        private string _outputFilePath = "output.wav";

        private WhisperFactory _whisperFactory;
        private WhisperProcessor _whisperProcessor;

        private readonly int _sampleRate = 16000; // Default sample rate for WhisperSS

        // TODO: implement sileroVAD sunod
        private readonly SileroVADService? _sileroVADService;

        private System.Timers.Timer? _recordingTimer;
        private readonly int _recordingDurationMs = 5000; // 5 seconds

        // Mga events
        public event Action? DoneTranscription;
        public event Action? ProcessingTranscription;
        public event Action? DoneProcessing;

        // store the transctription result here
        public event Action<string>? TranscriptionResultReady;

        public WhisperService(string whisperModelPath, string sileroVADModelPath)
        {
            // This section creates the whisperFactory object which is used to create the processor object.
            _whisperFactory = WhisperFactory.FromPath(whisperModelPath);

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            _whisperProcessor = _whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            _sileroVADService = new SileroVADService(sileroVADModelPath);
        }

        private async void processAudioFile()
        {
            using var fileStream = File.OpenRead(_outputFilePath);

            // gingamit an application invoke chuchu kay aadi bakround thread(async) gin i-invoke an mga events
            Application.Current.Dispatcher.Invoke(() => ProcessingTranscription?.Invoke());

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in _whisperProcessor.ProcessAsync(fileStream))
            {
                Debug.WriteLine($"{result.Start}->{result.End}: {result.Text}");

                TranscriptionResultReady?.Invoke(result.Text);
            }

            Application.Current.Dispatcher.Invoke(() => DoneProcessing?.Invoke());
        }

        public int RecordAudioInput()
        {
            try
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(_sampleRate, 1)
                };

                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.RecordingStopped += OnRecordingStopped;

                _waveFileWriter = new WaveFileWriter(_outputFilePath, _waveIn.WaveFormat);

                _waveIn.StartRecording();

                // an timer
                // Start timer to stop recording after fixed duration
                _recordingTimer = new System.Timers.Timer(_recordingDurationMs);
                _recordingTimer.Elapsed += (_, _) =>
                {
                    _recordingTimer?.Stop();
                    StopRecording();
                };
                _recordingTimer.AutoReset = false;
                _recordingTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording audio input: {ex.Message}");
                return 1;
            }

            return 0;
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_waveIn == null) return;

            _waveFileWriter?.Write(e.Buffer, 0, e.BytesRecorded);
            _waveFileWriter?.Flush();
        }

        // pan dispose an mga resources
        public void StopRecording()
        {
            _waveIn?.StopRecording();

            // invoke done transcription event
            DoneTranscription?.Invoke();
        }

        // cleanup sheets
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _waveFileWriter?.Dispose();
            _waveFileWriter = null;

            _waveIn?.Dispose();
            _waveIn = null;

            if (e.Exception != null)
            {
                Debug.WriteLine("Recording stopped due to an error: " + e.Exception.Message);
            }

            // Process the recorded audio file with Whisper
            processAudioFile();
        }
    }
}
