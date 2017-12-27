using Xunit;

namespace Treesor.PSDriveProvider.Test.Services
{
    public class TreesorColumnTest
    {
        [Fact]
        public void TreesorColumn_are_equal_if_name_and_type_are_equal()
        {
            // ARRANGE

            var column = new TreesorColumn("name", typeof(string));

            // ASSERT

            Assert.Equal(column, column);
            Assert.Equal(column, new TreesorColumn("name", typeof(string)));
            Assert.NotEqual(column, new TreesorColumn("nameX", typeof(string)));
            Assert.NotEqual(column, new TreesorColumn("name", typeof(int)));
            Assert.NotNull(column);
            Assert.NotEqual("any string", column.Name);
        }

        [Fact]
        public void TreesorColumn_hashcodes_are_equal_if_name_and_type_are_equal()
        {
            // ARRANGE

            var column = new TreesorColumn("name", typeof(string));

            // ASSERT

            Assert.Equal(column.GetHashCode(), column.GetHashCode());
            Assert.Equal(column.GetHashCode(), new TreesorColumn("name", typeof(string)).GetHashCode());
            Assert.NotEqual(column.GetHashCode(), new TreesorColumn("nameX", typeof(string)).GetHashCode());
            Assert.NotEqual(column.GetHashCode(), new TreesorColumn("name", typeof(int)).GetHashCode());
        }
    }
}