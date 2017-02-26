using Elementary.Hierarchy.Collections;
using LiteDB;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Treesor.PSDriveProvider.Test.Service.Base;

namespace Treesor.PSDriveProvider.Test.Services
{
    public class LiteDbTreesorServiceUsingPropertiesTest : TreesorServiceUsingPropertiesTestBase
    {
        private const string _id = nameof(_id);
        private LiteDatabase database;
        private MemoryStream databaseStream;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .EnsureIndex(c => c.Name, unique: true);
            //this.database
            //    .GetCollection<LiteDbTreesorService.ColumValue>(LiteDbTreesorService.value_collection)
            //    .EnsureIndex();

            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new LiteDbTreesorService(this.hierarchyMock.Object, this.database);
        }

        #region SetPropertyValue

        [Test]
        public void LiteDbService_sets_property_value()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT

            base.TreesorService_sets_property_value(nodeId, "value");

            // ASSERT
            // verify storage structure: single columns single value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            var nodeValues = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.AreEqual(nodeId, nodeValues.Get("_id").AsGuid);
            Assert.AreEqual("value", nodeValues.Get(column.Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_adds_second_property_value()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT

            base.TreesorService_adds_second_property_value(nodeId, p_value: "value", q_value: 2);

            // ASSERT
            // verify storage structure: single columns single value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var nodeValues = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual(nodeId, nodeValues.Get(_id).AsGuid);
            Assert.AreEqual((object)"value", nodeValues.Get(columns.Single(c => c.Name.Equals("p")).Id.ToString()).RawValue);
            Assert.AreEqual(2, nodeValues.Get(columns.Single(c => c.Name.Equals("q")).Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_changes_property_value()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_changes_property_value(nodeId, value: "value", newValue: "new value");

            // ASSERT
            // verify storage structure: single columns single value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            var nodeValues = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.AreEqual(nodeId, nodeValues.Get(_id).RawValue);
            Assert.AreEqual("new value", nodeValues.Get(column.Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_fails_on_SetPropertyValue_with_wrong_type()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_wrong_type(id, "value");

            // ASSERT
            // verify storage structure: single columns single value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            Assert.AreEqual((object)"value", this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single()
                .Get(column.Id.ToString())
                .RawValue);
        }

        [Test]
        public void LiteDbService_fails_on_SetPropertyValue_with_missing_column()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_column(id);

            // ASSERT
            // no values have been created

            Assert.IsFalse(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Test]
        public void LiteDbService_fails_on_SetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_at_missing_node();

            // ASSERT
            // no values have been created

            Assert.IsFalse(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Test]
        public void LiteDbService_fails_on_SetPropertyValue_on_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_property_name();

            // ASSERT
            // no values have been created

            Assert.IsFalse(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Test]
        public void LiteDbService_fails_on_SetPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_node_path();

            // ASSERT
            // no values have been created

            Assert.IsFalse(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        #endregion SetPropertyValue

        #region GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        [Test]
        public void LiteDbService_fails_on_GetPropertyValue_at_missing_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_at_missing_column();
        }

        [Test]
        public void LiteDbService_fails_on_GetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_for_missing_node();
        }

        [Test]
        public void LiteDbService_fails_on_GetPropertyValue_with_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_property_name();
        }

        #endregion GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        #region ClearProperty

        [Test]
        public void LiteDbService_clears_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSSERT

            base.TreesorService_clears_property_value(id);

            // ASSERT
            // verify storage structure: single columns, no value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            Assert.IsFalse(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single()
                .ContainsKey(column.Id.ToString()));
        }

        [Test]
        public void LiteDbService_clears_second_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSSERT

            base.TreesorService_clears_second_property_value(id, 5, "value");

            // ASSERT
            // verify storage structure: single columns, no value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll().ToArray();

            Assert.AreEqual(5, this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single()
                .Get(columns[0].Id.ToString())
                .AsInt32);

