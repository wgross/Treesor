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
        private Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        private TreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new TreesorService(this.hierarchyMock.Object);
        }

        #region CreateColumn

        [Test]
        public void Create_property_type_string()
        {
            // ACT

            var result = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string).Name, result.TypeName);
        }

        [Test]
        public void Create_same_property_twice_is_accepted()
        {
            // ARRANGE

            var column = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ACT

            var result = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreSame(column, result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string).Name, result.TypeName);
        }

        [Test]
        public void Create_same_property_twice_fails_with_different_type()
        {
            // ARRANGE

            this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.CreateColumn(name: "p", typename: typeof(int).Name));

            // ASSERT

            Assert.IsTrue(result.Message.Equals($"Column: 'p' already defined with type: '{typeof(string).Name}'"));
        }

        #endregion CreateProperty

        [Test]
        public void Get_inner_nodes_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
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

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5);

            // ACT

            this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
        }
    }
}