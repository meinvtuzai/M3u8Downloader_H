﻿using M3u8Downloader_H.Abstractions.Common;
using M3u8Downloader_H.Abstractions.M3u8;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace M3u8Downloader_H.M3U8.M3UFileReaderManangers
{
    public interface IM3UFileInfoMananger
    {
        ILog? Log { set; }
        TimeSpan TimeOuts { set; }

        //通过http方式请求数据
        Task<IM3uFileInfo> GetM3u8FileInfo(Uri uri, IEnumerable<KeyValuePair<string, string>>? headers, CancellationToken cancellationToken = default);

        //通过传入本地文件如xml,json,m3u8,文件夹等方式请求数据
        IM3uFileInfo GetM3u8FileInfo(string ext, Uri uri);
    }
}
