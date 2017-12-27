using Moq;
using NUnit.Framework;
using System;
using Treesor.Abstractions;
using Treesor.PSDriveProvider.Test.Services.Base;

namespace Treesor.PSDriveProvider.Test.Service
{
    [TestFixture]
    public class InMemoryTreesorServiceUsingColumnsTest : TreesorServiceUsesColumnsTestBase
    {
        private Mock<IHierarchy<string, Reference<Guid>>> hierarchyMock;
        private InMemoryTreesorService treesorService;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchyMock = new Mock<IHierarchy<string, Reference<Guid>>>();
            this.treesorService = new InMemoryTreesorService(this.hierarchyMock.Object);
        }

        #region CreateColumn

        [Test]
        public void CreateColumn_type_string()
        {
            base.CreateColumn_type_string(this.treesorService);
        }

        [Test]
        public void CreateColumn_twice_is_accepted()
        {
            base.CreateColumn_twice_is_accepted(this.treesorService);
        }

        [Test]
        public void CreateColumns_twice_fails_with_different_type()
        {
            base.CreateColumns_twice_fails_with_different_type(this.treesorService);
        }

        [Test]
        public void CreateColumn_fails_on_missing_name()
        {
            base.CreateColumn_fails_on_missing_name(this.treesorService);
        }

        [Test]
        public void CreateColumn_fails_on_missing_type()
        {
            base.CreateColumn_fails_on_missing_type(this.treesorService);
        }

        #endregion CreateColumn

        #region RemoveColumn

        [Test]
        public void RemoveColumn_unexisting_column_does_nothing()
        {
            base.RemoveColumn_unexisting_column_does_nothing(this.treesorService);
        }

        [Test]
        public void RemoveColumn_succeeds()
        {
            base.RemoveColumn_succeeds(this.treesorService);
        }

        [Test]
        public void RemoveColumns_fails_on_null_columnName()
        {
            base.RemoveColumns_fails_on_null_columnName(this.treesorService);
        }

        [Test]
        public void RemoveColumns_fails_on_empty_columnName()
        {
            base.RemoveColumns_fails_on_empty_columnName(this.treesorService);
        }

        #endregion RemoveColumn

        #region RenameColumn

        [Test]
        public void RenameColumn_throws_for_missing_column()
        {
            base.RenameColumn_throws_for_missing_column(this.treesorService);
        }

        [Test]
        public void RenameColumn_changes_the_column_name()
        {
            base.RenameColumn_changes_the_column_name(this.treesorService);
        }

        [Test]
        public void RenameColumn_fails_is_column_name_is_used_already()
        {
            base.RenameColumn_fails_is_column_name_is_used_already(this.treesorService);
        }

        #endregion RenameColumn
    }
}