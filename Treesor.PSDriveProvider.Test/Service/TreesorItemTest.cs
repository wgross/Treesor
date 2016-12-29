using NUnit.Framework;
using System;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class TreesorItemTest
    {
        [Test]
        public void TreesorItems_are_equal_if_ids_are_equal()
        {
            // ARRANGE

            var id = Guid.NewGuid();
            var a = new TreesorItem(TreesorNodePath.Create("a"), id);
            var b = new TreesorItem(TreesorNodePath.Create("b"), id);

            // ACT

            var result_a = a.Equals(b);
            var result_b = b.Equals(a);

            // ASSERT

            Assert.IsTrue(result_a);
            Assert.IsTrue(result_b);
        }

        [Test]
        public void TreesorItems_hachcodes_are_equal_if_ids_are_equal()
        {
            // ARRANGE

            var id = Guid.NewGuid();
            var a = new TreesorItem(TreesorNodePath.Create("a"), id);
            var b = new TreesorItem(TreesorNodePath.Create("b"), id);

            // ACT

            var result_a = a.GetHashCode();
            var result_b = b.GetHashCode();

            // ASSERT

            Assert.AreEqual(id.GetHashCode(), result_a);
            Assert.AreEqual(id.GetHashCode(), result_b);
        }
    }
}