using LiteDB;
using System.Collections.Generic;

namespace Treesor.Persistence.LiteDb
{
    public static class BsonDocumentDecorator
    {
        //public static IEnumerable<KeyValuePair<string, BsonValue>> Children(this BsonDocument thisDoc) => thisDoc.ChildrenDoc();

        public static BsonDocument Children(this BsonDocument thisDoc)
        {
            if (thisDoc.TryGetValue("cn", out var bsonValue))
                return bsonValue.AsDocument;

            bsonValue = new BsonDocument();
            thisDoc.Add("cn", bsonValue);
            return bsonValue.AsDocument;
        }

        public static ObjectId Id(this BsonDocument thisDoc)
        {
            if (thisDoc.TryGetValue("_id", out var id))
                return id.AsObjectId;
            id = ObjectId.NewObjectId();
            thisDoc.Set("_id", id);
            return id;
        }

        public static string Key(this BsonDocument thisDoc) => thisDoc["key"].AsString;

        public static BsonDocument Key(this BsonDocument thisDoc, string childKey)
        {
            thisDoc.Set("key", childKey);
            return thisDoc;
        }

        public static bool TryAddChild(this BsonDocument thisDoc, BsonDocument child)
        {
            return thisDoc.Children().TryAdd(child.Key(), child.Id());
        }

        public static (bool Exists, string Key, ObjectId Id) TryGetChild(this BsonDocument thisDoc, string childKey)
        {
            if (thisDoc.Children().TryGetValue(childKey, out var childId))
                return (true, childKey, childId);
            else
                return (false, null, null);
        }
    }
}