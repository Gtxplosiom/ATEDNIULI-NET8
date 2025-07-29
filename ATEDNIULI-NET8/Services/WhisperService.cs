using NAudio.Wave;
using System.Diagnostics;
using Whisper;
using System.IO;
using Whisper.net;

namespace ATEDNIULI_NET8.Services
{
    public class WhisperService
    {
        private string _whisperModelPath;

        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveFileWriter;
        private string _outputFilePath = "output.wav";

        private WhisperFactory _whisperFactory;
        private WhisperProcessor _whisperProcessor;

        private readonly int _sampleRate = 16000; // Default sample rate for WhisperSS

        public WhisperService(string whisperModelPath)
        {
            _whisperModelPath = whisperModelPath;

            // This section creates the whisperFactory object which is used to create the processor object.
            _whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            _whisperProcessor = _whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();
        }

        private async void processAudioFile()
        {
            using var fileStream = File.OpenRead(_outputFilePath);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in _whisperProcessor.ProcessAsync(fileStream))
            {
                Debug.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
        }

        private int RecordAudioInput()
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
                Console.WriteLine("Recording stopped due to an error: " + e.Exception.Message);
            }

            // Process the recorded audio file with Whisper
            processAudioFile();
        }
    }
}
