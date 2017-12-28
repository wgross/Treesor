namespace Treesor.Model
{
    using Elementary.Hierarchy;
    using System.Linq;

    public sealed class TreesorItemPath
    {
        public static readonly TreesorItemPath RootPath = new TreesorItemPath(itemPath: Elementary.Hierarchy.HierarchyPath.Create<string>());

        public static TreesorItemPath ParsePath(string drivePath)
        {
            TreesorItemPath result;
            TryParse(drivePath, out result);
            return result;
        }

        public static bool TryParse(string drivePath, out TreesorItemPath parsedPath)
        {
            if (string.IsNullOrEmpty(drivePath))
                parsedPath = RootPath;

            parsedPath = new TreesorItemPath(Elementary.Hierarchy.HierarchyPath.Parse(drivePath, @"\/"));
            return true; // currently no error cases are implemented
        }

        public static TreesorItemPath Create(HierarchyPath<string> treeKey)
        {
            return new TreesorItemPath(treeKey);
        }

        public static TreesorItemPath CreatePath(params string[] pathItems)
        {
            if (pathItems.Count() == 1 && string.IsNullOrEmpty(pathItems.First()))
                return RootPath;

            return new TreesorItemPath(Elementary.Hierarchy.HierarchyPath.Create(pathItems));
        }

        #region Construction and initialization of this instance

        private TreesorItemPath(HierarchyPath<string> itemPath)
        {
            this.itemPath = itemPath;
        }

        public HierarchyPath<string> HierarchyPath
        {
            get
            {
                return this.itemPath;
            }
        }

        private readonly HierarchyPath<string> itemPath;

        #endregion Construction and initialization of this instance

        public bool IsDrive
        {
            get
            {
                return !(this.itemPath.Items.Any());
            }
        }

        #region Override object behaviour

        public override bool Equals(object other)
        {
            TreesorItemPath otherAsNodePath = other as TreesorItemPath;

            if (otherAsNodePath == null)
                return false; // wrong type

            if (object.ReferenceEquals(this, other))
                return true; // instances are same

            return this.HierarchyPath.Equals(otherAsNodePath.HierarchyPath);
        }

        public override int GetHashCode()
        {
            return this.HierarchyPath.GetHashCode();
        }

        public override string ToString()
        {
            return this.HierarchyPath.ToString(@"\");
        }

        #endregion Override object behaviour
    }
}