using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class NonVersionTag
    {
        [Fact]
        public static async Task HasDefaultVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "foo");

            var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0" });

            // act
            var (actual, _) = await Sdk.BuildProject(path);
            var cli = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
        }
    }
}
