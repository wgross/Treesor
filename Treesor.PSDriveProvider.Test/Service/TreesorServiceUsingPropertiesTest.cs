using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class TreesorServiceUsingPropertiesTest
    {
        private Mock<IHierarchy<string, ValueReference<Guid>>> hierarchyMock;
        private TreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, ValueReference<Guid>>>();
            this.treesorService = new TreesorService(this.hierarchyMock.Object);
        }

        [Test]
        public void Get_înner_nodes_property_value()
        {
            // ARRANGE

            var id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5);

            // ACT

            var result = this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.AreEqual(5, (int)result);
        }

        [Test]
        public void Clear_existing_column()
        {
            // ARRANGE

            var nodeItem = this.treesorService.NewItem(TreesorNodePath.Create("a"), newItemValue: null);
            this.treesorService.CreateColumn(name: "test").SetValue(nodeItem, 5);

            // ACT

            this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
        }
    }
}