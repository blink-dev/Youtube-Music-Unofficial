using CSCore;
using CSCore.Codecs;
using CSCore.MediaFoundation;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Youtube_Music.ViewModels;
using Youtube_Music.Commands;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Youtube_Music.Models
{
    public class DownloadItem : BindableBase
    {
        private string _title;
        private int _currentProgress;
        private Geometry _cancelBtnIcon;
        private Brush _indicatorForeground;
        private DelegateCommand _cancelCommand;
        private bool _canCancel;

        private bool CanCancel
        {
            get => _canCancel;
            set => Set(ref _canCancel, value);
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set => Set(ref _currentProgress, value);
        }

        public Geometry CancelBtnIcon
        {
            get => _cancelBtnIcon;
            set => Set(ref _cancelBtnIcon, value);
        }

        public Brush IndicatorForeground
        {
            get => _indicatorForeground;
            set => Set(ref _indicatorForeground, value);
        }

        public DelegateCommand CancelCommand
        {
            get => _cancelCommand;
            set => Set(ref _cancelCommand, value);
        }

        public readonly CancellationTokenSource tokenSource = new();

        public DownloadItem(string title, string fileName, string songUrl)
        {
            CanCancel = true;
            CancelCommand = new DelegateCommand(Cancel, CanCancel);
            CancelBtnIcon = App.Current.Resources["CloseIcon"] as Geometry;
            IndicatorForeground = Brushes.Gray;
            Title = title;
            var filePath = fileName;

            App.Current.MainWindow.Dispatcher.InvokeAsync(async () =>
            {
                YoutubeClient youtubeClient = new();
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(songUrl);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                var source = CodecFactory.Instance.GetCodec(new Uri(streamInfo.Url));
                var encoder = MediaFoundationEncoder.CreateMP3Encoder(source.WaveFormat, filePath);

                CurrentProgress = 0;
                IProgress<long> progress = new Progress<long>(value =>
                {
                    decimal tmp = (decimal)(value * 100) / source.Length;
                    if (tmp != CurrentProgress && tmp > CurrentProgress) CurrentProgress = (int)tmp;
                });
                try
                {

                    await CopyToAsync(source, encoder, progress, source.WaveFormat.BytesPerSecond, tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    source.Dispose();
                    encoder.Dispose();
                    File.Delete($"{filePath}");
                    App.ViewModel.Downloads.DownloadItems.Remove(this);
                    return;
                }
                catch (Exception)
                {
                    File.Delete($"{filePath}");
                    App.ViewModel.Downloads.DownloadItems.Remove(this);
                    return;
                }

                CanCancel = false;
                CurrentProgress = 100;
                IndicatorForeground = Brushes.BlueViolet;
                CancelBtnIcon = App.Current.Resources["FinishIcon"] as Geometry;
                source.Dispose();
                encoder.Dispose();
            });
        }

        private void Cancel()
        {
            tokenSource.Cancel();
        }

        public static async Task CopyToAsync(IWaveSource source, MediaFoundationEncoder destination, IProgress<long> progress, int bufferSize = 0x1000, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                progress.Report(totalRead);
                await Task.Delay(1, cancellationToken);
            }
        }

    }
}
