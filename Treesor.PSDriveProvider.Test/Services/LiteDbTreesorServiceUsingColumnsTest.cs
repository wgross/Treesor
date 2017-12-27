using LiteDB;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Treesor.Abstractions;
using Treesor.PSDriveProvider.Test.Services.Base;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class LiteDbTreesorServiceUsingColumnsTest : TreesorServiceUsesColumnsTestBase
    {
        private Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        private LiteDbTreesorService treesorService;
        private LiteDatabase database;
        private MemoryStream databaseStream;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.databaseStream = new MemoryStream();
            this.database = new LiteDatabase(this.databaseStream);
            this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).EnsureIndex(c => c.Name);

            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new LiteDbTreesorService(this.hierarchyMock.Object, this.database);
        }

        #region CreateColumn

        [Test]
        public void CreateColumn_type_string_in_memory_and_db()
        {
            // ACT

            base.CreateColumn_type_string(this.treesorService);

            // ASSERT
            // columns was created and can be found in the database

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
            Assert.AreEqual("p", persistentCollections.Single().Name);
            Assert.AreEqual(typeof(string).ToString(), persistentCollections.Single().TypeName);
        }

        [Test]
        public void CreateColumn_twice_is_accepted_and_not_stored_twice_in_db()
        {
            // ACT

            base.CreateColumn_twice_is_accepted(this.treesorService);

            // ASSERT

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
        }

        [Test]
        public void CreateColumns_twice_fails_with_different_type()
        {
            // ACT

            base.CreateColumns_twice_fails_with_different_type(this.treesorService);

            // ASSERT

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
            Assert.AreEqual("p", persistentCollections.Single().Name);
            Assert.AreEqual(typeof(string).ToString(), persistentCollections.Single().TypeName);
        }

        [Test]
        public void CreateColumn_fails_on_missing_name()
        {
            // ACT

            base.CreateColumn_fails_on_missing_name(this.treesorService);
        }

        [Test]
        public void CreateColumn_fails_on_missing_type()
        {
            // ACT

            base.CreateColumn_fails_on_missing_type(this.treesorService);
        }

        #endregion CreateColumn

        #region RemoveColumn

        [Test]
        public void RemoveColumn_unexisting_column_does_nothing()
        {
            // ACT

            base.RemoveColumn_unexisting_column_does_nothing(this.treesorService);
        }

        [Test]
        public void RemoveColumn_succeeds_in_memory_and_db()
        {
            // ACT

            base.RemoveColumn_succeeds(this.treesorService);

            // ASSERT
            // column is remved from memory and db

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(0, persistentCollections.Count());
        }

        [Test]
        public void RemoveColumns_fails_on_null_columnName()
        {
            // ACT

            base.RemoveColumns_fails_on_null_columnName(this.treesorService);
        }

        [Test]
        public void RemoveColumns_fails_on_empty_columnName()
        {
            // ACT

            base.RemoveColumns_fails_on_empty_columnName(this.treesorService);
        }

        #endregion RemoveColumn

        #region RenameColumn

        [Test]
        public void RenameColumn_throws_for_missing_column()
        {
            // ACT

            base.RenameColumn_throws_for_missing_column(this.treesorService);
        }

        [Test]
        public void RenameColumn_changes_the_column_name_in_db_too()
        {
            // ACT

            base.RenameColumn_changes_the_column_name(this.treesorService);

            // ASSERT

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection);

            Assert.IsFalse(persistentCollections.Find(c => c.Name.Equals("p")).Any());

            var q = persistentCollections.Find(c => c.Name.Equals("q")).Single();

            Assert.AreEqual(typeof(string).ToString(), q.TypeName);
        }

        [Test]
        public void RenameColumn_fails_is_column_name_is_used_already()
        {
            // ACT

            base.RenameColumn_fails_is_column_name_is_used_already(this.treesorService);

            // ASSERT

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection);

            var p = persistentCollections.Find(c => c.Name.Equals("p")).Single();

            Assert.AreEqual(typeof(string).ToString(), p.TypeName);

            var q = persistentCollections.Find(c => c.Name.Equals("q")).Single();

            Assert.AreEqual(typeof(string).ToString(), q.TypeName);
        }

        #endregion RenameColumn

        #region Restore Columns from DB

        [Test]
        public void TreesorService_reads_column_definitions_from_DB()
        {
            // ARRANGE
            // create columns at persietent storage

            this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = new LiteDbTreesorService(Mock.Of<IHierarchy<string, Reference<Guid>>>(), new LiteDatabase(this.databaseStream)).GetColumns();

            // ASSERT

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("p", result.Single().Name);
            Assert.AreEqual(typeof(string), result.Single().Type);
        }

        #endregion Restore Columns from DB
    }
}