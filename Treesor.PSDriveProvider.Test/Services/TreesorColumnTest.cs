using NUnit.Framework;

namespace Treesor.PSDriveProvider.Test.Services
{
    [TestFixture]
    public class TreesorColumnTest
    {
        [Test]
        public void TreesorColumn_are_equal_if_name_and_type_are_equal()
        {
            // ARRANGE

            var column = new TreesorColumn("name", typeof(string));

            // ASSERT

            Assert.AreEqual(column, column);
            Assert.AreEqual(column, new TreesorColumn("name", typeof(string)));
            Assert.AreNotEqual(column, new TreesorColumn("nameX", typeof(string)));
            Assert.AreNotEqual(column, new TreesorColumn("name", typeof(int)));
            Assert.AreNotEqual(column, null);
            Assert.AreNotEqual(column, "any string");
        }

        [Test]
        public void TreesorColumn_hashcodes_are_equal_if_name_and_type_are_equal()
        {
            // ARRANGE

            var column = new TreesorColumn("name", typeof(string));

            // ASSERT

            Assert.AreEqual(column.GetHashCode(), column.GetHashCode());
            Assert.AreEqual(column.GetHashCode(), new TreesorColumn("name", typeof(string)).GetHashCode());
            Assert.AreNotEqual(column.GetHashCode(), new TreesorColumn("nameX", typeof(string)).GetHashCode());
            Assert.AreNotEqual(column.GetHashCode(), new TreesorColumn("name", typeof(int)).GetHashCode());
        }
    }
}