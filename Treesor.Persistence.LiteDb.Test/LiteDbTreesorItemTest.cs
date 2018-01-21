//using LiteDB;
//using Xunit;

//namespace Treesor.Persistence.LiteDb.Test
//{
//    public class BsonDocumentDecoratorTest
//    {
//        private readonly BsonDocument bsonDocument;

//        public BsonDocumentDecoratorTest()
//        {
//            this.bsonDocument = new BsonDocument();
//        }

//        [Fact]
//        public void BsonDocument_has_no_childnodes()
//        {
//            // ACT

//            var result = this.bsonDocument.ChildrenIds();

//            // ASSERT
//            // bey default empüty list of child nodes

//            Assert.Empty(result);
//        }

//        [Fact]
//        public void BsonDocument_adds_new_childre()
//        {
//            // ACT

//            this.bsonDocument.Child("b", ObjectId.NewObjectId());

//            // ASSERT
//            // list of chldren contains now 1 item

//            var children = this.bsonDocument.ChildrenIds();

//            Assert.Single(this.bsonDocument);
//        }
//    }
//}