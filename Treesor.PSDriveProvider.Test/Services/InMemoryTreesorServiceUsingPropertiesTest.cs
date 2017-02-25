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
        public void InMemoryService_fails_on_SetPropertyValue_fon_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_column(id);
        }

        [Test]
        public void InMemoryService_fails_on_SetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_at_missing_node();
        }

        [Test]
        public void InMemoryService_fails_on_SetPropertyValue_on_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_property_name();
        }

        [Test]
        public void InMemoryService_fails_on_SetPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_node_path();
        }

        #endregion SetPropertyValue

        #region GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        [Test]
        public void InMemoryService_fails_on_GetPropertyValue_at_missing_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_at_missing_column();
        }

        [Test]
        public void InMemoryService_fails_on_GetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_for_missing_node();
        }

        [Test]
        public void InMemoryService_fails_on_GetPropertyValue_with_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_property_name();
        }

        [Test]
        public void InMemoryService_GetPropertyValue_fails_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_node_path();
        }

        #endregion GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        #region ClearPropertyValue

        [Test]
        public void InMemoryService_clears_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSSERT

            base.TreesorService_clears_property_value(id);
        }

        [Test]
        public void InMemoryService_clears_second_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSSERT

            base.TreesorService_clears_second_property_value(id, 5, "value");
        }

        [Test]
        public void InMemoryService_fails_on_ClearPropertyValue_for_missing_column()
        {
            // ACT & ARRANGE

            base.TreesorService_fails_on_ClearPropertyValue_for_missing_column();
        }

        [Test]
        public void InMemoryService_fails_on_ClearPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_at_missing_node();
        }

        [Test]
        public void InMemoryService_fails_ClearPropertyValue_fails_for_missing_column_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_column_name();
        }

        [Test]
        public void InMemoryService_fails_on_ClearPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_node_path();
        }

        #endregion ClearPropertyValue

        #region CopyPropertyValue

        [Test]
        public void InMemoryService_copies_property_value_from_root_to_child()
        {
            // ACT & ARRANGE

            base.TreesorService_copies_property_value_from_root_to_child("value");
        }

        [Test]
        public void TreesorService_copies_property_value_within_same_node()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_within_same_node(5);
        }

        [Test]
        public void InMemomryService_copies_property_value_from_child_to_root()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_from_child_to_root(6);
        }

        [Test]
        public void InMemoryService_fails_on_CopyPropertyValue_at_missing_destination_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_at_missing_destination_node(7);
        }

        [Test]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_destination_column(7);
        }

        [Test]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_source_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValues_for_missing_source_node();
        }

        [Test]
        public void InMemoryService_CopyPropertyValue_fails_for_missing_source_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_source_column();
        }

        [Test]
        public void InMemoryService_CopyPropertyValue_fails_for_different_types()
        {
            // ACT & ASSERT

            base.TreesorService_fails_CopyPropertyValue_for_different_types(7);
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