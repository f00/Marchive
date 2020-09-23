using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Marchive.App.Exceptions;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Marchive.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marchive.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly App.Marchive _sut;
        private const string FixturePath = "Fixtures";
        private const string FileName1 = "input1.txt";
        private const string FileName2 = "input2.txt";
        private const string FileName3 = "IMG_3555.jpg";
        private const string ArchiveFileName = "archive";
        private const string OutputUnArchiveDirectory = "UnArchive";

        public IntegrationTests()
        {
            var fileSystem = new FileSystemProxy();
            _sut = new App.Marchive(new Archiver(fileSystem), new UnArchiver(), fileSystem,
                A.Fake<ILogger<App.Marchive>>(), new MArchiveSettings());
        }

        [Fact]
        public void GivenTwoTextFiles_WhenArchiveAndUnArchive_ThenUnArchivedFilesAreIdenticalToOriginal()
        {
            // Act
            _sut.Archive(new[] { FileName1.WithPath(FixturePath), FileName2.WithPath(FixturePath) }.ToList(),
                ArchiveFileName);
            _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory);

            // Assert
            AssertUnArchivedFilesAreIdenticalToOriginal(FileName1, FileName2);
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_ThenUnArchivedFileIsIdenticalToOriginal()
        {
            // Act
            _sut.Archive(new[] { FileName3.WithPath(FixturePath) }.ToList(), ArchiveFileName);
            _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory);

            // Assert
            AssertUnArchivedFilesAreIdenticalToOriginal(FileName3);
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_AndEncrypted_AndPasswordIsCorrect_ThenUnArchivedFileIsIdenticalToOriginal()
        {
            // Arrange
            const string password = "password";

            // Act
            _sut.Archive(new[] { FileName3.WithPath(FixturePath) }.ToList(), ArchiveFileName, password);
            _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory, password);

            // Assert
            AssertUnArchivedFilesAreIdenticalToOriginal(FileName3);
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_AndNotEncrypted_AndPasswordIsPassed_ThenUnArchivedFileIsIdenticalToOriginal()
        {
            // Arrange
            const string password = "password";

            // Act
            _sut.Archive(new[] { FileName3.WithPath(FixturePath) }.ToList(), ArchiveFileName);
            _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory, password);

            // Assert
            AssertUnArchivedFilesAreIdenticalToOriginal(FileName3);
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_AndEncrypted_AndPasswordIsIncorrect_ThenThrowsException()
        {
            // Arrange
            const string password = "password";

            // Act
            _sut.Archive(new[] { FileName3.WithPath(FixturePath) }.ToList(), ArchiveFileName, password);
            Action unArchive = () => _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory, "anotherPass");

            // Assert
            unArchive.Should().Throw<InvalidEncryptionKeyException>();
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_AndEncrypted_AndPasswordIsNull_ThenThrowsException()
        {
            // Arrange
            const string password = "password";

            // Act
            _sut.Archive(new[] { FileName3.WithPath(FixturePath) }.ToList(), ArchiveFileName, password);
            Action unArchive = () => _sut.UnArchive(ArchiveFileName, OutputUnArchiveDirectory, null);

            // Assert
            unArchive.Should().Throw<InvalidEncryptionKeyException>();
        }

        private static void AssertUnArchivedFilesAreIdenticalToOriginal(params string[] fileNames)
        {
            // ** Produces archive file **
            Assert.True(File.Exists(ArchiveFileName + ".mar"));

            // ** Extracts archived files **
            foreach (var fileName in fileNames)
            {
                Assert.True(File.Exists(fileName.WithPath(OutputUnArchiveDirectory)));
            }

            // ** Extracted files are identical to original **
            foreach (var fileName in fileNames)
            {
                Assert.Equal(File.ReadAllBytes(fileName.WithPath(FixturePath)),
                    File.ReadAllBytes(fileName.WithPath(OutputUnArchiveDirectory)));
            }
        }

        private static void CleanUpArchiveAndUnArchivedFiles()
        {
            if (File.Exists(ArchiveFileName + ".mar"))
            {
                File.Delete(ArchiveFileName + ".mar");
            }
            if (Directory.Exists(OutputUnArchiveDirectory))
            {
                Directory.Delete(OutputUnArchiveDirectory, true);
            }
        }

        public void Dispose()
        {
            CleanUpArchiveAndUnArchivedFiles();
        }
    }
}