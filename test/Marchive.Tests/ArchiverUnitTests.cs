using System;
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
    public class ArchiverUnitTests
    {
        private readonly IFileSystem _fileSystem = A.Fake<IFileSystem>();
        private readonly Archiver _archiver;

        public ArchiverUnitTests()
        {
            _archiver = new Archiver(_fileSystem, A.Fake<ILogger<Archiver>>());
        }

        [Fact]
        public void GivenTwoFilesThatExist_WhenArchive_ThenSavesArchiveFileWithContent()
        {
            var fileNames = new[] { "file1.bin", "file2.exe" };
            var archiveFileName = "archive";
            var fileContent = Encoding.UTF8.GetBytes("content of file");
            A.CallTo(() => _fileSystem.ReadAllBytes(A<string>.Ignored))
                .Returns(fileContent);

            _archiver.Archive(fileNames.ToList(), archiveFileName);

            A.CallTo(() =>
                    _fileSystem.SaveFile(A<string>.Ignored,
                        A<byte[]>.That.Matches(x => x.Length > fileContent.Length * 2)))
                .MustHaveHappenedOnceExactly();
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
