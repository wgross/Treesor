using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class TreesorServiceUsingHierarchyTest
    {
        private Mock<IHierarchy<string, Guid>> hierarchyMock;
        private TreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Guid>>();
            this.treesorService = new TreesorService(this.hierarchyMock.Object);
        }

        #region NewItem > Add

        [Test]
        public void NewItem_creates_hierarchy_node_under_root()
        {
            // ACT

            this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Guid>()), Times.Once());
        }

        [Test]
        public void NewItem_doesnt_accept_value()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), new object()));

            // ASSERT

            Assert.IsTrue(result.Message.Contains($"A value for node {TreesorNodePath.Create("item")} is not allowed"));
        }

        [Test]
        public void NewItem_fails_if_item_exists_already()
        {
            // ARRANGE

            this.hierarchyMock
                .Setup(h => h.Add(HierarchyPath.Create("item"), It.IsAny<Guid>()))
                .Throws(new ArgumentException());

            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorService.NewItem(TreesorNodePath.Create("item"), newItemValue: null));

            // ASSERT

            Assert.NotNull(result);
        }

        #endregion NewItem > Add

        #region ItemExists > TryGetValue

        [Test]
        public void ItemExists_tries_to_retrieve_existing_hierarchy_node()
        {
            // ARRANGE

            var value = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out value))
                .Returns(true);

            // ACT

            var result = this.treesorService.ItemExists(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.IsTrue(result);
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out value), Times.Once());
        }

        [Test]
        public void ItemExists_tries_to_retrieve_missing_hierarchy_node()
        {
            // ARRANGE

            var value = Guid.NewGuid();
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

        [Test]
        public void GetItem_retrieves_existing_node()
        {
            // ARRANGE

            Guid id = Guid.NewGuid();

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            // ACT

            var result = this.treesorService.GetItem(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.NotNull(result);
            Assert.IsTrue(result.IsContainer);
            Assert.AreEqual(TreesorNodePath.Create("item"), result.Path);
            Assert.AreEqual(id, result.Id);

            this.hierarchyMock.Verify(s => s.TryGetValue(HierarchyPath.Create("item"), out id), Times.Once());
        }

        [Test]
        public void GetItem_retrieves_missing_node_returns_null()
        {
            // ARRANGE

            Guid id = Guid.Empty;

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(false);

            // ACT

            var result = this.treesorService.GetItem(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.Null(result);

            this.hierarchyMock.Verify(s => s.TryGetValue(HierarchyPath.Create("item"), out id), Times.Once());
        }

        #endregion GetItem > TryGetValue

        #region SetItem: NotSupported

        [Test]
        public void SetItem_throws_NotSupportedException()
        {
            // ACT

            var result = Assert.Throws<NotSupportedException>(() => this.treesorService.SetItem(TreesorNodePath.Create("item"), null));

            // ASSERT

            Assert.True(result.Message.Contains("A value for node item is not allowed"));
        }

        #endregion SetItem: NotSupported

        #region ClearItem: Does nothing

        [Test]
        public void ClearItem_does_nothing()
        {
            // ACT

            Assert.DoesNotThrow(() => this.treesorService.ClearItem(TreesorNodePath.Create("item")));
        }

        #endregion ClearItem: Does nothing

        #region RemoveItem > RemoveNode

        [Test]
        public void RemoveItem_removes_existing_node_under_root()
        {
            // ACT

            this.treesorService.RemoveItem(TreesorNodePath.Create("item"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.RemoveNode(HierarchyPath.Create("item"), false), Times.Once());
        }

        [Test]
        public void RemoveItem_removes_existing_node_under_root_recursively()
        {
            // ACT

            this.treesorService.RemoveItem(TreesorNodePath.Create("item"), true);

            // ASSERT

            this.hierarchyMock.Verify(h => h.RemoveNode(HierarchyPath.Create("item"), true), Times.Once());
        }

        #endregion RemoveItem > RemoveNode

        #region GetChildItems,GetDescendants > Traverse

        [Test]
        public void GetChildItems_retrieves_nodes_child_nodes()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b
            //             / \
            //            c   d

            var a = new Mock<IHierarchyNode<string, Guid>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Guid a_value;
            a.Setup(n => n.Value).Returns(a_value = Guid.NewGuid());
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var c = new Mock<IHierarchyNode<string, Guid>>();
            c.Setup(n => n.HasChildNodes).Returns(false);
            Guid c_value;
            c.Setup(n => n.Value).Returns(c_value = Guid.NewGuid());
            c.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "c"));

            var d = new Mock<IHierarchyNode<string, Guid>>();
            d.Setup(n => n.HasChildNodes).Returns(false);
            Guid d_value;
            d.Setup(n => n.Value).Returns(d_value = Guid.NewGuid());
            d.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "d"));

            var b = new Mock<IHierarchyNode<string, Guid>>();
            b.Setup(n => n.HasChildNodes).Returns(true);
            b.Setup(n => n.ChildNodes).Returns(new[] { c.Object, d.Object });
            Guid b_value;
            b.Setup(n => n.Value).Returns(b_value = Guid.NewGuid());
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Guid item_value;
            item.Setup(n => n.Value).Returns(item_value = Guid.NewGuid());
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.GetChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(TreesorNodePath.Create("item", "a"), result.ElementAt(0).Path);
            Assert.AreEqual(TreesorNodePath.Create("item", "b"), result.ElementAt(1).Path);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
        }

        [Test]
        public void GetDescendants_retrieves_nodes_descendant_nodes()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b
            //             / \
            //            c   d

            var a = new Mock<IHierarchyNode<string, Guid>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Guid a_value;
            a.Setup(n => n.Value).Returns(a_value = Guid.NewGuid());
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var c = new Mock<IHierarchyNode<string, Guid>>();
            c.Setup(n => n.HasChildNodes).Returns(false);
            Guid c_value;
            c.Setup(n => n.Value).Returns(c_value = Guid.NewGuid());
            c.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b", "c"));

            var d = new Mock<IHierarchyNode<string, Guid>>();
            d.Setup(n => n.HasChildNodes).Returns(false);
            Guid d_value;
            d.Setup(n => n.Value).Returns(d_value = Guid.NewGuid());
            d.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b", "d"));

            var b = new Mock<IHierarchyNode<string, Guid>>();
            b.Setup(n => n.HasChildNodes).Returns(true);
            b.Setup(n => n.ChildNodes).Returns(new[] { c.Object, d.Object });
            Guid b_value;
            b.Setup(n => n.Value).Returns(b_value = Guid.NewGuid());
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Guid item_value;
            item.Setup(n => n.Value).Returns(item_value = Guid.NewGuid());
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.GetDescendants(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.AreEqual(4, result.Count());
            Assert.AreEqual(TreesorNodePath.Create("item", "a"), result.ElementAt(0).Path);
            Assert.AreEqual(TreesorNodePath.Create("item", "b"), result.ElementAt(1).Path);
            Assert.AreEqual(TreesorNodePath.Create("item", "b", "c"), result.ElementAt(2).Path);
            Assert.AreEqual(TreesorNodePath.Create("item", "b", "d"), result.ElementAt(3).Path);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
        }

        #endregion GetChildItems,GetDescendants > Traverse

        #region RenameItem > TryGetValue,Remove,Add

        [Test]
        public void RenameItem_creates_newItem_with_same_value_and_new_name()
        {
            // ARRAMGE

            Guid id = Guid.NewGuid();

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

            Guid id_read;

            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item"), out id_read), Times.Once());
            this.hierarchyMock.Verify(h => h.TryGetValue(HierarchyPath.Create("item2"), out id_read), Times.Once());
            this.hierarchyMock.Verify(h => h.Remove(HierarchyPath.Create("item"), null), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.IsAny<Guid>()), Times.Once());
        }

        #endregion RenameItem > TryGetValue,Remove,Add

        #region CopyItem > TryGetValue,NewItem

        [Test]
        public void CopyItem_creates_new_destination_item()
        {
            // ARRANGE

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Guid destination_id;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.Is<Guid>(g => g != id)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyItem_creates_new_destination_item_recursivly()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b

            var a = new Mock<IHierarchyNode<string, Guid>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Guid a_value;
            a.Setup(n => n.Value).Returns(a_value = Guid.NewGuid());
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var b = new Mock<IHierarchyNode<string, Guid>>();
            b.Setup(n => n.HasChildNodes).Returns(false);
            Guid b_value;
            b.Setup(n => n.Value).Returns(b_value = Guid.NewGuid());
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            Guid item_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out item_id))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            Guid item_value;
            item.Setup(n => n.Value).Returns(item_value = Guid.NewGuid());
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            Guid destination_id;
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), recurse: true);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2"), It.Is<Guid>(g => g != item_id)), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "a"), It.Is<Guid>(g => g != a_value)), Times.Once());
            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "b"), It.Is<Guid>(g => g != b_value)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyItem_creates_new_item_under_existing_destination_item()
        {
            // ARRANGE
            // item and item2 exist, item2/item doesn't exists

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Guid destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Guid new_destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(false);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(HierarchyPath.Create("item2", "item"), It.Is<Guid>(g => g != id)), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void CopyItem_doesnt_do_anything_if_all_possible_destinations_exist()
        {
            // ARRANGE
            // item and item2 exist, item2/item exists

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Guid destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Guid new_destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(true);

            // ACT

            this.treesorService.CopyItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"), false);

            // ASSERT

            this.hierarchyMock.Verify(h => h.Add(It.IsAny<HierarchyPath<string>>(), It.IsAny<Guid>()), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        #endregion CopyItem > TryGetValue,NewItem

        #region MoveItem > TryGetValue,NewValue,Remove

        [Test]
        public void MoveItem_create_new_destination_item()
        {
            // ARRANGE

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out id))
                .Returns(false);

            this.hierarchyMock
                .Setup(h => h.Remove(HierarchyPath.Create("item"), null))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Guid>>();
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

        [Test]
        public void MoveItem_create_new_destination_item_recursively()
        {
            // ARRANGE
            //         item
            //        /    \
            //       a      b

            var a = new Mock<IHierarchyNode<string, Guid>>();
            a.Setup(n => n.HasChildNodes).Returns(false);
            Guid a_value;
            a.Setup(n => n.Value).Returns(a_value = Guid.NewGuid());
            a.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "a"));

            var b = new Mock<IHierarchyNode<string, Guid>>();
            b.Setup(n => n.HasChildNodes).Returns(false);
            Guid b_value;
            b.Setup(n => n.Value).Returns(b_value = Guid.NewGuid());
            b.Setup(n => n.Path).Returns(HierarchyPath.Create("item", "b"));

            Guid item_value = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out item_value))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            item.Setup(n => n.ChildNodes).Returns(new[] { a.Object, b.Object });
            item.Setup(n => n.Value).Returns(item_value);
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            Guid destination_id;
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

        [Test]
        public void MoveItem_creates_new_item_under_existing_destination_item()
        {
            // ARRANGE
            // item and item2 exist, item2/item doesn't exists

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(false);
            item.Setup(n => n.Value).Returns(id);
            item.Setup(n => n.Path).Returns(HierarchyPath.Create("item"));

            Guid destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Guid new_destination_id = Guid.NewGuid();
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

        [Test]
        public void MoveItem_doesnt_do_anything_if_all_possible_destinations_exist()
        {
            // ARRANGE
            // item and item2 exist, item2/item exists

            Guid id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item"), out id))
                .Returns(true);

            Guid destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2"), out destination_id))
                .Returns(true);

            Guid new_destination_id = Guid.NewGuid();
            this.hierarchyMock
                .Setup(h => h.TryGetValue(HierarchyPath.Create("item2", "item"), out new_destination_id))
                .Returns(true);

            // ACT

            this.treesorService.MoveItem(TreesorNodePath.Create("item"), TreesorNodePath.Create("item2"));

            // ASSERT

            this.hierarchyMock.Verify(h => h.Remove(It.IsAny<HierarchyPath<string>>(), null), Times.Never());
            this.hierarchyMock.Verify(h => h.Remove(It.IsAny<HierarchyPath<string>>(), It.IsAny<int>()), Times.Never());
            this.hierarchyMock.Verify(h => h.Add(It.IsAny<HierarchyPath<string>>(), It.IsAny<Guid>()), Times.Never());
            this.hierarchyMock.VerifyAll();
        }

        #endregion MoveItem > TryGetValue,NewValue,Remove

        #region HasChildItems > Traverse

        [Test]
        public void HasChildItems_check_item_under_root_returns_false()
        {
            // ARRANGE

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(false);
            Guid id = Guid.NewGuid();

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.HasChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.IsFalse(result);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
        public void HasChildItems_check_item_under_root_returns_true()
        {
            // ARRANGE

            var item = new Mock<IHierarchyNode<string, Guid>>();
            item.Setup(n => n.HasChildNodes).Returns(true);
            Guid id = Guid.NewGuid();

            this.hierarchyMock
                .Setup(h => h.Traverse(HierarchyPath.Create("item")))
                .Returns(item.Object);

            // ACT

            var result = this.treesorService.HasChildItems(TreesorNodePath.Create("item"));

            // ASSERT

            Assert.IsTrue(result);

            this.hierarchyMock.Verify(h => h.Traverse(HierarchyPath.Create("item")), Times.Once());
            this.hierarchyMock.VerifyAll();
        }

        [Test]
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