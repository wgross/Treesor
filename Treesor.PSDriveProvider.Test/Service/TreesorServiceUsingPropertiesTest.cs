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
        public void CreateColumn_type_string()
        {
            // ACT

            var result = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string).Name, result.TypeName);
        }

        [Test]
        public void CreateColumn_twice_is_accepted()
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
        public void CreateColumns_twice_fails_with_different_type()
        {
            // ARRANGE

            this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.CreateColumn(name: "p", typename: typeof(int).Name));

            // ASSERT

            Assert.AreEqual($"Column: 'p' already defined with type: '{typeof(string).Name}'", result.Message);
        }

        [Test]
        public void CreateColumns_fails_on_missing_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.CreateColumn(null, "type"));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void CreateColumns_fails_on_missing()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.CreateColumn(null, "type"));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        #endregion CreateColumn

        #region SetPropertyValue

        [Test]
        public void SetPropertyValue_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ACT

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: "value");

            // ASSERT

            Assert.AreEqual("value", column.GetValue(id));
        }

        [Test]
        public void SetProperyValue_changes_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);
            column.SetValue(id, "test");

            // ACT

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: "value2");

            // ASSERT

            Assert.AreEqual("value2", column.GetValue(id));
        }

        [Test]
        public void SetPropertyValue_fails_with_wrong_type()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);
            column.SetValue(id, "value");

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual("value", column.GetValue(id));
            Assert.AreEqual($"Couldn't assign value '5' to property 'p' at node '{id.Value}': value.GetType().Name must be 'String'", result.Message);
        }

        [Test]
        public void SetPropertyValue_fails_on_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual($"Property 'p' doesn't exist", result.Message);
        }

        [Test]
        public void SetPropertyValue_fails_on_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            var column = this.treesorService.CreateColumn(name: "p", typename: typeof(string).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual($"Node 'a' doesn't exist", result.Message);
        }

        [Test]
        public void SetPropertyValue_fails_on_missing_property_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), null, "value"));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void SetPropertyValue_fails_on_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.SetPropertyValue(null, "p", "value"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion SetPropertyValue

        #region GetPropertyValue

        [Test]
        public void GetPropertyValue_inner_nodes_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5);

            // ACT

            var result = this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.AreEqual(5, (int)result);
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            this.treesorService.CreateColumn("p", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_property_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.GetPropertyValue(null, "p"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion GetPropertyValue

        #region ClearPropertyValue

        [Test]
        public void ClearPropertyValue_at_existing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5);

            // ACT

            this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
        }

        [Test]
        public void ClearPropertyValue__fails_for_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
        }

        [Test]
        public void ClearPropertyValue__fails_for_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            this.treesorService.CreateColumn("p", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
        }

        [Test]
        public void ClearPropertyValue__fails_for_missing_columns_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void ClearPropertyValue_fails_for_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.ClearPropertyValue(null, "p"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion ClearPropertyValue
    }
}