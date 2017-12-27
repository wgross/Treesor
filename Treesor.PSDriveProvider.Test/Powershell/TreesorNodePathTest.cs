using Elementary.Hierarchy;
using System.Linq;
using Treesor.Model;
using Treesor.PSDriveProvider;
using Xunit;

namespace Treesor.PowershellDriveProvider.Test
{
    public class TreesorNodePathTest
    {
        [Fact]
        public void Create_TreesorNodePath_from_path_item_array()
        {
            // ACT

            TreesorNodePath result = TreesorNodePath.Create("a", "b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void Create_TreesorNodePath_from_HierarchyPath()
        {
            // ARRANGE

            var treeKey = HierarchyPath.Create("a");

            // ACT

            TreesorNodePath result = TreesorNodePath.Create(treeKey.Items.ToArray());

            // ASSERT

            Assert.Equal(treeKey, result.HierarchyPath);
            Assert.NotSame(treeKey, result.HierarchyPath);
        }

        [Fact]
        public void Parse_HierarchyPath_from_string_with_slashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse("a/b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void Parse_HierarchyPath_from_string_with_backslashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse(@"a\b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void Parse_HierarchyPath_from_string_with_mixed_slashes_and_backslashes()
        {
            // ARRANGE

            TreesorNodePath result = TreesorNodePath.Parse(@"a\b/c");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b", "c"), result.HierarchyPath);
        }

        [Fact]
        public void TreesorNodePathes_are_equal_if_HierarchyPaths_are_equal()
        {
            // ARRANGE

            var left = TreesorNodePath.Create("a", "b");
            var right = TreesorNodePath.Create("a", "b");

            // ACT

            bool result1 = left.Equals(right);
            bool result2 = right.Equals(left);

            // ASSERT

            Assert.True(result1);
            Assert.True(result2);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }

        [Fact]
        public void TreesorNodePathes_are_not_equal_if_HierarchyPaths_are_not_equal()
        {
            // ARRANGE

            var left = TreesorNodePath.Create("b", "a");
            var right = TreesorNodePath.Create("a", "b");

            // ACT

            bool result1 = left.Equals(right);
            bool result2 = right.Equals(left);

            // ASSERT

            Assert.False(result1);
            Assert.False(result2);
            Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Fact]
        public void TreesotNodePath_empty_is_root_path()
        {
            // ASSERT

            Assert.Equal(TreesorNodePath.RootPath, TreesorNodePath.Create());
            Assert.Equal(TreesorNodePath.RootPath, TreesorNodePath.Create(""));
            Assert.Equal(TreesorNodePath.RootPath, TreesorNodePath.Parse(@"\"));
        }
    }
}