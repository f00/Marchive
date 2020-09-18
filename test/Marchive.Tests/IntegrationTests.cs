using System.IO;
using System.Linq;
using Marchive.App;
using Marchive.App.IO;
using Xunit;

namespace Marchive.Tests
{
    public class IntegrationTests
    {
        private readonly Archiver _archiver = new Archiver(new FileSystemProxy());
        private readonly UnArchiver _unArchiver = new UnArchiver(new FileSystemProxy());
        private const string FixturePath = "Fixtures";

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
            _archiver.Archive(new[] { file1Path, file2Path }.ToList(), archiveFileName);
            _unArchiver.UnArchive(archiveFileName, unArchiveDirectory);

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
            _archiver.Archive(new[] { filePath }.ToList(), archiveFileName);
            _unArchiver.UnArchive(archiveFileName, unArchiveDirectory);

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