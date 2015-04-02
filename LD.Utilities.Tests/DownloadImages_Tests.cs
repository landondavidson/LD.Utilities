using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LD.Utilities.Tests
{
    [TestClass]
    public class DownloadImages_Tests
    {
        /// <summary>
        /// Asserts that a DownloadImages object is instantiated when the constructor is called.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public void ctor_BaseTest()
        {
            #region Arrange
            var client = new HttpClient();
            #endregion

            #region Adjust
            //base arrangement no need for adjustment
            #endregion

            #region Act
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Assert
            Assert.IsNotNull(classUnderTest);
            Assert.IsInstanceOfType(classUnderTest, typeof(DownloadImages));
            #endregion
        }

        /// <summary>
        /// Asserts that an ArgumentNullException is thrown when a null client is passed into the constructor.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_client_Null()
        {
            #region Arrange
            var client = new HttpClient();
            #endregion

            #region Adjust
            client = null;
            #endregion

            #region Act
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Assert
            //exception expected
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_BaseTest()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            //base arrangement no need for adjustment
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.AreEqual(directory.ToString(), Path.GetDirectoryName(savePath));
            Assert.AreEqual(imageName, Path.GetFileName(savePath));
            CollectionAssert.AreEquivalent(imageContent, imageBytes);
            #endregion
        }

        /// <summary>
        /// Asserts that when the directory does not exist an ArgumentException is thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Download_Directory_Does_Not_Exist()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            //base arrangement no need for adjustment
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (directoryInfo) =>
                {
                    return false;
                };
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            //exception expected
            #endregion
        }

        /// <summary>
        /// Asserts that when a string content is not returned from the given uri no images are downloaded.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_No_String_Content_In_uri()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var invalidContent = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NoContent);
                    response.Content = null;
                    return response;
                }));
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.IsNull(savePath);
            Assert.IsNull(imageBytes);
            #endregion
        }

        /// <summary>
        /// Asserts that when the directory is null and ArgumentNullException is thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Download_directory_Null()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            directory = null;
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.AreEqual(directory.ToString(), Path.GetDirectoryName(savePath));
            Assert.AreEqual(imageName, Path.GetFileName(savePath));
            CollectionAssert.AreEquivalent(imageContent, imageBytes);
            #endregion
        }

        /// <summary>
        /// Asserts that when the uri is null an ArgumentNullException is thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Download_uri_Null()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            uri = null;
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            //exception expected
            #endregion
        }

        /// <summary>
        /// Asserts that when the uri is not absolute an ArgumentOutOfRangeException is thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task Download_uri_NotAbsolute()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            uri = new Uri("relative", UriKind.Relative);
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            //exception expected
            #endregion
        }

        /// <summary>
        /// Asserts that when an invalid url is passed into the a DownloadException is thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(DownloadException))]
        public async Task Download_uri_Invalid()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound); //<-- adjusted to get error.
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            //exception expected
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded even if the html is not strict XHTML.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_Malformed_Html()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var notValidHtml = string.Format("<img src=\"{0}\">", imageName); //missing closing tag.
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(notValidHtml);
                    return response;
                }));
            #endregion

            #region Act
            string savePath = null;
            byte[] imageBytes = null;
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath = path;
                        imageBytes = bytes;
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.AreEqual(directory.ToString(), Path.GetDirectoryName(savePath));
            Assert.AreEqual(imageName, Path.GetFileName(savePath));
            CollectionAssert.AreEquivalent(imageContent, imageBytes);
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded. Including a mix of absolute and relative images.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_With_Absolute_Image()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var absoluteImageUrl = "http://example.com/g.gif";
            var absoluteImageName = "g.gif";
            var imageHtml = string.Format(
                            @"<body>
                                <img src=""{0}""/>
                                <img src=""{1}""/>
                              </body>", imageName, absoluteImageUrl);
            var contentWithAbsoluteAndRelativeUris = new StringContent(imageHtml);
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = contentWithAbsoluteAndRelativeUris;
                    return response;
                }));
            var absoluteImageContent = UnicodeEncoding.Default.GetBytes("This is our absolute mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(absoluteImageUrl)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(absoluteImageContent);
                    return response;
                }));
            #endregion

            #region Act
            var savePath = new List<string>();
            var imageBytes = new List<byte[]>();
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath.Add(path);
                        imageBytes.Add(bytes);
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.IsTrue(savePath.All(m => Path.GetDirectoryName(m) == directory.ToString()));
            Assert.AreEqual(2, savePath.Count);
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == imageName));
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == absoluteImageName));
            Assert.AreEqual(2, imageBytes.Count);
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our mocked up image"));
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our absolute mocked up image"));
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded. Including a mix of absolute and relative images.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_With_Relative_Image()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var relativeImageUrl = "./images/g.gif";
            var relativeImageName = "g.gif";
            var imageHtml = string.Format(
                            @"<body>
                                <img src=""{0}""/>
                                <img src=""{1}""/>
                              </body>", imageName, relativeImageUrl);
            var contentWithAbsoluteAndRelativeUris = new StringContent(imageHtml);
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = contentWithAbsoluteAndRelativeUris;
                    return response;
                }));
            var absoluteImageContent = UnicodeEncoding.Default.GetBytes("This is our absolute mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, relativeImageUrl)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(absoluteImageContent);
                    return response;
                }));
            #endregion

            #region Act
            var savePath = new List<string>();
            var imageBytes = new List<byte[]>();
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath.Add(path);
                        imageBytes.Add(bytes);
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.IsTrue(savePath.All(m => Path.GetDirectoryName(m) == directory.ToString()));
            Assert.AreEqual(2, savePath.Count);
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == imageName));
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == relativeImageName));
            Assert.AreEqual(2, imageBytes.Count);
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our mocked up image"));
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our absolute mocked up image"));
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded and the images that fail to download are skipped.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_Image_Fails_To_Download()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var absoluteImageUrl = "http://example.com/g.gif";
            var absoluteImageName = "g.gif";
            var imageHtml = string.Format(
                            @"<body>
                                <img src=""{0}""/>
                                <img src=""{1}""/>
                              </body>", imageName, absoluteImageUrl);
            var contentWithAbsoluteAndRelativeUris = new StringContent(imageHtml);
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = contentWithAbsoluteAndRelativeUris;
                    return response;
                }));
            var absoluteImageContent = UnicodeEncoding.Default.GetBytes("This is our absolute mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(absoluteImageUrl)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound); //<-- Adjusted the response code to be a failure
                    return response;
                }));
            #endregion

            #region Act
            var savePath = new List<string>();
            var imageBytes = new List<byte[]>();
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath.Add(path);
                        imageBytes.Add(bytes);
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.IsTrue(savePath.All(m => Path.GetDirectoryName(m) == directory.ToString()));
            Assert.AreEqual(1, savePath.Count);
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == imageName));
            Assert.AreEqual(1, imageBytes.Count);
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our mocked up image"));
            #endregion
        }

        /// <summary>
        /// Asserts that when a valid url is supplied all image files are downloaded the img elements with no src attribute will be skipped.
        /// </summary>
        [TestMethod]
        [TestCategory("LD.Utilities.DownloadImages")]
        public async Task Download_Missing_src_Attribute_Missing()
        {
            #region Arrange
            var uri = new Uri("http://localhost");
            var directory = new DirectoryInfo("C:\\");
            //mocks
            var handler = Mock.Of<HttpMessageHandler>();
            //mocks setup
            var imageName = "l.png";
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(string.Format("<img src=\"{0}\"/>", imageName));
                    return response;
                }));

            var imageContent = UnicodeEncoding.Default.GetBytes("This is our mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(uri, imageName)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(imageContent);
                    return response;
                }));
            //class under test
            var client = HttpClientFactory.Create(handler);
            var classUnderTest = new LD.Utilities.DownloadImages(client);
            #endregion

            #region Adjust
            var absoluteImageUrl = "http://example.com/g.gif";
            var absoluteImageName = "g.gif";
            var imageHtml = string.Format( // second image below has no src attribute
                            @"<body>
                                <img src=""{0}""/>
                                <img /> 
                              </body>", imageName, absoluteImageUrl);
            var contentWithAbsoluteAndRelativeUris = new StringContent(imageHtml);
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == uri), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = contentWithAbsoluteAndRelativeUris;
                    return response;
                }));
            var absoluteImageContent = UnicodeEncoding.Default.GetBytes("This is our absolute mocked up image");
            Mock.Get(handler).Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(absoluteImageUrl)), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(absoluteImageContent);
                    return response;
                }));
            #endregion

            #region Act
            var savePath = new List<string>();
            var imageBytes = new List<byte[]>();
            using (ShimsContext.Create())
            {
                System.IO.Fakes.ShimFile.WriteAllBytesStringByteArray =
                    (path, bytes) =>
                    {
                        savePath.Add(path);
                        imageBytes.Add(bytes);
                    };
                await classUnderTest.DownloadAsync(uri, directory);
            }
            #endregion

            #region Assert
            Assert.IsTrue(savePath.All(m => Path.GetDirectoryName(m) == directory.ToString()));
            Assert.AreEqual(1, savePath.Count);
            Assert.AreEqual(1, savePath.Count(m => Path.GetFileName(m) == imageName));
            Assert.AreEqual(1, imageBytes.Count);
            Assert.AreEqual(1, imageBytes.Count(m => UnicodeEncoding.Default.GetString(m) == "This is our mocked up image"));
            #endregion
        }
    }
}
