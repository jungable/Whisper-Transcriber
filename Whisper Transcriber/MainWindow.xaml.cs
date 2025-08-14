using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Whisper.net;


namespace WhisperTranscriber
{
    public partial class MainWindow : Window
    {
        // Variables pour la transcription de fichier
        private string _selectedFilePath = string.Empty;

        // Variables pour la transcription en direct
        private bool _isTranscribingLive;
        private WaveInEvent? _waveIn;
        private BufferedWaveProvider? _bufferedWaveProvider;
        private Task? _transcriptionTask;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Logique de Transcription Principale

        private async Task<WhisperProcessor?> CreateProcessorAsync()
        {
            // 1. Déterminer le chemin du modèle à utiliser
            var modelName = GetSelectedModelName();
            if (string.IsNullOrEmpty(modelName)) return null;

            var modelPath = Path.Combine(AppContext.BaseDirectory, modelName);

            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"Le fichier modèle '{modelName}' est introuvable. Veuillez le télécharger et le placer dans le dossier de l'application.", "Erreur de Modèle", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            // 2. Déterminer la langue à utiliser
            var language = GetSelectedLanguage();

            // 3. Créer la factory et le processeur avec les bons paramètres
            var whisperFactory = WhisperFactory.FromPath(modelPath);
            var processor = whisperFactory.CreateBuilder()
                .WithLanguage(language)
                .Build();

            return processor;
        }

        #endregion

        #region Transcription en Direct (Microphone)

        private async void LiveTranscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isTranscribingLive) await StartLiveTranscription();
            else await StopLiveTranscription();
        }

        private async Task StartLiveTranscription()
        {
            _isTranscribingLive = true;
            ResultTextBox.Text = string.Empty;
            UpdateUiState();

            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 16, 1) };
            _bufferedWaveProvider = new BufferedWaveProvider(_waveIn.WaveFormat) { DiscardOnBufferOverflow = true };
            _waveIn.DataAvailable += (s, a) => _bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            _waveIn.StartRecording();

            _transcriptionTask = Task.Run(ProcessAudioBuffer);
        }

        private async Task StopLiveTranscription()
        {
            _isTranscribingLive = false;
            if (_transcriptionTask != null) await _transcriptionTask;

            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _waveIn = null;
            UpdateUiState();
        }

        private async Task ProcessAudioBuffer()
        {
            var processor = await Application.Current.Dispatcher.InvokeAsync(CreateProcessorAsync);
            if (processor == null)
            {
                await StopLiveTranscription();
                return;
            }

            using (processor)
            {
                while (_isTranscribingLive)
                {
                    await Task.Delay(2000);
                    if (_bufferedWaveProvider == null || _bufferedWaveProvider.BufferedBytes == 0) continue;

                    var audioBytes = new byte[_bufferedWaveProvider.BufferedBytes];
                    _bufferedWaveProvider.Read(audioBytes, 0, audioBytes.Length);

                    using var memoryStream = new MemoryStream();
                    using (var waveStream = new RawSourceWaveStream(new MemoryStream(audioBytes), _bufferedWaveProvider.WaveFormat))
                    {
                        WaveFileWriter.WriteWavFileToStream(memoryStream, waveStream);
                        memoryStream.Position = 0;
                        await foreach (var segment in (await processor).ProcessAsync(memoryStream))
                        {
                            Application.Current.Dispatcher.Invoke(() => ResultTextBox.Text += segment.Text);
                        }
                    }
                }
            }
        }

        #endregion

        #region Transcription depuis un Fichier

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Choisir un fichier audio",
                Filter = "Fichiers Audio|*.mp3;*.wav;*.m4a;*.flac;*.ogg|Tous les fichiers|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                SelectedFileTextBlock.Text = Path.GetFileName(_selectedFilePath);
                UpdateUiState();
            }
        }

        private async void TranscribeFileButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateUiState(isFileTranscribing: true);
            ResultTextBox.Text = string.Empty;

            try
            {
                using var processor = await CreateProcessorAsync();
                if (processor == null) return; // Erreur gérée dans CreateProcessorAsync

                using var memoryStream = new MemoryStream();
                using var reader = new MediaFoundationReader(_selectedFilePath);
                var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
                var waveFormat = new WaveFormat(16000, 16, 1);
                // Remplacement de WaveFormatConversionStream par un BufferedWaveProvider temporaire
                var resampledProvider = resampler.ToWaveProvider16();
                WaveFileWriter.WriteWavFileToStream(memoryStream, resampledProvider);
                memoryStream.Position = 0;

                var resultBuilder = new StringBuilder();
                await foreach (var segment in processor.ProcessAsync(memoryStream))
                {
                    resultBuilder.AppendLine(segment.Text);
                }
                ResultTextBox.Text = resultBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : \n\n{ex.Message}", "Erreur de Transcription", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateUiState();
            }
        }

        #endregion

        #region Helpers et Gestion UI

        private string GetSelectedModelName()
        {
            var selectedItem = ModelComboBox.SelectedItem as ComboBoxItem;
            return selectedItem?.Content.ToString() switch
            {
                "Base (rapide, précision standard)" => "ggml-base.bin",
                "Small (plus lent, haute précision)" => "ggml-small.bin",
                "Medium (lent, très haute précision)" => "ggml-medium.bin",
                "Tiny (très rapide, basse précision)" => "ggml-tiny.bin",
                _ => string.Empty
            };
        }

        private string GetSelectedLanguage()
        {
            var selectedItem = LanguageComboBox.SelectedItem as ComboBoxItem;
            return selectedItem?.Content.ToString() switch
            {
                "Français" => "fr",
                "English" => "en",
                "Español" => "es",
                "Deutsch" => "de",
                "Italiano" => "it",
                _ => "auto" // Cas par défaut pour "Auto-détection"
            };
        }

        private void UpdateUiState(bool isFileTranscribing = false)
        {
            bool isBusy = _isTranscribingLive || isFileTranscribing;
            LoadingOverlay.Visibility = isFileTranscribing ? Visibility.Visible : Visibility.Collapsed;
            StatusTextBlock.Text = GetStatusText(isFileTranscribing);

            SelectFileButton.IsEnabled = !isBusy;
            TranscribeFileButton.IsEnabled = !isBusy && !string.IsNullOrEmpty(_selectedFilePath);
            LiveTranscribeButton.IsEnabled = !isFileTranscribing;
            LiveTranscribeButton.Content = _isTranscribingLive ? "■ Arrêter la transcription" : "🎤 Démarrer la transcription en direct";

            ModelComboBox.IsEnabled = !isBusy;
            LanguageComboBox.IsEnabled = !isBusy;
        }

        private string GetStatusText(bool isFileTranscribing)
        {
            if (_isTranscribingLive) return "Enregistrement en direct...";
            if (isFileTranscribing) return "Transcription du fichier en cours...";
            return "Prêt.";
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_isTranscribingLive) await StopLiveTranscription();
        }

        #endregion
    }
}