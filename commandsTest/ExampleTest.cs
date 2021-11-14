using System;
using System.IO;
using System.Threading.Tasks;
using commands.extensions;
using progressTree;
using Xunit;

namespace commandsTest
{
    public sealed class ExampleTest : IDisposable
    {
        private readonly DirectoryInfo tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "foo"));
        [Fact]
        public async Task LocalTest()
        {
            var progress = Progress.Empty;
            String source = FullPath("addition.json");
            String result = FullPath("result.json");
            CreateAddition(source);
            var command = Examples.EvaluateFromFileSystem(source, result);

            await command.Execute(progress).ConfigureAwait(false);

            var evaluatedResult = File.ReadAllText(result);
            Assert.Contains("8", evaluatedResult);
        }

        public String FullPath(String localPath) => Path.Combine(this.tempDir.FullName, localPath);

        public void Dispose()
        {
            this.tempDir.Delete(recursive: true);
        }

        private void CreateAddition(String source)
        {
            var json = "{\"LeftOperand\": 3, \"Operation\": \"+\", \"RightOperand\":5}";
            File.WriteAllText(source, json);
        }
    }
}