            Assert.IsFalse(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single()
                .ContainsKey(columns[1].Id.ToString()));
        }

        [Test]
        public void LiteDbService_fails_on_ClearPropertyValue_for_missing_column()
        {
            // ACT & ARRANGE

            base.TreesorService_fails_on_ClearPropertyValue_for_missing_column();
        }

        [Test]
        public void LiteDbService_fails_on_ClearPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_at_missing_node();
        }

        [Test]
        public void LiteDbService_fails_ClearPropertyValue_fails_for_missing_column_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_column_name();
        }

        [Test]
        public void LiteDbService_fails_on_ClearPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_node_path();
        }

        #endregion ClearProperty

        #region CopyPropertyValue

        [Test]
        public void LiteDbService_copies_property_value_from_root_to_child()
        {
            // ACT & ARRANGE

            base.TreesorService_copies_property_value_from_root_to_child("value");

            // ASSERT
            // both items hav same value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.AreEqual(2, items.Count());
            Assert.AreEqual("value", items[0].Get(columns[0].Id.ToString()).RawValue);
            Assert.AreEqual("value", items[1].Get(columns[1].Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_copies_property_value_within_same_node()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_within_same_node(5);

            // ASSERT

            var columns = this.database
               .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
               .FindAll()
               .ToArray();

            var item = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.AreEqual(5, item.Get(columns[0].Id.ToString()).RawValue);
            Assert.AreEqual(5, item.Get(columns[1].Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_copies_property_value_from_child_to_root()
        {
            // ACT & ASSERT

            base.TreesorService_copies_property_value_from_child_to_root(7);

            // ASSERT

            var columns = this.database
               .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
               .FindAll()
               .ToArray();

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.AreEqual(7, items[0].Get(columns[0].Id.ToString()).RawValue);
            Assert.AreEqual(7, items[1].Get(columns[1].Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_fails_on_CopyPropertyValue_at_missing_destination_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_at_missing_destination_node(7);

            // ASSERT
            // an item wasn't created

            Assert.AreEqual(1, this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Count());
        }

        [Test]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_destination_column(7);

            // ASSERT
            // destination item wasn't created

            Assert.AreEqual(1, this.database
                .GetCollection(LiteDbTreesorService.column_collection)
                .FindAll()
                .Count());

            Assert.AreEqual(1, this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Count());
        }

        [Test]
        public void LiteDbService_fails_on_CopyPropertyValue_for_missing_source_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValues_for_missing_source_node();

            // ASSERT
            // two columns but neither destination nor source itenes exist.

            Assert.AreEqual(2, this.database
                .GetCollection(LiteDbTreesorService.column_collection)
                .FindAll()
                .Count());

            Assert.IsFalse(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Any());
        }

        [Test]
        public void LiteDbService_CopyPropertyValue_fails_for_missing_source_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_source_column();

            // ASSERT
            // not value items do exist

            Assert.IsFalse(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Any());
        }

        [Test]
        public void LiteDbService_CopyPropertyValue_fails_for_different_types()
        {
            // ACT & ASSERT

            base.TreesorService_fails_CopyPropertyValue_for_different_types(7);

            // ASSERT
            // two columns, one items

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.AreEqual(7, this.database
              .GetCollection(LiteDbTreesorService.value_collection)
              .FindAll()
              .Single()
              .Get(columns[0].Id.ToString())
              .RawValue);
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        [Test]
        public void LiteDbService_moves_property_value_from_root_to_child()
        {
            // ACT & ASSERT

            base.TreesorService_moves_property_value_from_root_to_child(7);

            // ASSERT
            // second item has the value, first one hasn't no values any more

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.IsFalse(items[0].ContainsKey(columns[0].Id.ToString()));
            Assert.IsFalse(items[0].ContainsKey(columns[1].Id.ToString()));
            Assert.AreEqual(7, items[1].Get(columns[1].Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_moves_values_between_properties_of_same_node()
        {
            // ACT & ASSERT

            base.TreesorService_moves_values_between_properties_of_same_node(6);

            // ASSERT
            // value is no in second columns

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var item = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.IsFalse(item.ContainsKey(columns[0].Id.ToString()));
            Assert.AreEqual(6, item.Get(columns[1].Id.ToString()).RawValue);
        }

        [Test]
        public void LiteDbService_moves_property_value_from_child_to_root()
        {
            // ACT

            base.TreesorService_moves_property_value_from_child_to_root(6);

            // ASSERT
            // value is in second column of first (root) item

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.IsFalse(items[0].ContainsKey(columns[0].Id.ToString()));
            Assert.IsFalse(items[0].ContainsKey(columns[1].Id.ToString()));
            Assert.AreEqual(6, items[1].Get(columns[0].Id.ToString()).RawValue);
            Assert.IsFalse(items[1].ContainsKey(columns[1].Id.ToString()));
        }

        [Test]
        public void LiteDbService_fails_on_MovePropertyValue_for_missing_destination_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_node(6);

            // ASSERT
            // value is still at source node

            Assert.AreEqual(2, this.database
               .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
               .FindAll()
               .Count());

            Assert.AreEqual(1, this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray()
                .Count());
        }

        [Test]
        public void LiteDbService_fails_on_MovePropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_node(6);

            // ASSERT
        }

        #endregion MovePropertyValue
    }
}