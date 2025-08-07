using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Vosk;

namespace VoskTest
{
    public class VoskService
    {
        private readonly Model _model;
        public readonly VoskRecognizer _recognizer;

        private WaveInEvent _waveIn = new WaveInEvent();
        private readonly int _sampleRate = 16000;

        // events
        public event Action? DoneTranscription;
        public event Action? ProcessingTranscription;
        public event Action<string>? TranscriptionResultReady;

        public VoskService(string voskModelPath)
        {
            Vosk.Vosk.SetLogLevel(0);

            _model = new Model(voskModelPath);
            _recognizer = new VoskRecognizer(_model, 16000);
        }

        private void SetupAudioStream()
        {
            try
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(_sampleRate, 1),
                    BufferMilliseconds = 1000
                };

                _waveIn.DataAvailable += OnDataAvailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up audio input: {ex.Message}");
            }
        }

        public void StartRecording()
        {
            SetupAudioStream();

            _waveIn.StartRecording();
        }

        public void StopRecording()
        {
            _waveIn.StopRecording();
            DoneTranscription?.Invoke();

            _waveIn?.Dispose();
            _waveIn = null;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                StreamTranscribe(e.Buffer, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing audio data: {ex.Message}");
            }
        }

        public void StreamTranscribe(byte[] buffer, int bytesRecorded)
        {
            try
            {
                if (_recognizer.AcceptWaveform(buffer, bytesRecorded))
                {
                    string rawResult = _recognizer.Result();

                    if (rawResult != null)
                    {
                        var cleanedResult = CleanTranscription(rawResult);

                        TranscriptionResultReady?.Invoke(cleanedResult);

                        StopRecording();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in StreamTranscribe: {ex.Message}");
            }
        }

        public string CleanTranscription(string rawTranscription)
        {
            try
            {
                // Handle multiple JSON objects if Vosk returns concatenated JSON
                var cleanedSegments = new List<string>();
                var parts = rawTranscription.Split(new[] { "}{", "}\n{" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    string jsonPart = part;

                    // Fix malformed JSON due to split
                    if (!jsonPart.StartsWith("{")) jsonPart = "{" + jsonPart;
                    if (!jsonPart.EndsWith("}")) jsonPart += "}";

                    using var doc = JsonDocument.Parse(jsonPart);
                    if (doc.RootElement.TryGetProperty("text", out JsonElement textElement))
                    {
                        string text = textElement.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            cleanedSegments.Add(text.Trim());
                        }
                    }
                }

                return string.Join(" ", cleanedSegments).Trim() + ". ";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CleanTranscription: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
