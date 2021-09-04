using Community.VisualStudio.Toolkit;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Task = System.Threading.Tasks.Task;

namespace NestedUnitTests
{
    [Command(PackageGuids.NestedUnitTestsString, PackageIds.PrepareProjectCommandId)]
    internal sealed class PrepareProjectCommand : BaseCommand<PrepareProjectCommand>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            //TODO: check for project is sdk style
            //TODO: check for this modification already done

            base.BeforeQueryStatus(e);
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var project = await VS.Solutions.GetActiveProjectAsync();

            if (project == null)
            {
                return;
            }

            var projectFullPath = project.FullPath;

            SurgeCsproj(projectFullPath);
        }

        private void SurgeCsproj(
            string projectFilePath
            )
        {
            if (projectFilePath is null)
            {
                throw new ArgumentNullException(nameof(projectFilePath));
            }

            var csproj = XDocument.Load(
                projectFilePath,
                LoadOptions.PreserveWhitespace
                );

            var pg = csproj.XPathSelectElement("*/PropertyGroup");

            var pgIntend = pg.ToString()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .FirstOrDefault(l => l.Contains("/PropertyGroup"))
                ?.IndexOf('<') ?? 0
                ;

            var suffix =
                string.IsNullOrEmpty(General.Instance.FileNameSuffix)
                    ? "Fixture"
                    : General.Instance.FileNameSuffix;

            var itemGroupNode = new XElement(
                "ItemGroup",
                null
                );

            itemGroupNode.Add(Environment.NewLine);

            var compileNode = new XElement(
                "Compile",
                new XAttribute(
                    "Update",
                    @$"**\*.*.{suffix}.cs"
                    ),
                Environment.NewLine + new string(' ', pgIntend * 3),
                new XElement(
                    "DependentUpon",
                    $@"$([System.Text.RegularExpressions.Regex]::Replace(%(Filename), '(\.\d+\.{suffix})', '.cs'))"
                    ),
                Environment.NewLine + new string(' ', pgIntend * 2)
                );

            itemGroupNode.Add(new string(' ', pgIntend * 2));
            itemGroupNode.Add(compileNode);
            itemGroupNode.Add(Environment.NewLine);

            itemGroupNode.Add(new string(' ', pgIntend));

            csproj.Root.Add(Environment.NewLine + new string(' ', pgIntend));

            csproj.Root.Add(
                itemGroupNode
                );

            csproj.Root.Add(new string(' ', pgIntend));
            csproj.Root.Add(Environment.NewLine);
            csproj.Root.Add(Environment.NewLine);

            using (var sw = new StreamWriter(projectFilePath, false))
            {
                csproj.Save(
                    sw,
                    SaveOptions.None
                    );
            }

        }

    }
}
