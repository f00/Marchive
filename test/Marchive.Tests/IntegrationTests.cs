using System.IO;
using System.Linq;
using FakeItEasy;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marchive.Tests
{
    public class IntegrationTests
    {
        private readonly App.Marchive _marchive;
        private const string FixturePath = "Fixtures";

        public IntegrationTests()
        {
            var fileSystem = new FileSystemProxy();
            _marchive = new App.Marchive(new Archiver(fileSystem), new UnArchiver(), fileSystem,
                A.Fake<ILogger<App.Marchive>>(), new MArchiveSettings());
        }

        [Fact]
        public void GivenTwoTextFiles_WhenArchiveAndUnArchive_ThenUnArchivedFilesAreIdenticalToOriginal()
        {
            // Arrange
            var fileName1 = "input1.txt";
            var fileName2 = "input2.txt";
            var file1Path = Path.Combine(FixturePath, fileName1);
            var file2Path = Path.Combine(FixturePath, fileName2);
            const string archiveFileName = "archive";
            const string unArchiveDirectory = "UnArchive";

            // Act
            _marchive.Archive(new[] { file1Path, file2Path }.ToList(), archiveFileName);
            _marchive.UnArchive(archiveFileName, unArchiveDirectory);

            // Assert
            // ** Produces archive file **
            Assert.True(File.Exists(archiveFileName + ".mar"));

            // ** Extracts archived files **
            Assert.True(File.Exists(Path.Combine(unArchiveDirectory, fileName1)));
            Assert.True(File.Exists(Path.Combine(unArchiveDirectory, fileName2)));

            // ** Extracted files are identical to original **
            Assert.Equal(File.ReadAllBytes(file1Path), File.ReadAllBytes(Path.Combine(unArchiveDirectory, fileName1)));
            Assert.Equal(File.ReadAllBytes(file2Path), File.ReadAllBytes(Path.Combine(unArchiveDirectory, fileName2)));

            // Cleanup
            File.Delete(archiveFileName + ".mar");
            Directory.Delete(unArchiveDirectory, true);
        }

        [Fact]
        public void GivenOneImageFile_WhenArchiveAndUnArchive_ThenUnArchivedFileIsIdenticalToOriginal()
        {
            // Arrange
            var fileName = "IMG_3555.jpg";
            var filePath = Path.Combine(FixturePath, fileName);
            const string archiveFileName = "archive";
            const string unArchiveDirectory = "UnArchive";

            // Act
            _marchive.Archive(new[] { filePath }.ToList(), archiveFileName);
            _marchive.UnArchive(archiveFileName, unArchiveDirectory);

            // Assert
            // ** Produces archive file **
            Assert.True(File.Exists(archiveFileName + ".mar"));

            // ** Extracts archived files **
            Assert.True(File.Exists(Path.Combine(unArchiveDirectory, fileName)));

            // ** Extracted files are identical to original **
            Assert.Equal(File.ReadAllBytes(filePath), File.ReadAllBytes(Path.Combine(unArchiveDirectory, fileName)));

            // Cleanup
            File.Delete(archiveFileName + ".mar");
            Directory.Delete(unArchiveDirectory, true);
        }
    }
}