﻿using System;
using Caliburn.Micro;
using M3u8Downloader_H.Common.DownloadPrams;
using M3u8Downloader_H.Services;

namespace M3u8Downloader_H.Models
{
    public class MediaDownloadInfo : PropertyChangedBase
    {
        public string VideoUrl { get; set; } = default!;
        public string? AudioUrl { get; set; } = default!;

        public string? VideoName { get; set; } = default!;

        public int StreamIndex { get; set; } = default!;
        public Action<MediaDownloadParams> NormalProcessDownloadAction { get; set; } = default!;

        public void Reset(bool resetUrl, bool resetName)
        {
            if (resetUrl)
            {
                VideoUrl = string.Empty;
                AudioUrl = string.Empty;
            }
            if (resetName) 
                VideoName = string.Empty;
        }


        public void DoProcess(SettingsService settingsService)
        {

            if (string.IsNullOrWhiteSpace(VideoUrl) && string.IsNullOrWhiteSpace(AudioUrl))
                throw new InvalidOperationException("视频地址和音频地址不能同时为空");

            Uri uri = new(VideoUrl);
            if(uri.IsFile)
                throw new InvalidOperationException("请确认是否输入正确的网络地址");

            Uri? AudioUri = !string.IsNullOrWhiteSpace( AudioUrl) ? new Uri(AudioUrl) : null;
            MediaDownloadParams mediaDownloadParams = new(settingsService.SavePath,new Uri(VideoUrl), AudioUri,  VideoName, settingsService.Headers)
            {
                IsVideoStream = StreamIndex == 0
            };
            NormalProcessDownloadAction(mediaDownloadParams);
        }
    }
}
