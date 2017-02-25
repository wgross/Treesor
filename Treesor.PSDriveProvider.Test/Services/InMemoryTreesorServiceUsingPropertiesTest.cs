using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;
using Treesor.PSDriveProvider.Test.Service.Base;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class InMemoryTreesorServiceUsingPropertiesTest : TreesorServiceUsingPropertiesTestBase
    {
        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new InMemoryTreesorService(this.hierarchyMock.Object);
        }

        #region SetPropertyValue

        [Test]
        public void InMemoryService_sets_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_sets_property_value(id, "value");
        }

        [Test]
        public void InMemoryService_adds_second_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_adds_second_property_value(id, "value", 2);
        }

        [Test]
        public void InMemoryService_changes_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT

            base.TreesorService_changes_property_value(id, "value", "new value");
        }

        [Test]
        public void InMemoryService_fails_on_SetPropertyValue_with_wrong_type()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_wrong_type(id, "value");
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

            var column = this.treesorService.CreateColumn(name: "p", type: typeof(string));

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

            this.treesorService.CreateColumn("p", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));

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

            this.treesorService.CreateColumn("p", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));

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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));

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

            this.treesorService.CreateColumn("q", typeof(int));

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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(double));
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));

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

            this.treesorService.CreateColumn("q", typeof(int));

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

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(double));
            this.treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                this.treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
            Assert.AreEqual(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        #endregion MovePropertyValue
    }
}