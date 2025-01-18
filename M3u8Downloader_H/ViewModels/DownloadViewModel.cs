﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Caliburn.Micro;
using M3u8Downloader_H.Services;
using M3u8Downloader_H.Utils;
using M3u8Downloader_H.Models;
using System.Text;
using System.Linq;
using System.Timers;
using M3u8Downloader_H.Abstractions.Common;
using Newtonsoft.Json.Linq;

namespace M3u8Downloader_H.ViewModels
{
    public abstract  partial  class DownloadViewModel(SettingsService settingsService, SoundService soundService) : PropertyChangedBase, Abstractions.Common.ILog
    {
        private readonly ThrottlingSemaphore throttlingSemaphore = ThrottlingSemaphore.Instance;
        private CancellationTokenSource? cancellationTokenSource;
        protected IDownloadParamBase DownloadParam = default!;
        protected DownloadProgress? downloadProgress;

        public BindableCollection<LogParams> Logs { get; } = [];
        public Uri RequestUrl { get; set; } = default!;

        public string VideoName { get; set; } = default!;

        public double ProgressNum { get; set; }

        public long DownloadRateBytes { get; set; } = -1;

        public bool IsActive { get; private set; }

        public DownloadStatus Status { get; set; }

        public bool IsProgressIndeterminate => IsActive && Status < DownloadStatus.StartedVod;

        public string? FailReason { get; private set; } = string.Empty;

        protected abstract Task StartDownload(CancellationToken cancellationToken);

        public bool CanOnStart => !IsActive;

        public void OnStart()
        {
            if (!CanOnStart)
                return;

            IsActive = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    Status = DownloadStatus.Enqueued;
                    cancellationTokenSource = new CancellationTokenSource();
                    using var semaphore = await throttlingSemaphore.AcquireAsync(cancellationTokenSource.Token);

                    await StartDownload(cancellationTokenSource.Token);
                    soundService.PlaySuccess(settingsService.IsPlaySound);
                    Status = DownloadStatus.Completed;
                }
                catch (OperationCanceledException) when (cancellationTokenSource!.IsCancellationRequested)
                {
                    Status = DownloadStatus.Canceled;
                    Info("已经停止下载");
                }
                catch (Exception e)
                {
                    soundService.PlayError(settingsService.IsPlaySound);
                    Status = DownloadStatus.Failed;
                    FailReason = e.ToString();
                    Error(e);
                }
                finally
                {
                    IsActive = false;
                    downloadProgress?.Clear();
                    cancellationTokenSource?.Dispose();
                }

            });
        }

        public bool CanOnShowFile => Status == DownloadStatus.Completed;
        public virtual void OnShowFile()
        {
            if (!CanOnShowFile)
                return;

            try
            {
                Process.Start("explorer", $"/select, \"{Path.Combine(DownloadParam.SavePath, DownloadParam.VideoFullName)}\"");
            }
            catch (Exception)
            {
            }
        }


        public bool CanOnCancel => IsActive && Status != DownloadStatus.Canceled;
        public void OnCancel()
        {
            if (!CanOnCancel)
                return;

            cancellationTokenSource?.Cancel();
        }

        public bool CanOnRestart => CanOnStart && Status != DownloadStatus.Completed;

        public void OnRestart() => OnStart();


        public void DeleteCache()
        {
            string cachePath = Path.Combine(DownloadParam.SavePath, DownloadParam.VideoName);
            DirectoryInfo directory = new(cachePath);
            if (directory.Exists)
                directory.Delete(true);
        }

        public void Info(string format, params object[] args)
        {
            Logs.Add(new LogParams(LogType.Info, string.Format(format, args)));
        }

        public void Warn(string format, params object[] args)
        {
            Logs.Add(new LogParams(LogType.Warning, string.Format(format, args)));
        }

        public void Error(Exception exception)
        {
            Logs.Add(new LogParams(LogType.Error, exception.Message));
        }

        public string CopyLog()
        {
            StringBuilder sb = new();
            foreach (var log in Logs.ToArray())
            {
                sb.Append(log.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append(' ');
                sb.Append(log.Message);
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }


    }

    public partial class DownloadViewModel : IEquatable<DownloadViewModel>
    {
        
        public bool Equals(DownloadViewModel? other) => GetHashCode() == other?.GetHashCode();
        public override bool Equals(object? obj) =>  obj is DownloadViewModel  downloadviewmodel && Equals(downloadviewmodel);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(DownloadParam.SavePath + VideoName);
        public static bool operator ==(DownloadViewModel downloadviewmode, DownloadViewModel downloadviewmode1) => downloadviewmode.Equals(downloadviewmode1);
        public static bool operator !=(DownloadViewModel downloadviewmode, DownloadViewModel downloadviewmode1) => !(downloadviewmode == downloadviewmode1);
    }


    public partial class DownloadViewModel
    {
        protected class DownloadProgress(DownloadViewModel downloadViewModel) : IDialogProgress
        {
            private readonly TimerContainer timerContainer = TimerContainer.Instance;
            private long _lastBytes;
            private long _countBytes;
            private double _currentProgress;

            public void Clear()
            {
                _countBytes = 0;
                _lastBytes = 0;
                _currentProgress = 0;
                downloadViewModel.DownloadRateBytes = -1;
            }

            private void OnTimedEvent(Object? source, ElapsedEventArgs e)
            {
                long tmpBytes = _countBytes;
                downloadViewModel.DownloadRateBytes = tmpBytes - _lastBytes;
                _lastBytes = tmpBytes;
                downloadViewModel.ProgressNum = _currentProgress;
            }

            public IDisposable Acquire() => timerContainer.AddTimerCallback(OnTimedEvent);

            public void Report(long value)
            {
                Interlocked.Add(ref _countBytes, value);
            }

            public void Report(double value)
            {
                _currentProgress = value;
            }

            public void SetDownloadStatus(bool IsLiveDownloading)
            {
                downloadViewModel.Status = IsLiveDownloading ? DownloadStatus.StartedLive : DownloadStatus.StartedVod;
            }
        }
    }

   

}
