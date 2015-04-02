using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD.Utilities
{
    /// <summary>
    /// Exception that indicates a failure in the download attempt.
    /// </summary>
    [Serializable]
    public class DownloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        public DownloadException(Uri uri): this(uri, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        /// <param name="inner">The inner exception.</param>
        public DownloadException(Uri uri, Exception inner)
            : this(string.Format("Unable to download html from uri {0}", uri), inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DownloadException(string message) : this(message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="message">The message.The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public DownloadException(string message, Exception inner) : base(message, inner) { }

        protected DownloadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
