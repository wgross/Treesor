using Elementary.Hierarchy;
using NUnit.Framework;
using System.Linq;
using Treesor.PSDriveProvider;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorNodePathTest
    {
        [Test]
        public void Create_TreesorNodePath_from_path_item_array()
        {
            // ACT

            TreesorNodePath result = TreesorNodePath.Create("a", "b");

            // ASSERT

            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Test]
        public void Create_TreesorNodePath_from_HierarchyPath()
        {
            // ARRANGE

            var treeKey = HierarchyPath.Create("a");

            // ACT

            TreesorNodePath result = TreesorNodePath.Create(treeKey.Items.ToArray());

            // ASSERT

            Assert.AreEqual(treeKey, result.HierarchyPath);
            Assert.AreNotSame(treeKey, result.HierarchyPath);
        }

        [Test]
        public void Parse_HierarchyPath_from_string_with_slashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse("a/b");

            // ASSERT

            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Test]
        public void Parse_HierarchyPath_from_string_with_backslashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse(@"a\b");

            // ASSERT

            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Test]
        public void Parse_HierarchyPath_from_string_with_mixed_slashes_and_backslashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse(@"a\b/c");

            // ASSERT

            Assert.AreEqual(HierarchyPath.Create("a", "b", "c"), result.HierarchyPath);
        }

        [Test]
        public void TreesorNodePathes_are_equal_if_HierarchyPaths_are_equal()
        {
            // ARRANGE

            var left = TreesorNodePath.Create("a", "b");
            var right = TreesorNodePath.Create("a", "b");

            // ACT

            bool result1 = left.Equals(right);
            bool result2 = right.Equals(left);

            // ASSERT

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void TreesorNodePathes_are_not_equal_if_HierarchyPaths_are_not_equal()
        {
            // ARRANGE

            var left = TreesorNodePath.Create("b", "a");
            var right = TreesorNodePath.Create("a", "b");

            // ACT

            bool result1 = left.Equals(right);
            bool result2 = right.Equals(left);

            // ASSERT

            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
            Assert.AreNotEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void TreesotNodePath_empty_is_root_path()
        {
            // ASSERT

            Assert.AreEqual(TreesorNodePath.RootPath, TreesorNodePath.Create());
            Assert.AreEqual(TreesorNodePath.RootPath, TreesorNodePath.Create(""));
            Assert.AreEqual(TreesorNodePath.RootPath, TreesorNodePath.Parse(@"\"));
        }
    }
}