//using LiteDB;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Treesor.Model;

//namespace Treesor.Persistence.LiteDb
//{
//    public static class BsonDocumentDecorator
//    {
//        private static BsonDocument ChildrenIdDocument(this BsonDocument doc)
//        {
//            if (doc.TryGetValue("cn", out var childDoc))
//            {
//                return childDoc.AsDocument;
//            }
//            else
//            {
//                var tmp = new BsonDocument();
//                doc["cn"] = tmp;
//                return tmp;
//            }

//        }

//        public static IEnumerable<KeyValuePair<string, BsonValue>> ChildrenIds(this BsonDocument doc)
//        {
//            return doc.ChildrenIdDocument();
//        }

//        public static void Child(this BsonDocument doc, string key, ObjectId value)
//        {
//            doc.ChildrenIdDocument()[key] = value;
//        }
//    }
//}
