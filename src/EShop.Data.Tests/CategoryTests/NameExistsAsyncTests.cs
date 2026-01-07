// todo: move to generic repository test
//namespace EShop.Data.Tests.CategoryTests;

//public sealed class NameExistsAsyncTests : CategoryRepositoryTestBase
//{
//    [Theory]
//    [InlineData("Widget", "widget", true)]
//    [InlineData("WIDGET", "widget", true)]
//    [InlineData("widget", "WIDGET", true)]
//    [InlineData("Widget", "Gadget", false)]
//    public Task CategoryNameExistsAsync_IsCaseInsensitive(string existingName, string queryName, bool expected)
//    {
//        return TestAsync(
//            seed:
//            [
//               new Category(existingName)
//            ],
//            testBody: async scope =>
//            {
//                var id = scope.Entities.Single().Id;

//                var exists = await scope.Sut.NameExistsAsync(queryName, CancellationToken.None);

//                Assert.Equal(expected, exists);
//            });
//    }
//}
