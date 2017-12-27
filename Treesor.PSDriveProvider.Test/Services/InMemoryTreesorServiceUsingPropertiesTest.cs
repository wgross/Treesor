using Elementary.Hierarchy;
using Moq;
using System;
using Treesor.Abstractions;
using Treesor.PSDriveProvider.Test.Service.Base;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Service
{
    public class InMemoryTreesorServiceUsingPropertiesTest : TreesorServiceUsingPropertiesTestBase
    {
        public InMemoryTreesorServiceUsingPropertiesTest()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new InMemoryTreesorService(this.hierarchyMock.Object);
        }

        #region SetPropertyValue

        [Fact]
        public void InMemoryService_sets_property_value()
        {
            // ACT & ASSERT

            base.TreesorService_sets_property_value(nodeId: Guid.NewGuid(), value: "value");
        }

        [Fact]
        public void InMemoryService_adds_second_property_value()
        {
            // ACT & ASSERT

            base.TreesorService_adds_second_property_value(nodeId: Guid.NewGuid(), p_value: "value", q_value: 2);
        }

        [Fact]
        public void InMemoryService_changes_property_value()
        {
            // ACT

            base.TreesorService_changes_property_value(Guid.NewGuid(), value: "value", newValue: "new value");
        }

        [Fact]
        public void InMemoryService_fails_on_SetPropertyValue_with_wrong_type()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_wrong_type(Guid.NewGuid(), "value", 3);
        }

        [Fact]
        public void InMemoryService_fails_on_SetPropertyValue_fon_missing_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_column(Guid.NewGuid());
        }

        [Fact]
        public void InMemoryService_fails_on_SetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_at_missing_node();
        }

        [Fact]
        public void InMemoryService_fails_on_SetPropertyValue_on_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_property_name();
        }

        [Fact]
        public void InMemoryService_fails_on_SetPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_node_path();
        }

        #endregion SetPropertyValue

        #region GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        [Fact]
        public void InMemoryService_fails_on_GetPropertyValue_at_missing_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_at_missing_column();
        }

        [Fact]
        public void InMemoryService_fails_on_GetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_for_missing_node();
        }

        [Fact]
        public void InMemoryService_fails_on_GetPropertyValue_with_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_property_name();
        }

        [Fact]
        public void InMemoryService_GetPropertyValue_fails_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_node_path();
        }

        #endregion GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        #region ClearPropertyValue

        [Fact]
        public void InMemoryService_clears_property_value()
        {
            // ACT & ASSSERT

            base.TreesorService_clears_property_value(Guid.NewGuid(), 2);
        }

        [Fact]
        public void InMemoryService_clears_second_property_value()
        {
            // ACT & ASSSERT

            base.TreesorService_clears_second_property_value(Guid.NewGuid(), p_value: 5, q_value: "value");
        }

        [Fact]
        public void InMemoryService_fails_on_ClearPropertyValue_for_missing_column()
        {
            // ACT & ARRANGE

            base.TreesorService_fails_on_ClearPropertyValue_for_missing_column();
        }

        [Fact]
        public void InMemoryService_fails_on_ClearPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_at_missing_node();
        }

        [Fact]
        public void InMemoryService_fails_ClearPropertyValue_fails_for_missing_column_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_column_name();
        }

        [Fact]
        public void InMemoryService_fails_on_ClearPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_node_path();
        }

        #endregion ClearPropertyValue

        #region CopyPropertyValue

        [Fact]
        public void InMemoryService_copies_property_value_from_root_to_child()
        {
            // ACT & ARRANGE

            base.TreesorService_copies_property_value_from_root_to_child(Guid.NewGuid(), Guid.NewGuid(), "value");
        }

        [Fact]
        public void TreesorService_copies_property_value_within_same_node()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_within_same_node(Guid.NewGuid(), node_p_value: 5);
        }

        [Fact]
        public void InMemomryService_copies_property_value_from_child_to_root()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_from_child_to_root(Guid.NewGuid(), Guid.NewGuid(), 6);
        }

        [Fact]
        public void InMemoryService_fails_on_CopyPropertyValue_at_missing_destination_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_at_missing_destination_node(Guid.NewGuid(), 7);
        }

        [Fact]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_destination_column(Guid.NewGuid(), Guid.NewGuid(), 7);
        }

        [Fact]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_source_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValues_for_missing_source_node();
        }

        [Fact]
        public void InMemoryService_CopyPropertyValue_fails_for_missing_source_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_source_column();
        }

        [Fact]
        public void InMemoryService_CopyPropertyValue_fails_for_different_types()
        {
            // ACT & ASSERT

            base.TreesorService_fails_CopyPropertyValue_for_different_types(Guid.NewGuid(), Guid.NewGuid(), 7);
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        [Fact]
        public void InMemoryService_MovePropertyValue_from_root_to_child()
        {
            // ACT & ASSERT

            base.TreesorService_moves_property_value_from_root_to_child(Guid.NewGuid(), Guid.NewGuid(), 7);
        }

        [Fact]
        public void InMemoryService_moves_values_between_properties_of_same_node()
        {
            // ACT & ASSERT

            base.TreesorService_moves_values_between_properties_of_same_node(Guid.NewGuid(), 6);
        }

        [Fact]
        public void InMemoryService_moves_property_value_from_child_to_root()
        {
            // ACT

            base.TreesorService_moves_property_value_from_child_to_root(Guid.NewGuid(), Guid.NewGuid(), 6);
        }

        [Fact]
        public void InMemoryService_fails_on_MovePropertyValue_for_missing_destination_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_node(Guid.NewGuid(), 6);
        }

        [Fact]
        public void InMemoryService_fails_on_MovePropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_column();
        }

        [Fact]
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

            Assert.Equal("Node 'a' doesn't exist", result.Message);
            Assert.Null(this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            this.hierarchyMock.VerifyAll();
        }

        [Fact]
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

            Assert.Equal("Property 'p' doesn't exist", result.Message);
            Assert.Null(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out id), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        [Fact]
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

            Assert.Equal($"Couldn't assign value '5'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
            Assert.Equal(5, (int)this.treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.Null(this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            this.hierarchyMock.VerifyAll();
        }

        #endregion MovePropertyValue
    }
}