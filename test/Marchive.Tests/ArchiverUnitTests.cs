using System;
using System.IO;
using System.Linq;
using System.Text;
using FakeItEasy;
using Marchive.App;
using Marchive.App.IO;
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
        public void GivenTwoFilesThatExist_WhenArchive_ThenSavesArchiveFileWithContent()
        {
            var fileNames = new[] { "file1.bin", "file2.exe" };
            var archiveFileName = "archive";
            var fileContent = Encoding.UTF8.GetBytes("content of file");
            A.CallTo(() => _fileSystem.OpenFile(A<string>.Ignored))
                .ReturnsLazily(() => new FileStreamProxy(new MemoryStream(fileContent)));

            _archiver.Archive(fileNames.ToList(), archiveFileName);

            A.CallTo(() =>
                    _fileSystem.SaveFile(A<string>.Ignored,
                        A<byte[]>.That.Matches(x => x.Length > fileContent.Length * 2)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void GivenFileNameTooLong_WhenArchive_ThenThrowsException()
        {
            var fileName = "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbn.bin";
            var archiveFileName = "archive";

            Assert.Throws<ArgumentOutOfRangeException>(() => _archiver.Archive(new[] { fileName }.ToList(), archiveFileName));
        }
    }
}
