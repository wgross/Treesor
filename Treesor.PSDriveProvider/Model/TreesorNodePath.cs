namespace Treesor.Model
{
    using Elementary.Hierarchy;
    using System.Linq;

    public sealed class TreesorNodePath
    {
        public static readonly TreesorNodePath RootPath = new TreesorNodePath(itemPath: Elementary.Hierarchy.HierarchyPath.Create<string>());

        public static TreesorNodePath Parse(string drivePath)
        {
            TreesorNodePath result;
            TryParse(drivePath, out result);
            return result;
        }

        public static bool TryParse(string drivePath, out TreesorNodePath parsedPath)
        {
            if (string.IsNullOrEmpty(drivePath))
                parsedPath = RootPath;

            parsedPath = new TreesorNodePath(Elementary.Hierarchy.HierarchyPath.Parse(drivePath, @"\/"));
            return true; // currently no error cases are implemented
        }

        public static TreesorNodePath Create(HierarchyPath<string> treeKey)
        {
            return new TreesorNodePath(treeKey);
        }

        public static TreesorNodePath Create(params string[] pathItems)
        {
            if (pathItems.Count() == 1 && string.IsNullOrEmpty(pathItems.First()))
                return RootPath;

            return new TreesorNodePath(Elementary.Hierarchy.HierarchyPath.Create(pathItems));
        }

        #region Construction and initialization of this instance

        private TreesorNodePath(HierarchyPath<string> itemPath)
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
            TreesorNodePath otherAsNodePath = other as TreesorNodePath;

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