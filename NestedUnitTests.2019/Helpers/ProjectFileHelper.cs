using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NestedUnitTests.Helpers
{
    public static class ProjectFileHelper
    {
        public static bool IsSdkStyle(
            this string projectFilePath
            )
        {
            if (projectFilePath is null)
            {
                throw new ArgumentNullException(nameof(projectFilePath));
            }
            if (!File.Exists(projectFilePath))
            {
                throw new FileNotFoundException(projectFilePath);
            }

            var projectBody = File.ReadAllText(projectFilePath);

            return projectBody.Contains("Sdk=\"Microsoft.NET.Sdk\"");
        }
    }
}
