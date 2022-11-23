﻿using M3u8Downloader_H.Common.M3u8Infos;
using M3u8Downloader_H.Common.Utils;
using M3u8Downloader_H.M3U8.AttributeReader.Attributes;
using M3u8Downloader_H.Plugin;

namespace M3u8Downloader_H.M3U8.AttributeReader.AttributeReaders
{
    [M3U8Reader("#EXT-X-VERSION", typeof(VersionAttributeReader))]
    internal class VersionAttributeReader : IAttributeReader
    {
        public bool ShouldTerminate => false;

        public void Write(M3UFileInfo fileInfo,string value, IEnumerator<string> reader, Uri baseUri)
        {
            fileInfo.Version = To.Value<int>(value);
        }
    }
}