// whisper service
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using Whisper.net;

namespace ATEDNIULI_NET8.Services
{
    public class WhisperService
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveFileWriter;
        private readonly string _outputFilePath = "output.wav";

        private readonly WhisperFactory _whisperFactory;
        private readonly WhisperProcessor _whisperProcessor;

        private readonly int _sampleRate = 16000;
        private readonly SileroVADService _sileroVADService;

        // vad timers
        private System.Timers.Timer? _silenceTimer;
        private System.Timers.Timer? _gracePeriodTimer;

        // vad baryabols
        private readonly int _gracePeriodMs = 3000;
        private readonly int _silenceDurationMs = 2000;
        private volatile bool _isGracePeriod;

        // Buffer to hold audio samples for VAD processing
        private readonly List<float> _audioBuffer = new List<float>();

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

            // This section creates the processor object which is used to process the audio file.
            _whisperProcessor = _whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            _sileroVADService = new SileroVADService(sileroVADModelPath);

            // Initialize the timers for VAD logic
            SetupTimers();
        }

        // method to intialize vad timers
        private void SetupTimers()
        {
            _gracePeriodTimer = new System.Timers.Timer(_gracePeriodMs);
            _gracePeriodTimer.Elapsed += OnGracePeriodElapsed;
            _gracePeriodTimer.AutoReset = false;

            _silenceTimer = new System.Timers.Timer(_silenceDurationMs);
            _silenceTimer.Elapsed += OnSilenceTimerElapsed;
            _silenceTimer.AutoReset = false;
        }

        // kun an grace period mag end ma start an silence detection
        // maupay ini na may grace period kay an vad detection na implementation ko currently
        // malako cpu hahahhaha
        private void OnGracePeriodElapsed(object? sender, ElapsedEventArgs e)
        {
            _isGracePeriod = false;
            Debug.WriteLine("Grace period ended. VAD is now active.");
        }

        // kun an silence 2 seconds na ma invoke ini na method
        // ngan ig stop recording which then eventually process the recorded audio
        private void OnSilenceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("Silence detected. Stopping recording.");

            Application.Current.Dispatcher.Invoke(StopRecording);
        }

        private async void processAudioFile()
        {
            using var fileStream = File.OpenRead(_outputFilePath);
            var transcriptionResult = "";

            // gingamit an application invoke chuchu kay aadi bakround thread(async) gin i-invoke an mga events
            Application.Current.Dispatcher.Invoke(() => ProcessingTranscription?.Invoke());

            await foreach (var result in _whisperProcessor.ProcessAsync(fileStream))
            {
                Debug.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                transcriptionResult = result.Text;
            }

            // gawas ha foreach kay para ma prevent an multiple invokes
            TranscriptionResultReady?.Invoke(transcriptionResult);

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

                // reset la an mga states para safe
                _audioBuffer.Clear();
                _isGracePeriod = true;
                _gracePeriodTimer?.Start();
                _silenceTimer?.Stop();

                _waveIn.StartRecording();
                Debug.WriteLine("Recording started. Grace period initiated.");
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
            if (_waveFileWriter == null) return;

            _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveFileWriter.Flush();

            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i);

                _audioBuffer.Add(sample / 32768.0f);
            }

            while (_audioBuffer.Count >= SileroVADService.ChunkSize)
            {
                var chunk = _audioBuffer.GetRange(0, SileroVADService.ChunkSize).ToArray();
                _audioBuffer.RemoveRange(0, SileroVADService.ChunkSize);

                // vad logic
                // this starts when grace period ended
                // only start the vad detection kun an grace period ended
                // napatay kasi cpu lol
                if (!_isGracePeriod)
                {
                    // an vad detection
                    bool isSpeech = _sileroVADService.Predict(chunk);

                    if (isSpeech)
                    {
                        // reset an timer kun may speech na detected
                        if (_silenceTimer is { Enabled: true })
                        {
                            _silenceTimer.Stop();
                            Debug.WriteLine("VAD: Speech detected. Silence timer reset.");
                        }
                    }
                    else
                    {
                        if (_silenceTimer is { Enabled: false })
                        {
                            _silenceTimer.Start();
                            Debug.WriteLine("VAD: Silence detected. Silence timer started.");
                        }
                    }
                }
            }
        }

        public void StopRecording()
        {
            _waveIn?.StopRecording();

            _gracePeriodTimer?.Stop();
            _silenceTimer?.Stop();

            DoneTranscription?.Invoke();
        }

        // cleanup method and triggers the transcription method
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            // Dispose of NAudio resources
            _waveFileWriter?.Dispose();
            _waveFileWriter = null;
            _waveIn?.Dispose();
            _waveIn = null;

            // Dispose of timers and re-initialize them for the next recording session
            _gracePeriodTimer?.Dispose();
            _silenceTimer?.Dispose();
            SetupTimers();

            if (e.Exception != null)
            {
                Debug.WriteLine("Recording stopped due to an error: " + e.Exception.Message);
            }

            // whisper transcription processing
            processAudioFile();
        }
    }
}

// TODO: apply typing logic
// add InitiateTyping in the intent model
// 
