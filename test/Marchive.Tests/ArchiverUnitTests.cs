using System;
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
                .Be(fileContent.Length * 2 + Constants.MetaBlockSizeBytes * 2 + Constants.HeaderSizeBytes);
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
    }
}
