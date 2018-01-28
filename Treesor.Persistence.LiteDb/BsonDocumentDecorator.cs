using LiteDB;
using System.Collections.Generic;

namespace Treesor.Persistence.LiteDb
{
    public static class BsonDocumentDecorator
    {
        //public static IEnumerable<KeyValuePair<string, BsonValue>> Children(this BsonDocument thisDoc) => thisDoc.ChildrenDoc();

        #region Retrieva all children

        public static BsonDocument Children(this BsonDocument thisDoc)
        {
            if (thisDoc.TryGetValue("cn", out var bsonValue))
                return bsonValue.AsDocument;

            bsonValue = new BsonDocument();
            thisDoc.Add("cn", bsonValue);
            return bsonValue.AsDocument;
        }

        #endregion Retrieva all children

        #region Every Node has a lite db Id

        public static ObjectId Id(this BsonDocument thisDoc)

        {
            if (thisDoc.TryGetValue("_id", out var id))
                return id.AsObjectId;
            id = ObjectId.NewObjectId();
            thisDoc.Set("_id", id);
            return id;
        }

        #endregion Every Node has a lite db Id

        #region Every node and a string Key

        public static string Key(this BsonDocument thisDoc) => thisDoc["key"].AsString;

        public static BsonDocument Key(this BsonDocument thisDoc, string childKey)
        {
            thisDoc.Set("key", childKey);
            return thisDoc;
        }

        private static string KeyCI(this BsonDocument thisDoc) => CI(thisDoc["key"].AsString);

        #endregion Every node and a string Key

        #region Get or modify child structure

        public static bool TryAddChild(this BsonDocument thisDoc, BsonDocument child)
        {
            return thisDoc.TryAddChild(child.Key(), child.Id());
        }

        private static bool TryAddChild(this BsonDocument thisDoc, string childKey, ObjectId childId)
        {
            return thisDoc.Children().TryAdd(CI(childKey), childId);
        }

        public static (bool Exists, string Key, ObjectId Id) TryGetChild(this BsonDocument thisDoc, string childKey)
        {
            if (thisDoc.Children().TryGetValue(CI(childKey), out var childId))
                return (true, childKey, childId);
            else
                return (false, null, null);
        }

        public static bool TryRemoveChild(this BsonDocument thisDoc, string childKey)
        {
            return thisDoc.Children().Remove(CI(childKey));
        }

        private static string CI(string str)
        {
            return str.ToLowerInvariant();
        }

        public static bool TryRenameChild(this BsonDocument thisDoc, string childKey, string newChildKey)
        {
            var (exists, keyCI, id) = thisDoc.TryGetChild(childKey);
            return (thisDoc.TryRemoveChild(keyCI) && thisDoc.TryAddChild(newChildKey, id));
        }

        #endregion Get or modify child structure
    }
}