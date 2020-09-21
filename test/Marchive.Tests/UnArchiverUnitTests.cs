using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FakeItEasy;
using Marchive.App;
using Marchive.App.IO;
using Marchive.App.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marchive.Tests
{
    public class UnArchiverUnitTests
    {
        private readonly IFileSystem _fileSystem = A.Fake<IFileSystem>();
        private readonly UnArchiver _unArchiver;

        public UnArchiverUnitTests()
        {
            _unArchiver = new UnArchiver(_fileSystem, A.Fake<ILogger<UnArchiver>>());
        }

        [Fact]
        public void GivenExistingArchive_WhenUnArchive_ThenSavesExtractedFiles()
        {
            var archiveFileName = "archive";
            var archiveContent = GetArchive(archiveFileName,
                new List<(string name, string content)>()
                    {("file1.bin", "file 1 content and then some"), ("file2.bin", "file 2 content")});
            A.CallTo(() => _fileSystem.ReadAllBytes(A<string>.Ignored))
                .Returns(archiveContent);

            _unArchiver.UnArchive(archiveFileName);

            A.CallTo(() => _fileSystem.SaveFile(A<string>.Ignored, A<byte[]>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        // Helper method that uses Archiver to create an archive for testing
        // This is to avoid duplicating any file merging logic here in the tests
        private byte[] GetArchive(string archiveFileName, List<(string name, string content)> filesAndContent)
        {
            var fileSystem = A.Fake<IFileSystem>();
            MemoryStream ms = new MemoryStream();
            var archiver = new Archiver(fileSystem, A.Fake<ILogger<Archiver>>());
            foreach (var file in filesAndContent)
            {
                A.CallTo(() => fileSystem.ReadAllBytes(file.name))
                    .Returns(Encoding.UTF8.GetBytes(file.content));
            }
            A.CallTo(() => fileSystem.SaveFile(A<string>.Ignored, A<byte[]>.Ignored))
                .Invokes(x => ms = new MemoryStream(x.GetArgument<byte[]>("content")));

            archiver.Archive(filesAndContent.Select(x => x.name).ToList(), archiveFileName);

            return ms.ToArray();
        }
    }
}