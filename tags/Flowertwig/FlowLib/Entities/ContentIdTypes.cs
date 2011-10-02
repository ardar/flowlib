using System;

namespace FlowLib.Entities
{
    /// <summary>
    /// Id types, example for ContentInfo
    /// </summary>
    [Flags]
    public enum ContentIdTypes : byte
    {
        /* Has no Id */
        None = 0,
        /// <summary>
        /// Mainly for DownloadItem, we have no hash for this file, just virtualname
        /// </summary>
        Filename,
        /// <summary>
        /// Mainly for DownloadItem, this is a filelist
        /// </summary>
        Filelist,
        FilelistBz = Filelist | 1,
        FilelistXmlBz = Filelist | 2,
        /// <summary>
        /// Mainly for DownloadItem, this is a filelist that is just downloaded to get more alternative sources
        /// </summary>
        Parent,
        /* Id is a hash. It could be TTH but it doesnt have to be */
        Hash,
        /* Id is a hash, tiger tree hash to be exact */
        TTH = Hash | 1
    }
}