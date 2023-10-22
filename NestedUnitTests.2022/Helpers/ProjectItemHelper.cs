using EnvDTE;

namespace NestedUnitTests.Helpers
{
    public static class ProjectItemHelper
    {
        public static bool ContainsProperty(this ProjectItem projectItem, string propertyName)
        {
            if (projectItem.Properties != null)
            {
                foreach (Property item in projectItem.Properties)
                {
                    if (item != null && item.Name == propertyName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
