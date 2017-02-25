using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;

namespace Treesor.PSDriveProvider.Test.Service.Base
{
    public class TreesorServiceUsingPropertiesTestBase
    {
        protected Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        protected ITreesorService treesorService;

        #region SetPropertyValue

        public void TreesorService_sets_property_value(Reference<Guid> id, string value)
        {
            // ARRANGE

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

        public void TreesorService_adds_second_property_value(Reference<Guid> id, string value1, int value2)
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            var column1 = this.treesorService.CreateColumn(name: "string", type: typeof(string));
            var column2 = this.treesorService.CreateColumn(name: "int", type: typeof(int));

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "string", value: value1);

            // ACT

            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "int", value: value2);

            // ASSERT

            Assert.AreEqual(value1, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), column1.Name));
            Assert.AreEqual(value2, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), column2.Name));

            this.hierarchyMock.VerifyAll();
        }

        public void TreesorService_changes_property_value(Reference<Guid> id, string value, string newValue)
        {
            // ARRANGE

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

        public void TreesorService_fails_on_SetPropertyValue_with_wrong_type(Reference<Guid> id, string value)
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            this.treesorService.CreateColumn(name: "p", type: typeof(string));
            this.treesorService.SetPropertyValue(TreesorNodePath.Create("a"), "p", value);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5));

            // ASSERT

            Assert.AreEqual(value, this.treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
            Assert.AreEqual($"Couldn't assign value '5'(type='System.Int32') to property 'p' at node '{id.Value}': value.GetType() must be 'System.String'", result.Message);

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_fails_on_SetPropertyValue_with_missing_column(Reference<Guid> id)
        {
            // ARRANGE

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

        public void TreesorService_clears_property_value(Reference<Guid> id)
        {
            // ARRANGE

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: 5);

            // ACT

            treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "p");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));

            hierarchyMock.VerifyAll();
        }

        public void TreesorService_clears_second_property_value(Reference<Guid> id, int value1, string value2)
        {
            // ARRANGE

            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out id))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(string));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "p", value: value1);
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: value2);

            // ACT

            treesorService.ClearPropertyValue(TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.AreEqual(value1, treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "p"));
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

        public void CopyPropertyValue_from_root_to_child(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void CopyPropertyValue_at_same_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            hierarchyMock.VerifyAll();
        }

        public void CopyPropertyValue_from_child_to_root(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: 5);

            // ACT

            treesorService.CopyPropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void CopyPropertyValue_fails_for_missing_destination_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void CopyPropertyValue_fails_for_missing_destination_column(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Property 'q' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void CopyPropertyValue_fails_for_missing_source_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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

        public void CopyPropertyValue_fails_for_missing_source_column(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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

        public void CopyPropertyValue_fails_for_different_types(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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
                treesorService.CopyPropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual($"Couldn't assign value '5'(type='System.Int32') to property 'q' at node '{id_a.Value.ToString()}': value.GetType() must be 'System.Double'", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        public void MovePropertyValue_from_root_to_child(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_at_same_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.RootPath, "q");

            // ASSERT

            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "q"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_from_child_to_root(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.Create("a"), name: "q", value: 5);

            // ACT

            treesorService.MovePropertyValue(TreesorNodePath.Create("a"), "q", TreesorNodePath.Create(), "p");

            // ASSERT

            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));
            Assert.IsNull(treesorService.GetPropertyValue(TreesorNodePath.Create("a"), "q"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_fails_for_missing_destination_node(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
        {
            // ARRANGE

            var id_root = new Reference<Guid>(Guid.NewGuid());
            hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out id_root))
                .Returns(true);

            treesorService.CreateColumn("p", typeof(int));
            treesorService.CreateColumn("q", typeof(int));
            treesorService.SetPropertyValue(TreesorNodePath.RootPath, name: "p", value: 5);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.MovePropertyValue(TreesorNodePath.RootPath, "p", TreesorNodePath.Create("a"), "q"));

            // ASSERT

            Assert.AreEqual("Node 'a' doesn't exist", result.Message);
            Assert.AreEqual(5, (int)treesorService.GetPropertyValue(TreesorNodePath.Create(), "p"));

            hierarchyMock.VerifyAll();
        }

        public void MovePropertyValue_fails_for_missing_destination_column(Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock, ITreesorService treesorService)
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