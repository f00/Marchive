using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FakeItEasy;
using FluentAssertions;
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
            _unArchiver = new UnArchiver(_fileSystem);
        }

        [Fact]
        public void GivenExistingArchive_WhenUnArchive_ThenReturnsExtractedFiles()
        {
            var archiveFileName = "archive";
            var filesInArchive = new List<(string name, string content)>()
                {("file1.bin", "file 1 content and then some"), ("file2.bin", "file 2 content")};
            var archiveContent = GetArchive(archiveFileName, filesInArchive);
            A.CallTo(() => _fileSystem.ReadAllBytes(A<string>.Ignored))
                .Returns(archiveContent);

            var files = _unArchiver.UnArchive(archiveFileName).ToList();

            files.Select(f => f.filename).Should().BeEquivalentTo(filesInArchive.Select(f => f.name));
            files.Select(f => f.content).Should()
                .BeEquivalentTo(filesInArchive.Select(f => Encoding.UTF8.GetBytes(f.content)));
        }

        // Helper method that uses Archiver to create an archive for testing
        // This is to avoid duplicating any file merging logic here in the tests
        private byte[] GetArchive(string archiveFileName, List<(string name, string content)> filesAndContent)
        {
            var fileSystem = A.Fake<IFileSystem>();
            var archiver = new Archiver(fileSystem);
            foreach (var file in filesAndContent)
            {
                A.CallTo(() => fileSystem.ReadAllBytes(file.name))
                    .Returns(Encoding.UTF8.GetBytes(file.content));
            }

            return archiver.Archive(filesAndContent.Select(x => x.name).ToList(), archiveFileName);
        }
    }
}