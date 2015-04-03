using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace LD.Utilities
{
    /// <summary>
    /// A utility class to download images from a given HTML page.
    /// </summary>
    public class DownloadImages
    {
        private System.Net.Http.HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadImages"/> class.
        /// </summary>
        public DownloadImages()
        {
            this.client = new HttpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadImages"/> class.
        /// </summary>
        /// <param name="client">The http client to use to download web page and image files.</param>
        /// <exception cref="System.ArgumentNullException">client is null</exception>
        public DownloadImages(System.Net.Http.HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            this.client = client;
        }

        /// <summary>
        /// Downloads images given a url to an html page.
        /// </summary>
        /// <param name="uri">The URI to the html page.</param>
        /// <param name="directory">The directory to save the image to.  Directory must exist and have write permission to the folder.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="uri"/> is null.
        /// or
        /// <paramref name="directory"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="uri"/> is not absolute.</exception>
        /// <exception cref="LD.Utilities.DownloadException">Failed to download html from <paramref name="uri"/>.</exception>
        public async Task DownloadAsync(Uri uri, System.IO.DirectoryInfo directory)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri", "The uri must be absolute.");
            }
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }
            if (!directory.Exists)
            {
                throw new ArgumentException(string.Format("Directory does not exist: {0}", directory));
            }
            try
            {
                var initialResponse = await client.GetStringAsync(uri);
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(initialResponse);
                var imageUris = html.DocumentNode
                    .Descendants("img")
                    .Select(m => m.GetAttributeValue("src", null))
                    .Where(m => m != null)
                    .Distinct()
                    .Select(m =>
                    {
                        var imageUri = new Uri(m, UriKind.RelativeOrAbsolute);
                        if (!imageUri.IsAbsoluteUri)
                        {
                            imageUri = new Uri(uri, imageUri);
                        }
                        return imageUri;
                    });
                var tasks = imageUris.Select(imageUri => GetImageAsync(imageUri, directory)).ToArray();
                await Task.WhenAll(tasks);
                
            }
            catch (HttpRequestException requestException)
            {
                throw new DownloadException(uri, requestException);
            }
        }

        private async Task GetImageAsync(Uri imageUri, DirectoryInfo directory)
        {
            try
            {
                var imageBytes = await client.GetByteArrayAsync(imageUri);
                var path = Path.Combine(directory.ToString(), Path.GetFileName(imageUri.LocalPath));
                File.WriteAllBytes(path, imageBytes);
            }
            catch (HttpRequestException) { }
        }
    }
}
