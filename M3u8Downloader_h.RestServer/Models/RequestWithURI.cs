﻿using M3u8Downloader_H.Abstractions.Common;
using M3u8Downloader_H.Common.DownloadPrams;
using M3u8Downloader_H.RestServer.Attributes;
using System.Text.Json.Serialization;

namespace M3u8Downloader_H.RestServer.Models
{
    internal class RequestWithURI : RequestBase
    {
        [JsonPropertyName("url")]
        [Required(ExceptionMsg = "url不能为空")]
        public Uri RequestUrl { get; set; } = default!;

        [Contained(["AES-128", "AES-192", "AES-256"], ExceptionMsg = "不可用的key方法,必须是AES-128,AES-192,AES-256其中之一")]
        public string Method { get; set; } = "AES-128";
        public string? Key { get; set; }
        public string? Iv { get; set; }

        public IM3u8DownloadParam ToM3u8DownloadParams()
            => new M3u8DownloadParams(RequestUrl, VideoName, SavePath, "mp4", Headers, Method, Key, Iv);
    }
}
