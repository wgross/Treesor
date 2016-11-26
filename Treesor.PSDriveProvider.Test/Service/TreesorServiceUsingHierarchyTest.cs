using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class TreesorServiceUsingHierarchyTest
    {
        private Mock<IHierarchy<string, Guid>> hierarchy;
        private TreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchy = new Mock<IHierarchy<string, Guid>>();
            this.treesorService = new TreesorService(this.hierarchy.Object);
        }

        #region NewItem > Add

        [Test]
        public void NewItem_creates_hierarchy_node_under_root()
        {
            // ACT

            this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null);

            // ASSERT

            this.hierarchy.Verify(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Guid>()), Times.Once());
        }

        [Test]
        public void NewItem_doesnt_accept_value()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), new object()));

            // ASSERT

            Assert.IsTrue(result.Message.Contains($"A value for node {TreesorNodePath.Create("item")} is not allowed"));
        }

        #endregion NewItem > Add
    }
}