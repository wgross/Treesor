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

        #region RemoveColumn - NotSupported

        [Test]
        public void RemoveColumn_isnt_supported()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.RemoveProperty(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Removal of columns is currently not supported", result.Message);
        }

        #endregion RemoveColumn - NotSupported

        #region RenameColumns - NotSuported

        [Test]
        public void RenameColumn_isnt_supported()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.RenameProperty(TreesorNodePath.Create("a"), "p", "q"));

            // ASSERT

            Assert.AreEqual("Renaming columns is currently not supported", result.Message);
        }

        #endregion RenameColumns - NotSuported

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

            this.hierarchyMock.VerifyAll();
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

            this.hierarchyMock.VerifyAll();
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

            this.hierarchyMock.VerifyAll();
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

            this.hierarchyMock.VerifyAll();
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

            this.hierarchyMock.VerifyAll();
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

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_column()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out id), Times.Never());
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

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void GetPropertyValue_fails_for_missing_property_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
            ;
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

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void ClearPropertyValue_fails_for_missing_column()
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
        public void ClearPropertyValue_fails_for_missing_node()
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

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void ClearPropertyValue_fails_for_missing_columns_name()
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

        #region CopyPropertyValue

        [Test]
        public void CopyPropertyValue_from_root_to_child()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_at_same_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_from_child_to_root()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: 5);

            // ACT

            this.treesorService.CopyPropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_fails_for_missing_destination_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_fails_for_missing_destination_column()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'q' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_fails_for_missing_source_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.CopyPropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_fails_for_missing_source_column()
        {
            // ARRANGE

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("q", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out id), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyPropertyValue_fails_for_different_types()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(double).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5' to property 'q' at node '{id_a.Value.ToString()}': value.GetType().Name must be 'Double'", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        [Test]
        public void MovePropertyValue_from_root_to_child()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_at_same_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_from_child_to_root()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: 5);

            // ACT

            this.treesorService.MovePropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_fails_for_missing_destination_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_fails_for_missing_destination_column()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'q' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_fails_for_missing_source_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.MovePropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_fails_for_missing_source_column()
        {
            // ARRANGE

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("q", typeof(int).Name);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out id), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void MovePropertyValue_fails_for_different_types()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int).Name);
            this.treesorService.CreateColumn("q", typeof(double).Name);
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5' to property 'q' at node '{id_a.Value.ToString()}': value.GetType().Name must be 'Double'", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        #endregion MovePropertyValue
    }
}