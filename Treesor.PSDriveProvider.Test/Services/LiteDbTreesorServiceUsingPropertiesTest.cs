﻿using LiteDB;
using Moq;
using System;
using System.IO;
using System.Linq;
using Treesor.Abstractions;
using Treesor.Model;
using Treesor.PSDriveProvider.Test.Service.Base;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Services
{
    public class LiteDbTreesorServiceUsingPropertiesTest : TreesorServiceUsingPropertiesTestBase
    {
        private const string _id = nameof(_id);
        private readonly LiteDatabase database;
        private readonly MemoryStream databaseStream;

        public LiteDbTreesorServiceUsingPropertiesTest()
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

        [Fact]
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

            Assert.Equal(nodeId, nodeValues.Get("_id").AsGuid);
            Assert.Equal("value", nodeValues.Get(column.Id.ToString()).RawValue);
        }

        [Fact]
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

            Assert.Equal(2, columns.Count());
            Assert.Equal(nodeId, nodeValues.Get(_id).AsGuid);
            Assert.Equal((object)"value", nodeValues.Get(columns.Single(c => c.Name.Equals("p")).Id.ToString()).RawValue);
            Assert.Equal(2, nodeValues.Get(columns.Single(c => c.Name.Equals("q")).Id.ToString()).RawValue);
        }

        [Fact]
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

            Assert.Equal(nodeId, nodeValues.Get(_id).RawValue);
            Assert.Equal("new value", nodeValues.Get(column.Id.ToString()).RawValue);
        }

        [Fact]
        public void LiteDbService_fails_on_SetPropertyValue_with_wrong_type()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_wrong_type(nodeId, value: "value", wrongValue: 6);

            // ASSERT
            // verify storage structure: single columns single value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            Assert.Equal(typeof(string).ToString(), column.TypeName);

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(nodeId, bsonDocument.Get(_id).RawValue);
            Assert.Equal("value", bsonDocument.Get(column.Id.ToString()).RawValue);
        }

        [Fact]
        public void LiteDbService_fails_on_SetPropertyValue_with_missing_column()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_column(nodeId);

            // ASSERT
            // no values have been created

            Assert.False(this.database
                .GetCollection(LiteDbTreesorService.column_collection)
                .FindAll()
                .Any());

            Assert.False(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Fact]
        public void LiteDbService_fails_on_SetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_at_missing_node();

            // ASSERT
            // no values have been created

            Assert.False(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Fact]
        public void LiteDbService_fails_on_SetPropertyValue_on_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_property_name();

            // ASSERT
            // no values have been created

            Assert.False(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Fact]
        public void LiteDbService_fails_on_SetPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_SetPropertyValue_with_missing_node_path();

            // ASSERT
            // no values have been created

            Assert.False(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        #endregion SetPropertyValue

        #region GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        [Fact]
        public void LiteDbService_fails_on_GetPropertyValue_at_missing_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_at_missing_column();
        }

        [Fact]
        public void LiteDbService_fails_on_GetPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_for_missing_node();

            // ASSERT
            // no nodes have been created, one column exists

            Assert.Equal(1, this.database
               .GetCollection(LiteDbTreesorService.column_collection)
               .FindAll()
               .Count());

            Assert.False(this.database
               .GetCollection(LiteDbTreesorService.value_collection)
               .FindAll()
               .Any());
        }

        [Fact]
        public void LiteDbService_fails_on_GetPropertyValue_with_missing_property_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_GetPropertyValue_with_missing_property_name();
        }

        #endregion GetPropertyValue: only error cases. Get value was used during set tests sufficiantly

        #region ClearProperty

        [Fact]
        public void LiteDbService_clears_property_value()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSSERT

            base.TreesorService_clears_property_value(nodeId, value: 3);

            // ASSERT
            // verify storage structure: single columns, no value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(nodeId, bsonDocument.Get(_id).RawValue);
            Assert.False(bsonDocument.ContainsKey(column.Id.ToString()));
        }

        [Fact]
        public void LiteDbService_clears_second_property_value()
        {
            // ARRANGE

            var id = Guid.NewGuid();

            // ACT & ASSSERT

            base.TreesorService_clears_second_property_value(id, p_value: 5, q_value: "value");

            // ASSERT
            // verify storage structure: single columns, no value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll().ToArray();

            Assert.Equal(2, columns.Count());

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(id, bsonDocument.Get(_id).RawValue);
            Assert.Equal(5, bsonDocument.Get(columns.Single(c => c.Name.Equals("p")).Id.ToString()).AsInt32);
            Assert.False(bsonDocument.ContainsKey(columns.Single(c => c.Name.Equals("q")).Id.ToString()));
        }

        [Fact]
        public void LiteDbService_fails_on_ClearPropertyValue_for_missing_column()
        {
            // ACT & ARRANGE

            base.TreesorService_fails_on_ClearPropertyValue_for_missing_column();
        }

        [Fact]
        public void LiteDbService_fails_on_ClearPropertyValue_at_missing_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_at_missing_node();
        }

        [Fact]
        public void LiteDbService_fails_ClearPropertyValue_fails_for_missing_column_name()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_column_name();
        }

        [Fact]
        public void LiteDbService_fails_on_ClearPropertyValue_with_missing_node_path()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_ClearPropertyValue_with_missing_node_path();
        }

        #endregion ClearProperty

        #region CopyPropertyValue

        [Fact]
        public void LiteDbService_copies_property_value_from_root_to_child()
        {
            // ARRANGE

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT & ARRANGE

            base.TreesorService_copies_property_value_from_root_to_child(rootId, childId, root_p_value: "value");

            // ASSERT
            // both items have same value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, items.Count());
            Assert.Equal("value", items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .Get(columns.Single(c => c.Name.Equals("p")).Id.ToString())
                .RawValue);
            Assert.Equal("value", items
                .Single(i => i.Get(_id).RawValue.Equals(childId))
                .Get(columns.Single(c => c.Name.Equals("q")).Id.ToString())
                .RawValue);
        }

        [Fact]
        public void LiteDbService_copies_property_value_within_same_node()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_copies_property_value_within_same_node(nodeId, node_p_value: 5);

            // ASSERT

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(nodeId, bsonDocument.Get(_id).RawValue);

            Assert.Equal(5, bsonDocument
                .Get(columns.Single(c => c.Name.Equals("p")).Id.ToString())
                .RawValue);

            Assert.Equal(5, bsonDocument
                .Get(columns.Single(c => c.Name.Equals("q")).Id.ToString())
                .RawValue);
        }

        [Fact]
        public void LiteDbService_copies_property_value_from_child_to_root()
        {
            // ARRANGE

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_copies_property_value_from_child_to_root(rootId, childId, child_q_value: 7);

            // ASSERT

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, items.Count());

            Assert.Equal(7, items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .Get(columns.Single(c => c.Name.Equals("p")).Id.ToString())
                .RawValue);

            Assert.Equal(7, items
                .Single(i => i.Get(_id).RawValue.Equals(childId))
                .Get(columns.Single(c => c.Name.Equals("q")).Id.ToString())
                .RawValue);
        }

        [Fact]
        public void LiteDbService_fails_on_CopyPropertyValue_at_missing_destination_node()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_at_missing_destination_node(nodeId, p_value: 7);

            // ASSERT
            // an item wasn't created

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(nodeId, bsonDocument.Get(_id).RawValue);
        }

        [Fact]
        public void InMemoryService_fails_on_CopyPropertyValue_for_missing_destination_column()
        {
            // ACT

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_destination_column(rootId, childId, root_p_value: 7);

            // ASSERT
            // destination item wasn't created

            Assert.Equal(1, this.database
                .GetCollection(LiteDbTreesorService.column_collection)
                .FindAll()
                .Count());

            var bsonDocment = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(rootId, bsonDocment.Get(_id).RawValue);
            Assert.Equal(2, bsonDocment.Keys.Count());
        }

        [Fact]
        public void LiteDbService_fails_on_CopyPropertyValue_for_missing_source_node()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValues_for_missing_source_node();

            // ASSERT
            // two columns but neither destination nor source itenes exist.

            Assert.Equal(2, this.database
                .GetCollection(LiteDbTreesorService.column_collection)
                .FindAll()
                .Count());

            Assert.False(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Any());
        }

        [Fact]
        public void LiteDbService_CopyPropertyValue_fails_for_missing_source_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_CopyPropertyValue_for_missing_source_column();

            // ASSERT
            // not value items do exist

            Assert.False(this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Any());
        }

        [Fact]
        public void LiteDbService_CopyPropertyValue_fails_for_different_types()
        {
            // ARRANGE

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_CopyPropertyValue_for_different_types(rootId, childId, root_p_value: 7);

            // ASSERT
            // two columns, one items

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(7, bsonDocument
                .Get(columns.Single(c => c.Name.Equals("p")).Id.ToString())
                .RawValue);

            Assert.False(bsonDocument.ContainsKey(columns.Single(c => c.Name.Equals("q")).Id.ToString()));
        }

        #endregion CopyPropertyValue

        #region MovePropertyValue

        [Fact]
        public void LiteDbService_moves_property_value_from_root_to_child()
        {
            // ARRANGE

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_moves_property_value_from_root_to_child(rootId, childId, root_p_value: 7);

            // ASSERT
            // second item has the value, first one hasn't no values any more

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, items.Count());
            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .ContainsKey(columns.Single(c => c.Name.Equals("p")).Id.ToString()));
            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .ContainsKey(columns.Single(c => c.Name.Equals("q")).Id.ToString()));
            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(childId))
                .ContainsKey(columns.Single(c => c.Name.Equals("p")).Id.ToString()));
            Assert.Equal(7, items
                .Single(i => i.Get(_id).Equals(childId))
                .Get(columns.Single(c => c.Name.Equals("q")).Id.ToString()).RawValue);
        }

        [Fact]
        public void LiteDbService_moves_values_between_properties_of_same_node()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_moves_values_between_properties_of_same_node(nodeId, p_value: 6);

            // ASSERT
            // value is no in second columns

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var item = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.False(item.ContainsKey(columns.Single(c => c.Name.Equals("p")).Id.ToString()));
            Assert.Equal(6, item.Get(columns.Single(c => c.Name.Equals("q")).Id.ToString()).RawValue);
        }

        [Fact]
        public void LiteDbService_moves_property_value_from_child_to_root()
        {
            // ARRANGE

            var rootId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            // ACT

            base.TreesorService_moves_property_value_from_child_to_root(rootId, childId, child_q_value: 6);

            // ASSERT
            // value is in second column of first (root) item

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(2, columns.Count());

            var items = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .ToArray();

            Assert.Equal(6, items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .Get(columns.Single(c => c.Name.Equals("p")).Id.ToString()).RawValue);

            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(rootId))
                .ContainsKey(columns.Single(c => c.Name.Equals("q")).Id.ToString()));

            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(childId))
                .ContainsKey(columns.Single(c => c.Name.Equals("p")).Id.ToString()));

            Assert.False(items
                .Single(i => i.Get(_id).RawValue.Equals(childId))
                .ContainsKey(columns.Single(c => c.Name.Equals("q")).Id.ToString()));
        }

        [Fact]
        public void LiteDbService_fails_on_MovePropertyValue_for_missing_destination_node()
        {
            // ARRANGE

            var nodeId = Guid.NewGuid();

            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_node(nodeId, p_value: 6);

            // ASSERT
            // value is still at source node

            var columns = this.database
               .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
               .FindAll()
               .ToArray();

            Assert.Equal(2, columns.Count());

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.Equal(nodeId, bsonDocument.Get(_id).RawValue);
            Assert.Equal(6, bsonDocument.Get(columns.Single(c => c.Name.Equals("p")).Id.ToString()).RawValue);
        }

        [Fact]
        public void LiteDbService_fails_on_MovePropertyValue_for_missing_destination_column()
        {
            // ACT & ASSERT

            base.TreesorService_fails_on_MovePropertyValue_for_missing_destination_column();

            // ASSERT
        }

        #endregion MovePropertyValue
    }
}