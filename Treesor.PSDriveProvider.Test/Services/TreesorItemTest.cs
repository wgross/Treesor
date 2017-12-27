using System;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Service
{
    public class TreesorItemTest
    {
        [Fact]
        public void TreesorItems_are_equal_if_ids_are_equal()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            var a = new TreesorItem(TreesorNodePath.Create("a"), id);
            var b = new TreesorItem(TreesorNodePath.Create("b"), id);

            // ACT

            var result_a = a.Equals(b);
            var result_b = b.Equals(a);

            // ASSERT

            Assert.True(result_a);
            Assert.True(result_b);
        }

        [Fact]
        public void TreesorItems_hachcodes_are_equal_if_ids_are_equal()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            var a = new TreesorItem(TreesorNodePath.Create("a"), id);
            var b = new TreesorItem(TreesorNodePath.Create("b"), id);

            // ACT

            var result_a = a.GetHashCode();
            var result_b = b.GetHashCode();

            // ASSERT

            Assert.Equal(id.Value.GetHashCode(), result_a);
            Assert.Equal(id.Value.GetHashCode(), result_b);
        }
    }
}