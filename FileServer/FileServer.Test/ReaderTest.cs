using FileServer.Core;
using Xunit;

namespace FileServer.Test
{
    public class ReaderTest
    {
        [Fact]
        public void Make_Non_Null_Class()
        {
            Assert.NotNull(new Readers());
        }

        [Fact]
        public void Change_Class_State()
        {
            var dirProcessing = new DirectoryProcessor();
            var fileProcessing = new FileProcessor();
            var readers = new Readers()
            {
                FileProcess = fileProcessing,
                DirectoryProcess = dirProcessing
            };

            Assert.Equal(dirProcessing, readers.DirectoryProcess);
            Assert.Equal(fileProcessing, readers.FileProcess);
        }

    }
}
