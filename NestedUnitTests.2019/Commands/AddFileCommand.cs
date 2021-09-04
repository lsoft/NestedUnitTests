using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NestedUnitTests
{
    [Command(PackageGuids.NestedUnitTestsString, PackageIds.AddFileCommandId)]
    internal sealed class AddFileCommand : BaseCommand<AddFileCommand>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            //TODO: check for this project is prepared for nested unit tests
            //TODO: check this file is c# source file

            base.BeforeQueryStatus(e);
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var file = await VS.Solutions.GetActiveItemAsync();

            if (file == null)
            {
                await VS.MessageBox.ShowErrorAsync($"Please choose physical file to append unit test file!");
                return;
            }
            if (file.Type != SolutionItemType.PhysicalFile)
            {
                await VS.MessageBox.ShowErrorAsync($"Unit tests file can be appended only to physical file!");
                return;
            }

            var fileFullPath = file.FullPath;

            var targetNamespace = await GetTargetNamespaceAsync(fileFullPath);
            if (string.IsNullOrEmpty(targetNamespace))
            {
                await VS.MessageBox.ShowErrorAsync($"Cannot determine target namespace!");
                return;
            }

            var fileInfo = new FileInfo(fileFullPath);

            GetTestFileData(
                fileInfo,
                out var testFileName,
                out var testClassName
                );

            var fileFolderPath = fileInfo.Directory.FullName;
            var testFilePath = Path.Combine(fileFolderPath, testFileName);
            
            var testFileBody = GetUnitTestFileBody(
                testClassName,
                targetNamespace
                );

            File.WriteAllText(
                testFilePath,
                testFileBody,
                System.Text.Encoding.UTF8
                );

            if (General.Instance.OpenNewFile)
            {
                //await VS.Documents.OpenViaProjectAsync(testFilePath);
                await VS.Documents.OpenAsync(testFilePath);
            }
            else
            {
                await VS.MessageBox.ShowAsync($"Unit tests file successfully added!");
            }
        }

        private void GetTestFileData(
            FileInfo fileInfo,
            out string testFileName,
            out string testClassName
            )
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            var fileNameWithoutExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            var fileFolderPath = fileInfo.Directory.FullName;

            var index = 0;
            while(true)//for (var index = 0; index < 10; index++)
            {
                //var sfindex = index == 0 ? string.Empty : $"{index}.";
                //var scindex = index == 0 ? string.Empty : $"{index}_";

                testFileName = $"{fileNameWithoutExtension}.{index}.{General.Instance.FileNameSuffix}.cs";
                testClassName = $"{fileNameWithoutExtension}_{index}_{General.Instance.FileNameSuffix}";

                var testFilePath = Path.Combine(fileFolderPath, testFileName);
                if (!File.Exists(testFilePath))
                {
                    return;
                }

                index++;
            }
        }

        private async Task<string> GetTargetNamespaceAsync(
            string fileFullPath
            )
        {
            if (fileFullPath is null)
            {
                throw new ArgumentNullException(nameof(fileFullPath));
            }

            var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(fileFullPath));
            var nds = (await syntax.GetRootAsync())
                .DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault()
                ;

            if (nds == null)
            {
                return string.Empty;
            }

            var targetNamespace = nds.Name.ToString();

            return targetNamespace;
        }

        private string GetUnitTestFileBody(
            string unitTestClassName,
            string targetNamespace
            )
        {
            if (unitTestClassName is null)
            {
                throw new ArgumentNullException(nameof(unitTestClassName));
            }

            if (targetNamespace is null)
            {
                throw new ArgumentNullException(nameof(targetNamespace));
            }

            return $@"#if !{General.Instance.ConditionalSymbol}

namespace {targetNamespace}
{{
    internal sealed class {unitTestClassName}
    {{

    }}
}}

#endif
";
        }

    }
}
