using System;
using System.Linq;
using Treesor.Model;
using Xunit;

namespace Treesor.PSDriveProvider.Test.Services.Base
{
    public abstract class TreesorServiceUsesColumnsTestBase
    {
        #region CreateColumn

        public void CreateColumn_type_string(ITreesorModel treesorService)
        {
            // ACT

            var result = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal("p", result.Name);
            Assert.Equal(typeof(string), result.Type);
            Assert.Equal(result, treesorService.GetColumns().Single());
        }

        public void CreateColumn_twice_is_accepted(ITreesorModel treesorService)
        {
            // ARRANGE

            var column = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = treesorService.CreateColumn(name: "p", type: typeof(string));

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(column, result);
            Assert.Equal("p", result.Name);
            Assert.Same(typeof(string), result.Type);
            Assert.Equal(column, treesorService.GetColumns().Single());
        }

        public void CreateColumns_twice_fails_with_different_type(ITreesorModel treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn(name: "p", type: typeof(string));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.CreateColumn(name: "p", type: typeof(int)));

            // ASSERT

            Assert.Equal($"Column: 'p' already defined with type: '{typeof(string)}'", result.Message);
        }

        public void CreateColumn_fails_on_missing_name(ITreesorModel treesorService)
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.CreateColumn(null, typeof(string)));

            // ASSERT

            Assert.Equal("name", result.ParamName);
        }

        public void CreateColumn_fails_on_missing_type(ITreesorModel treesorService)
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.CreateColumn("name", null));

            // ASSERT

            Assert.Equal("type", result.ParamName);
        }

        #endregion CreateColumn

        #region RemoveColumn

        public void RemoveColumn_unexisting_column_does_nothing(ITreesorModel treesorService)
        {
            // ACT

            var result = treesorService.RemoveColumn("p");

            // ASSERT

            Assert.False(result);
        }

        public void RemoveColumn_succeeds(ITreesorModel treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));

            // ACT

            var result = treesorService.RemoveColumn("p");

            // ASSERT

            Assert.True(result);
            Assert.False(treesorService.GetColumns().Any());
        }

        public void RemoveColumns_fails_on_null_columnName(ITreesorModel treesorService)
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.RemoveColumn(null));

            Assert.Equal("columnName", result.ParamName);
        }

        public void RemoveColumns_fails_on_empty_columnName(ITreesorModel treesorService)
        {
            // ACT & ASSERT

            var result = Assert.Throws<ArgumentNullException>(() => treesorService.RemoveColumn(""));

            Assert.Equal("columnName", result.ParamName);
        }

        #endregion RemoveColumn

        #region RenameColumn

        public void RenameColumn_throws_for_missing_column(ITreesorModel treesorService)
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.Equal("Property 'p' doesn't exist", result.Message);
        }

        public void RenameColumn_changes_the_column_name(ITreesorModel treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));

            // ACT

            treesorService.RenameColumn("p", "q");

            // ASSERT

            Assert.Equal("q", treesorService.GetColumns().Single().Name);
        }

        public void RenameColumn_fails_is_column_name_is_used_already(ITreesorModel treesorService)
        {
            // ARRANGE

            treesorService.CreateColumn("p", typeof(string));
            treesorService.CreateColumn("q", typeof(string));

            // ACT

            var result = Assert.Throws<ArgumentException>(() => treesorService.RenameColumn("p", "q"));

            // ASSERT

            Assert.Equal("An item with the same key has already been added.", result.Message);
        }

        #endregion RenameColumn
    }
}