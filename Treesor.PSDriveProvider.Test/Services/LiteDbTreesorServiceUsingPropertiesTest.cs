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

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT

            base.TreesorService_sets_property_value(id, "value");

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
        public void LiteDbService_adds_second_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT

            base.TreesorService_adds_second_property_value(id, "value", 2);

            // ASSERT
            // verify storage structure: single columns single value

            var columns = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Take(2)
                .ToArray();

            var bsonDocument = this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single();

            Assert.AreEqual((object)"value", bsonDocument
                .Get(columns[0].Id.ToString())
                .RawValue);

            Assert.AreEqual((object)2, bsonDocument
                .Get(columns[1].Id.ToString())
                .RawValue);
        }

        [Test]
        public void LiteDbService_changes_property_value()
        {
            // ARRANGE

            var id = new Reference<Guid>(Guid.NewGuid());

            // ACT & ASSERT

            base.TreesorService_changes_property_value(id, "value", "new value");

            // ASSERT
            // verify storage structure: single columns single value

            var column = this.database
                .GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection)
                .FindAll()
                .Single();

            Assert.AreEqual((object)"new value", this.database
                .GetCollection(LiteDbTreesorService.value_collection)
                .FindAll()
                .Single()
                .Get(column.Id.ToString())
                .RawValue);
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
    }
}