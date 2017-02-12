using NUnit.Framework;
using System;
using System.Linq;

namespace Treesor.PSDriveProvider.Test.Services.Base
{
    public abstract class TreesorServiceUsesColumnsTestBase
    {
        #region CreateColumn

        public void CreateColumn_type_string(ITreesorService treesorService)
        {
            // ACT

            var result = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("p", result.Name);
            Assert.AreEqual(typeof(string), result.Type);
            Assert.AreEqual(result, treesorService.GetColumns().Single());
        }

        public void CreateColumn_twice_is_accepted(ITreesorService treesorService)
        {
            // ARRANGE

            var column = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(column, result);
            Assert.AreEqual("p", result.Name);
            Assert.AreSame(typeof(string), result.Type);
            Assert.AreEqual(column, treesorService.GetColumns().Single());
        }

        public void CreateColumns_twice_fails_with_different_type(ITreesorService treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CreateColumn(name: "p", type: typeof(int)));

            // ASSERT

            Assert.AreEqual($"Column: 'p' already defined with type: '{typeof(string)}'", result.Message);
        }

        public void CreateColumn_fails_on_missing_name(ITreesorService treesorService)
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.CreateColumn(null, typeof(string)));

            // ASSERT

            Assert.AreEqual("name", result.ParamName);
        }

        public void CreateColumn_fails_on_missing_type(ITreesorService treesorService)
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.CreateColumn("name", null));

            // ASSERT

            Assert.AreEqual("type", result.ParamName);
        }

        #endregion CreateColumn

        #region RemoveColumn

        public void RemoveColumn_unexisting_column_does_nothing(ITreesorService treesorService)
        {
            // ACT

            var result = treesorService.RemoveColumn("p");

            // ASSERT

            Assert.IsFalse(result);
        }

        public void RemoveColumn_succeeds(ITreesorService treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));

            // ACT

            var result = treesorService.RemoveColumn("p");

            // ASSERT

            Assert.IsTrue(result);
            Assert.IsFalse(treesorService.GetColumns().Any());
        }

        public void RemoveColumns_fails_on_null_columnName(ITreesorService treesorService)
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.RemoveColumn(null));

            Assert.AreEqual("columnName", result.ParamName);
        }

        public void RemoveColumns_fails_on_empty_columnName(ITreesorService treesorService)
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.RemoveColumn(""));

            Assert.AreEqual("columnName", result.ParamName);
        }

        #endregion RemoveColumn

        #region RenameColumn

        public void RenameColumn_throws_for_missing_column(ITreesorService treesorService)
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("Property 'p' doesn't exist", result.Message);
        }

        public void RenameColumn_changes_the_column_name(ITreesorService treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));

            // ACT

            Assert.DoesNotThrow(() => treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("q", treesorService.GetColumns().Single().Name);
        }

        public void RenameColumn_fails_is_column_name_is_used_already(ITreesorService treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));
            treesorService.CreateColumn("q", typeof(string));

            // ACT

            var result = Assert.Throws<ArgumentException>(() => treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.AreEqual("An item with the same key has already been added.", result.Message);
        }

        #endregion RenameColumn
    }
}