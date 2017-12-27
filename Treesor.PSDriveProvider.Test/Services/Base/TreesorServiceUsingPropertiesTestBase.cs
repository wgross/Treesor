using Elementary.Hierarchy;
using Moq;
using NUnit.Framework;
using System;
using Treesor.Abstractions;

namespace Treesor.PSDriveProvider.Test.Service.Base
{
    public class TreesorServiceUsingPropertiesTestBase
    {
        protected Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        protected ITreesorService treesorService;

        #region SetPropertyValue

        public void TreesorService_sets_property_value(Guid nodeId, string value)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column = this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: value);

            // ASSERT

            Assert.AreEqual(value, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), column.Name));

            this.hierarchyMock.VerifyAll();
        }

        public void TreesorService_adds_second_property_value(Guid nodeId, string p_value, int q_value)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column1 = this.treesorService.CreateColumn(name: "p", type: typeof(string));
            var column2 = this.treesorService.CreateColumn(name: "q", type: typeof(int));

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: p_value);

            // ACT

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: q_value);

            // ASSERT

            Assert.AreEqual(p_value, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), column1.Name));
            Assert.AreEqual(q_value, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), column2.Name));

            this.hierarchyMock.VerifyAll();
        }

        public void TreesorService_changes_property_value(Guid nodeId, string value, string newValue)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column = treesorService.CreateColumn(name: "p", type: typeof(string));
            column.SetValue(id, value);

            // ACT

            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: newValue);

            // ASSERT

            Assert.AreEqual(newValue, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_SetPropertyValue_with_wrong_type(Guid nodeId, string value, int wrongValue)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.CreateColumn(name: "p", type: typeof(string));
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), "p", value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: wrongValue));

            // ASSERT

            Assert.AreEqual(value, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
            Assert.AreEqual($"Couldn't assign value '{wrongValue}'(type='System.Int32') to property 'p' at node '{id.Value}': value.GetType() must be 'System.String'", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_SetPropertyValue_with_missing_column(Guid nodeId)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual($"Property 'p' doesn't exist", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_SetPropertyValue_at_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            var column = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual($"Node 'a' doesn't exist", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_SetPropertyValue_with_missing_property_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.SetPropertyValue(TreesorNodePath.Create("a"), null, "value"));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        public void TreesorService_fails_on_SetPropertyValue_with_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.SetPropertyValue(null, "p", "value"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion SetPropertyValue

        #region GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        public void TreesorService_fails_on_GetPropertyValue_at_missing_column()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);

            var id = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out id), Times.Never());
        }

        public void TreesorService_fails_on_GetPropertyValue_for_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            treesorService.CreateColumn("p", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_GetPropertyValue_with_missing_property_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.GetPropertyValue(TreesorNodePath.Create("a"), null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        public void TreesorService_fails_on_GetPropertyValue_with_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.GetPropertyValue(null, "p"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        #region ClearPropertyValue

        public void TreesorService_clears_property_value(Guid nodeId, int value)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: value);

            // ACT

            treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_clears_second_property_value(Guid nodeId, int p_value, string q_value)
        {
            // ARRANGE

            var id = Reference.To(nodeId);

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(string));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: p_value);
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: q_value);

            // ACT

            treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.AreEqual(p_value, treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_ClearPropertyValue_for_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
        }

        public void TreesorService_fails_on_ClearPropertyValue_at_missing_node()
        {
            // ARRANGE

            Reference<Guid> id = null;
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(false);

            treesorService.CreateColumn("p", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_ClearPropertyValue_with_missing_column_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), null));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        public void TreesorService_fails_on_ClearPropertyValue_with_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.ClearPropertyValue(null, "p"));

            // ASSERT

            Assert.AreEqual("path", result.ParamName);
        }

        #endregion ClearPropertyValue

        #region CopyPropertyValue

        public void TreesorService_copies_property_value_from_root_to_child(Guid rootId, Guid childId, string root_p_value)
        {
            // ARRANGE

            var id_root = Reference.To(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(string));
            treesorService.CreateColumn("q", typeof(string));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: root_p_value);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.AreEqual(root_p_value, (string)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(root_p_value, (string)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_copies_property_value_within_same_node(Guid nodeId, int node_p_value)
        {
            // ARRANGE

            var id_root = Reference.To(nodeId);

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: node_p_value);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.AreEqual(node_p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(node_p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_copies_property_value_from_child_to_root(Guid rootId, Guid childId, int child_q_value)
        {
            // ARRANGE

            var id_root = Reference.To(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = Reference.To(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("q", typeof(int));
            treesorService.CreateColumn("p", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: child_q_value);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(child_q_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(child_q_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_CopyPropertyValue_at_missing_destination_node(Guid nodeId, int p_value)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(nodeId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: p_value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_CopyPropertyValue_for_missing_destination_column(Guid rootId, Guid childId, int root_p_value)
        {
            // ARRANGE

            var id_root = Reference.To(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = Reference.To(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: root_p_value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'q' doesn't exist", result.Message);
            Assert.AreEqual(root_p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_CopyPropertyValues_for_missing_source_node()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.CopyPropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_CopyPropertyValue_for_missing_source_column()
        {
            // ARRANGE

            var id_a = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("q", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            var id = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out id), Times.Never());
            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_CopyPropertyValue_for_different_types(Guid rootId, Guid childId, int root_p_value)
        {
            // ARRANGE

            var id_root = Reference.To(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(double));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: root_p_value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '{root_p_value}'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
            Assert.AreEqual(root_p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        public void TreesorService_moves_property_value_from_root_to_child(Guid rootId, Guid childId, int root_p_value)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: root_p_value);

            // ACT

            treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(root_p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_moves_values_between_properties_of_same_node(Guid nodeId, int p_value)
        {
            // ARRANGE

            var id_root = Reference.To(nodeId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: p_value);

            // ACT

            treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_moves_property_value_from_child_to_root(Guid rootId, Guid childId, int child_q_value)
        {
            // ARRANGE

            var id_root = Reference.To(rootId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = Reference.To(childId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            this.treesorService.CreateColumn("p", typeof(int));
            this.treesorService.CreateColumn("q", typeof(int));
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: child_q_value);

            // ACT

            this.treesorService.MovePropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(child_q_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_MovePropertyValue_for_missing_destination_node(Guid nodeId, int p_value)
        {
            // ARRANGE

            var id_root = Reference.To(nodeId);
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: p_value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(p_value, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_MovePropertyValue_for_missing_destination_column()
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'q' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_fails_for_missing_source_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.MovePropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_fails_for_missing_source_column(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_a = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("q", typeof(int));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            var id = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out id), Times.Never());
            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_fails_for_different_types(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            var id_a = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id_a))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(double));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        #endregion MovePropertyValue
    }
}