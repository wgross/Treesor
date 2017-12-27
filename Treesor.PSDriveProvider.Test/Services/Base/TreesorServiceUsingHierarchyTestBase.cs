using Elementary.Hierarchy;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Treesor.Abstractions;
using Treesor.Model;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Service.Base
{
    public class TreesorServiceUsingHierarchyTestBase
    {
        protected Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        protected ITreesorModel treesorService;

        #region NewItem > Add

        public void NewItem_creates_hierarchy_node_under_root()
        {
            // ACT

            this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Reference<Guid>>()), Times.Once());
        }

        public void NewItem_doesnt_accept_value()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), new object()));

            // ASSERT

            Assert.True(result.Message.Contains($"A value for node {TreesorNodePath.Create("item")} is not allowed"));
        }

        public void NewItem_fails_if_item_exists_already()
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Reference<Guid>>()))
                .Throws(new ArgumentException());

            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null));

            // ASSERT

            Assert.NotNull(result);
        }

        #endregion NewItem > Add

        #region ItemExists > TryGetValue

        public void ItemExists_tries_to_retrieve_existing_hierarchy_node()
        {
            // ARRANGE

            var value = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out value))
                .Returns(true);

            // ACT

            var result = this.treesorService.ItemExists(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.True(result);
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out value), Times.Once());
        }

        public void ItemExists_tries_to_retrieve_missing_hierarchy_node()
        {
            // ARRANGE

            var value = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out value))
                .Returns(false);

            // ACT

            var result = this.treesorService.ItemExists(TreesorNodePath.Create("item"));

            // ASSERT

            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out value), Times.Once());
        }

        #endregion ItemExists > TryGetValue

        #region GetItem > TryGetValue

        public void GetItem_retrieves_existing_node()
        {
            // ARRANGE

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            // ACT

            var result = this.treesorService.GetItem(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.NotNull(result);
            Assert.True(result.IsContainer);
            Assert.Equal(TreesorNodePath.Create("item"), result.Path);
            Assert.Equal(id.Value, result.Id);

            this.hierarchyMock.Verify(s => s.TryGetValue(HierarchyPath.Create("item"), out id), Times.Once());
        }

        public void GetItem_retrieves_missing_node_returns_null()
        {
            // ARRANGE

            Reference<Guid> id = new Reference<Guid>(Guid.Empty);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(false);

            // ACT

            var result = this.treesorService.GetItem(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.Null(result);

            this.hierarchyMock.Verify(s => s.TryGetValue(HierarchyPath.Create("item"), out id), Times.Once());
        }

        public void GetItem_fails_on_missing_node_path()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.GetItem(null));

            // ASSERT

            Assert.Equal("path", result.ParamName);
        }

        #endregion GetItem > TryGetValue

        #region SetItem: NotSupported

        public void SetItem_throws_NotSupportedException()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.SetItem(TreesorNodePath.Create("item"), null));

            // ASSERT

            Assert.True(result.Message.Contains("A value for node item is not allowed"));
        }

        #endregion SetItem: NotSupported

        #region ClearItem: Does nothing

        public void ClearItem_does_nothing()
        {
            // ACT

            this.treesorService.ClearItem(TreesorNodePath.Create("item"));
        }

        #endregion ClearItem: Does nothing

        #region RemoveItem > RemoveNode

        public void RemoveItem_removes_existing_node_under_root()
        {
            // ACT

            this.treesorService.RemoveItem(TreesorNodePath.Create("item"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.RemoveNode(HierarchyPath.Create("item"), false), Times.Once());
        }

        public void RemoveItem_removes_existing_node_under_root_recursively()
        {
            // ACT

            this.treesorService.RemoveItem(TreesorNodePath.Create("item"), true);

            // ASSERT

            this.hierarchyMock.Verify(h => h.RemoveNode(HierarchyPath.Create("item"), true), Times.Once());
        }

        #endregion RemoveItem > RemoveNode

        #region GetChildItems,GetDescendants > Traverse

        public void GetChildItems_retrieves_nodes_child_nodes()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b
            //             / \
            //            c   d

            var a = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> a_value;
            a.Setup(n => n.Value).Returns(a_value = new Reference<Guid>(Guid.NewGuid()));
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var c = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            c.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> c_value;
            c.Setup(n => n.Value).Returns(c_value = new Reference<Guid>(Guid.NewGuid()));
            c.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "c"));

            var d = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            d.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> d_value;
            d.Setup(n => n.Value).Returns(d_value = new Reference<Guid>(Guid.NewGuid()));
            d.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "d"));

            var b = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            b.Setup(n => n.HasChildNodes).Returns(true);
            b.Setup(n => n.ChildNodes).Returns(new[] { c.Object, d.Object });
            Reference<Guid> b_value;
            b.Setup(n => n.Value).Returns(b_value = new Reference<Guid>(Guid.NewGuid()));
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Reference<Guid> item_value;
            item.Setup(n => n.Value).Returns(item_value = new Reference<Guid>(Guid.NewGuid()));
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.GetChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.Equal(2, result.Count());
            Assert.Equal(TreesorNodePath.Create("item", "a"), result.ElementAt(0).Path);
            Assert.Equal(TreesorNodePath.Create("item", "b"), result.ElementAt(1).Path);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
        }

        public void GetDescendants_retrieves_nodes_descendant_nodes()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b
            //             / \
            //            c   d

            var a = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> a_value;
            a.Setup(n => n.Value).Returns(a_value = new Reference<Guid>(Guid.NewGuid()));
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var c = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            c.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> c_value;
            c.Setup(n => n.Value).Returns(c_value = new Reference<Guid>(Guid.NewGuid()));
            c.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b", "c"));

            var d = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            d.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> d_value;
            d.Setup(n => n.Value).Returns(d_value = new Reference<Guid>(Guid.NewGuid()));
            d.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b", "d"));

            var b = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            b.Setup(n => n.HasChildNodes).Returns(true);
            b.Setup(n => n.ChildNodes).Returns(new[] { c.Object, d.Object });
            Reference<Guid> b_value;
            b.Setup(n => n.Value).Returns(b_value = new Reference<Guid>(Guid.NewGuid()));
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Reference<Guid> item_value;
            item.Setup(n => n.Value).Returns(item_value = new Reference<Guid>(Guid.NewGuid()));
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.GetDescendants(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.Equal(4, result.Count());
            Assert.Equal(TreesorNodePath.Create("item", "a"), result.ElementAt(0).Path);
            Assert.Equal(TreesorNodePath.Create("item", "b"), result.ElementAt(1).Path);
            Assert.Equal(TreesorNodePath.Create("item", "b", "c"), result.ElementAt(2).Path);
            Assert.Equal(TreesorNodePath.Create("item", "b", "d"), result.ElementAt(3).Path);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
        }

        #endregion GetChildItems,GetDescendants > Traverse

        #region RenameItem > TryGetValue,Remove,Add

        public void RenameItem_creates_newItem_with_same_value_and_new_name()
        {
            // ARRAMGE

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out id))
                .Returns(false);

            this.hierarchyMock
                .Setup(h => h.Remove(HierarchyPath.Create("item"), null))
                .Returns(true);

            // ACT

            this.treesorService
                .RenameItem(TreesorNodePath.Create("item"), newName: "item2");

            // ASSERT

            Reference<Guid> id_read;

            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out id_read), Times.Once());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item2"), out id_read), Times.Once());
            this.hierarchyMock.Verify(h => h.Remove(HierarchyPath.Create("item"), null), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.IsAny<Reference<Guid>>()), Times.Once());
        }

        #endregion RenameItem > TryGetValue,Remove,Add

        #region CopyItem > TryGetValue,NewItem

        public void CopyItem_creates_new_destination_item()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Reference<Guid> destination_id;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.Is<Reference<Guid>>(g => g.Value != id.Value)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void CopyItem_creates_new_destination_item_recursivly()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b

            var a = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> a_value;
            a.Setup(n => n.Value).Returns(a_value = new Reference<Guid>(Guid.NewGuid()));
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var b = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            b.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> b_value;
            b.Setup(n => n.Value).Returns(b_value = new Reference<Guid>(Guid.NewGuid()));
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            Reference<Guid> item_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out item_id))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Reference<Guid> item_value;
            item.Setup(n => n.Value).Returns(item_value = new Reference<Guid>(Guid.NewGuid()));
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            Reference<Guid> destination_id;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), recurse: true);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.Is<Reference<Guid>>(g => g.Value != item_id.Value)), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "a"), It.Is<Reference<Guid>>(g => g.Value != a_value.Value)), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "b"), It.Is<Reference<Guid>>(g => g.Value != b_value.Value)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void CopyItem_creates_new_item_under_existing_destination_item()
        {
            // ARRANGE
            // item and item2 exist, item2/item doesn't exists

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Reference<Guid> destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Reference<Guid> new_destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "item"), It.Is<Reference<Guid>>(g => g.Value != id.Value)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void CopyItem_doesnt_do_anything_if_all_possible_destinations_exist()
        {
            // ARRANGE
            // item and item2 exist, item2/item exists

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Reference<Guid> destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Reference<Guid> new_destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(true);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(It.IsAny<HierarchyPath<string>>(), It.IsAny<Reference<Guid>>()), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        #endregion CopyItem > TryGetValue,NewItem

        #region MoveItem > TryGetValue,NewValue,Remove

        public void MoveItem_create_new_destination_item()
        {
            // ARRANGE

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out id))
                .Returns(false);

            this.hierarchyMock
                .Setup(h => h.Remove(HierarchyPath.Create("item"), null))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(false);
            item.Setup(n => n.Value).Returns(id);
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            this.treesorService.MoveItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"));

            // ASSERT

            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out id), Times.Once());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item2"), out id), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), id), Times.Once());
            this.hierarchyMock.Verify(h => h.Remove(HierarchyPath.Create("item"), null), Times.Once());
        }

        public void MoveItem_create_new_destination_item_recursively()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b

            var a = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> a_value;
            a.Setup(n => n.Value).Returns(a_value = new Reference<Guid>(Guid.NewGuid()));
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var b = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            b.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> b_value;
            b.Setup(n => n.Value).Returns(b_value = new Reference<Guid>(Guid.NewGuid()));
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            Reference<Guid> item_value = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out item_value))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            item.Setup(n => n.Value).Returns(item_value);
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            Reference<Guid> destination_id;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(false);

            // ACT

            this.treesorService.MoveItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"));

            // ASSERT
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out item_value), Times.Once());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item2"), out item_value), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), item_value), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "a"), a_value), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "b"), b_value), Times.Once());
            this.hierarchyMock.Verify(h => h.Remove(HierarchyPath.Create("item"), null), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void MoveItem_creates_new_item_under_existing_destination_item()
        {
            // ARRANGE
            // item and item2 exist, item2/item doesn't exists

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(false);
            item.Setup(n => n.Value).Returns(id);
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            Reference<Guid> destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Reference<Guid> new_destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(false);

            // ACT

            this.treesorService.MoveItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"));

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "item"), id), Times.Once());
            this.hierarchyMock.Verify(h => h.Remove(HierarchyPath.Create("item"), null), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void MoveItem_doesnt_do_anything_if_all_possible_destinations_exist()
        {
            // ARRANGE
            // item and item2 exist, item2/item exists

            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Reference<Guid> destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Reference<Guid> new_destination_id = new Reference<Guid>(Guid.NewGuid());
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(true);

            // ACT

            this.treesorService.MoveItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"));

            // ASSERT

            this.hierarchyMock.Verify(h => h.Remove(It.IsAny<HierarchyPath<string>>(), null), Times.Never());
            this.hierarchyMock.Verify(h => h.Remove(It.IsAny<HierarchyPath<string>>(), It.IsAny<int>()), Times.Never());
            this.hierarchyMock.Verify(h => h.Add(It.IsAny<HierarchyPath<string>>(), It.IsAny<Reference<Guid>>()), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        #endregion MoveItem > TryGetValue,NewValue,Remove

        #region HasChildItems > Traverse

        public void HasChildItems_check_item_under_root_returns_false()
        {
            // ARRANGE

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(false);
            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.HasChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.False(result);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void HasChildItems_check_item_under_root_returns_true()
        {
            // ARRANGE

            var item = new Mock<IHierarchyNode<string, Reference<Guid>>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            Reference<Guid> id = new Reference<Guid>(Guid.NewGuid());

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.HasChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.True(result);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        public void HasChildItems_check_missing_item_throws_KeyNotFoundException()
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Throws(new KeyNotFoundException());

            // ACT

            var result = Assert.Throws<KeyNotFoundException>(() => this.treesorService.HasChildItems(TreesorNodePath.Create("item")));

            // ASSERT

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        #endregion HasChildItems > Traverse
    }
}