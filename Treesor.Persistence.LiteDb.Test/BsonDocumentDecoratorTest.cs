using LiteDB;
using Xunit;

namespace Treesor.Persistence.LiteDb.Test
{
    public class LiteDbTreesorItemRepositoryNodeTest
    {
        [Fact]
        public void Node_has_empty_list_of_children()
        {
            // ARRANGE

            var node = new BsonDocument();

            // ACT
            // add a child node

            var result = node.Children();

            // ASSERT
            // node has no children

            Assert.Empty(result);
        }

        [Fact]
        public void Node_has_id()
        {
            // ARRANGE

            var node = new BsonDocument();

            // ACT
            // get id

            var result = node.Id();

            // ASSERT
            // node has no children

            Assert.NotNull(result);
            Assert.IsType<ObjectId>(result);
        }

        [Fact]
        public void Node_has_hierarchy_key()
        {
            // ARRANGE

            var node = new BsonDocument().Key("key");

            // ACT
            // get id

            var result = node.Key();

            // ASSERT
            // node has no children

            Assert.Equal("key", result);
        }

        [Fact]
        public void RootNode_hasnt_hierarchy_key()
        {
            // ARRANGE

            var node = new BsonDocument();

            // ACT
            // get id

            var result = node.Key();

            // ASSERT
            // node has no children

            Assert.Null(result);
        }

        [Fact]
        public void Node_receives_a_childnode_id()
        {
            // ARRANGE
            // prepare two nodes

            var parent = new BsonDocument();
            var child = new BsonDocument().Key("item");

            // ACT
            // add a child node

            var result = parent.TryAddChild(child);

            // ASSERT
            // parent contains the child

            Assert.True(result);

            var (hasChild, childNodeKey, childNodeId) = parent.TryGetChild("item");

            Assert.True(hasChild);
            Assert.Equal("item", childNodeKey);
            Assert.Equal(child.Id(), childNodeId);
        }

        [Fact]
        public void Node_fails_on_adding_duplicate_child()
        {
            // ARRANGE
            // prepare three nodes

            var parent = new BsonDocument();
            var child1 = new BsonDocument().Key("item");
            var child2 = new BsonDocument().Key("item");

            parent.TryAddChild(child1);

            // ACT
            // add a child node

            var result = parent.TryAddChild(child2);

            // ASSERT
            // parent rejects duplicate child

            Assert.False(result);
        }
    }
}