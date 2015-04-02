using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace LD.Utilities.Tests
{
    /// <summary>
    /// Summary description for DonloadImages_IntegrationTests
    /// </summary>
    [TestClass]
    public class DownloadImages_IntegrationTests
    {
        /// <summary>
        /// Asserts that images are downloaded when calling Download
        /// </summary>
        [TestMethod]
        [TestCategory("Integration: LD.Utilities.DownloadImages")]
        public async Task DownloadAsync_BaseTest()
        {
            #region Arrange
            var uri = new Uri("http://google.com");
            var tempPath = Path.Combine(Path.GetTempPath(), "LD.Utilities");
            var tempDirectory = new DirectoryInfo(tempPath);
            if (tempDirectory.Exists)
            {
                tempDirectory.Delete(true);
            }
            tempDirectory.Create();
            var fileCount = tempDirectory.GetFiles().Count();
            //class under test
            var classUnderTest = new LD.Utilities.DownloadImages(new HttpClient());
            #endregion

            #region Adjust
            //base arrangement no need for adjustment
            #endregion

            #region Act
            await classUnderTest.DownloadAsync(uri, tempDirectory);
            #endregion

            #region Assert
            var currentFileCount = tempDirectory.GetFiles().Count();
            Assert.AreNotEqual(fileCount, currentFileCount);
            #endregion
        }

        /// <summary>
        /// Asserts that images are downloaded when calling Download for another site.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration: LD.Utilities.DownloadImages")]
        public async Task DownloadAsync_AltSource()
        {
            #region Arrange
            var uri = new Uri("http://google.com");
            var tempPath = Path.Combine(Path.GetTempPath(), "LD.Utilities");
            var tempDirectory = new DirectoryInfo(tempPath);
            if (tempDirectory.Exists)
            {
                tempDirectory.Delete(true);
            }
            tempDirectory.Create();
            var fileCount = tempDirectory.GetFiles().Count();
            //class under test
            var classUnderTest = new LD.Utilities.DownloadImages(new HttpClient());
            #endregion

            #region Adjust
            uri = new Uri("http://www.altsrc.net/");
            #endregion

            #region Act
            await classUnderTest.DownloadAsync(uri, tempDirectory);
            #endregion

            #region Assert
            var currentFileCount = tempDirectory.GetFiles().Count();
            Assert.AreNotEqual(fileCount, currentFileCount);
            #endregion
        }

        /// <summary>
        /// Asserts that a download exception is thrown when the uri is unnavigable.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration: LD.Utilities.DownloadImages")]
        [ExpectedException(typeof(DownloadException))]
        public async Task DownloadAsync_Unknown()
        {
            #region Arrange
            var uri = new Uri("http://google.com");
            var tempPath = Path.Combine(Path.GetTempPath(), "LD.Utilities");
            var tempDirectory = new DirectoryInfo(tempPath);
            if (tempDirectory.Exists)
            {
                tempDirectory.Delete(true);
            }
            tempDirectory.Create();
            var fileCount = tempDirectory.GetFiles().Count();
            //class under test
            var classUnderTest = new LD.Utilities.DownloadImages(new HttpClient());
            #endregion

            #region Adjust
            uri = new Uri("http://localhost:3453/");
            #endregion

            #region Act
            await classUnderTest.DownloadAsync(uri, tempDirectory);
            #endregion

            #region Assert

            #endregion
        }
    }
}
