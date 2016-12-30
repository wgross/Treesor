using Elementary.Hierarchy.Collections;
using Elementary.Properties.Sparse;
using Moq;
using NUnit.Framework;
using System;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class TreesorServiceUsingPropertiesTest
    {
        private Mock<IHierarchy<string, Guid>> hierarchyMock;
        private TreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Guid>>();
            this.treesorService = new TreesorService(this.hierarchyMock.Object);
        }

        [Test]
        public void Create_column_by_name()
        {
            // ACT

            var result = this.treesorService.CreateColumn(name: "test");

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("test", result.Name);
        }

        [Test]
        public void Create_column_with_null_name_throws_ArgumentNullException()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(()=> this.treesorService.CreateColumn(name: null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void Get_an_existing_column_by_name()
        {
            // ARRANGE

            this.treesorService.CreateColumn(name: "test");

            // ACT

            TreesorColumn result = this.treesorService.GetColumn(name: "test");

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("test", result.Name);
        }

        [Test]
        public void Set_a_column_value_at_existing_column()
        {
            // ARRANGE

            var nodeItem = this.treesorService.NewItem(TreesorNodePath.Create("a"), newItemValue: null);
            var column = this.treesorService.CreateColumn(name: "test");
            column.SetValue(nodeItem, 5);

            // ACT

            var result = column.GetValue(nodeItem);

            // ASSERT

            Assert.AreEqual(5, (int)result);
        }
    }
}