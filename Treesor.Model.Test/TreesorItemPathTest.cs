using Elementary.Hierarchy;
using System.Linq;
using Xunit;
using static Treesor.Model.TreesorItemPath;

namespace Treesor.Model.Test
{
    public class TreesorItemPathTest
    {
        [Fact]
        public void TreesorNodePath_from_path_item_array()
        {
            // ACT

            TreesorItemPath result = CreatePath("a", "b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void TreesorNodePath_from_HierarchyPath()
        {
            // ARRANGE

            var treeKey = HierarchyPath.Create("a");

            // ACT

            TreesorItemPath result = CreatePath(treeKey.Items.ToArray());

            // ASSERT

            Assert.Equal(treeKey, result.HierarchyPath);
            Assert.NotSame(treeKey, result.HierarchyPath);
        }

        [Fact]
        public void TreesorNodePath_from_string_with_slashes()
        {
            // ARRANGE

            TreesorItemPath result = ParsePath("a/b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void TreesorNodePath_from_string_with_backslashes()
        {
            // ARRANGE

            TreesorItemPath result = ParsePath(@"a\b");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b"), result.HierarchyPath);
        }

        [Fact]
        public void TreesotNodePath_from_string_with_mixed_slashes_and_backslashes()
        {
            // ARRANGE

            TreesorItemPath result = ParsePath(@"a\b/c");

            // ASSERT

            Assert.Equal(HierarchyPath.Create("a", "b", "c"), result.HierarchyPath);
        }

        [Fact]
        public void TreesorNodePaths_are_equal_if_HierarchyPaths_are_equal()
        {
            // ARRANGE

            var left = CreatePath("a", "b");
            var right = CreatePath("a", "b");

            // ACT

            bool result1 = left.Equals(right);
            bool result2 = right.Equals(left);

            // ASSERT

            Assert.True(result1);
            Assert.True(result2);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }

        [Fact]
        public void TreesorNodePaths_are_not_equal_if_HierarchyPaths_are_not_equal()
        {
            // ARRANGE

            var left = CreatePath("b", "a");
            var right = CreatePath("a", "b");

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

            Assert.Equal(RootPath, CreatePath());
            Assert.Equal(RootPath, CreatePath(""));
            Assert.Equal(RootPath, ParsePath(@"\"));
        }
    }
}