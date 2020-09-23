using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Xunit;

namespace Marchive.Tests
{
    public class ArchiverUnitTests
    {
        private readonly IFileSystem _fileSystem = A.Fake<IFileSystem>();
        private readonly Archiver _archiver;

        public ArchiverUnitTests()
        {
            _archiver = new Archiver(_fileSystem);
        }

        [Fact]
        public void GivenTwoFilesThatExist_WhenArchive_ThenCreatedArchiveWithContent()
        {
            var fileNames = new[] { "file1.bin", "file2.exe" };
            var archiveFileName = "archive";
            var fileContent = Encoding.UTF8.GetBytes("content of file");
            A.CallTo(() => _fileSystem.ReadAllBytes(A<string>.Ignored))
                .Returns(fileContent);

            var archive = _archiver.Archive(fileNames.ToList(), archiveFileName);

            archive.Length.Should()
                .Be(fileContent.Length * 2 + Archiver.MetaBlockSizeBytes * 2 + Archiver.HeaderSizeBytes);
        }

        [Fact]
        public void GivenFileNameTooLong_WhenArchive_ThenThrowsException()
        {
            var fileName =
                "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvb"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn"
                + "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn.bin";
            var archiveFileName = "archive";

            Assert.Throws<ArgumentOutOfRangeException>(() => _archiver.Archive(new[] { fileName }.ToList(), archiveFileName));
        }

        [Fact]
        public void GivenExistingArchive_WhenUnArchive_ThenReturnsExtractedFiles()
        {
            var archiveFileName = "archive";
            var filesInArchive = new List<(string name, string content)>()
                {("file1.bin", "file 1 content and then some"), ("file2.bin", "file 2 content")};
            foreach (var file in filesInArchive)
            {
                A.CallTo(() => _fileSystem.ReadAllBytes(file.name))
                    .Returns(Encoding.UTF8.GetBytes(file.content));
            }
            var archiveContent = _archiver.Archive(filesInArchive.Select(x => x.name).ToList(), archiveFileName);

            var files = _archiver.UnArchive(archiveContent).ToList();

            files.Select(f => f.filename).Should().BeEquivalentTo(filesInArchive.Select(f => f.name));
            files.Select(f => f.content).Should()
                .BeEquivalentTo(filesInArchive.Select(f => Encoding.UTF8.GetBytes(f.content)));
        }
    }
}
