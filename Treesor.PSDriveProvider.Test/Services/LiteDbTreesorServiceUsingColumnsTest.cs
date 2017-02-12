using Elementary.Hierarchy.Collections;
using LiteDB;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class LiteDbTreesorServiceUsingColumnsTest
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

        //[TearDown]
        //public void CleanUpAllTests()
        //{
        //    this.database.Dispose();
        //}

        #region CreateColumn

        [Test]
        public void CreateColumn_type_string_in_memory_and_db()
        {
            // ACT

            var result = this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT
            // columns was created and can be found in the database

            Assert.IsNotNull(result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string), result.Type);
            Assert.AreEqual(result, this.treesorService.GetColumns().Single());

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
            Assert.AreEqual("p", persistentCollections.Single().Name);
            Assert.AreEqual(typeof(string).ToString(), persistentCollections.Single().TypeName);
        }

        [Test]
        public void CreateColumn_twice_is_accepted_and_not_stored_twice_in_db()
        {
            // ARRANGE

            var column = this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(column, result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string), result.Type);
            Assert.AreEqual(column, this.treesorService.GetColumns().Single());

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
        }

        [Test]
        public void CreateColumns_twice_fails_with_different_type()
        {
            // ARRANGE

            this.treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.CreateColumn(name: "p", type: typeof(int)));

            // ASSERT
            // excetion is throne and DB is not changed

            Assert.AreEqual($"Column: 'p' already defined with type: '{typeof(string)}'", result.Message);

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(1, persistentCollections.Count());
            Assert.AreEqual("p", persistentCollections.Single().Name);
            Assert.AreEqual(typeof(string).ToString(), persistentCollections.Single().TypeName);
        }

        [Test]
        public void CreateColumn_fails_on_null_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.CreateColumn(null, typeof(string)));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void CreateColumn_fails_on_empty_name()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.CreateColumn(null, typeof(string)));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        [Test]
        public void CreateColumn_fails_on_missing_type()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.CreateColumn("name", null));

            // ASSERT

            Assert.AreEqual("type", result.ParamName);
        }

        #endregion CreateColumn

        #region RemoveColumn

        [Test]
        public void RemoveColumn_unexisting_column_does_nothing()
        {
            // ACT

            var result = this.treesorService.RemoveColumn("p");

            // ASSERT

            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveColumn_succeeds_in_memory_and_db()
        {
            // ARRANGE

            this.treesorService.CreateColumn("p", typeof(string));

            // ACT

            var result = this.treesorService.RemoveColumn("p");

            // ASSERT
            // column is remved from memory and db

            Assert.IsTrue(result);
            Assert.IsFalse(this.treesorService.GetColumns().Any());

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection).FindAll();

            Assert.AreEqual(0, persistentCollections.Count());
        }

        [Test]
        public void RemoveColumns_fails_on_null_columnName()
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.RemoveColumn(null));

            Assert.AreEqual("columnName", result.ParamName);
        }

        [Test]
        public void RemoveColumns_fails_on_empty_columnName()
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => this.treesorService.RemoveColumn(""));

            Assert.AreEqual("columnName", result.ParamName);
        }

        #endregion RemoveColumn

        #region RenameColumn

        [Test]
        public void RenameColumn_throws_for_missing_column()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
        }

        [Test]
        public void RenameColumn_changes_the_column_name_in_db_too()
        {
            // ARRANGE

            this.treesorService.CreateColumn("p", typeof(string));

            // ACT

            Assert.DoesNotThrow(() => this.treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("q", this.treesorService.GetColumns().Single().Name);

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection);

            Assert.IsFalse(persistentCollections.Find(c => c.Name.Equals("p")).Any());

            var q = persistentCollections.Find(c => c.Name.Equals("q")).Single();

            Assert.AreEqual(typeof(string).ToString(), q.TypeName);
        }

        [Test]
        public void RenameColumn_fails_is_column_name_is_used_already()
        {
            // ARRANGE

            this.treesorService.CreateColumn("p", typeof(string));
            this.treesorService.CreateColumn("q", typeof(int));

            // ACT

            var result = Assert.Throws<ArgumentException>(() => this.treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("An item with the same key has already been added.", result.Message);

            var persistentCollections = this.database.GetCollection<LiteDbTreesorService.ColumnEntity>(LiteDbTreesorService.column_collection);

            var p = persistentCollections.Find(c => c.Name.Equals("p")).Single();

            Assert.AreEqual(typeof(string).ToString(), p.TypeName);

            var q = persistentCollections.Find(c => c.Name.Equals("q")).Single();

            Assert.AreEqual(typeof(int).ToString(), q.TypeName);
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