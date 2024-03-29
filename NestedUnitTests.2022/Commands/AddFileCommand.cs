﻿using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using NestedUnitTests.Helpers;
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
            #region creating an unit tests file
            var solutionItem = await VS.Solutions.GetActiveItemAsync();

            if (solutionItem == null)
            {
                await VS.MessageBox.ShowErrorAsync($"Please choose physical file to append unit test file!");
                return;
            }
            if (solutionItem.Type != SolutionItemType.PhysicalFile)
            {
                await VS.MessageBox.ShowErrorAsync($"Unit tests file can be appended only to physical file!");
                return;
            }

            var fileFullPath = solutionItem.FullPath;

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

            #endregion

            var project = await VS.Solutions.GetActiveProjectAsync();
            var projectFilePath = project.FullPath;

            if (projectFilePath.IsSdkStyle())
            {
                await ProcessSdkStyleProjectAsync(project, fileFullPath, testFilePath);
            }
            else
            {
                await ProcessLegacyStyleProjectAsync(project, (PhysicalFile)solutionItem, testFilePath);
            }
        }

        private async Task ProcessLegacyStyleProjectAsync(
            Community.VisualStudio.Toolkit.Project project,
            PhysicalFile rootPhysicalFile,
            string testFilePath
            )
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (rootPhysicalFile is null)
            {
                throw new ArgumentNullException(nameof(rootPhysicalFile));
            }

            if (testFilePath is null)
            {
                throw new ArgumentNullException(nameof(testFilePath));
            }

            //community sdk approach
            //var testPhysicalFiles = await project.AddExistingFilesAsync(testFilePath);
            //var testPhysicalFile = testPhysicalFiles.First();
            //await rootPhysicalFile.AddNestedFileAsync(testPhysicalFile);

            //classic approach
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte == null)
            {
                return;
            }

            var documentItem = dte.Solution.FindProjectItem(rootPhysicalFile.FullPath);
            if (documentItem == null)
            {
                return;
            }

            documentItem.ProjectItems.AddFromFile(testFilePath);

            if (!General.Instance.OpenNewFile)
            {
                await VS.MessageBox.ShowAsync($"Unit tests file successfully added!");
                return;
            }

            await VS.Documents.OpenAsync(testFilePath);
        }

        private async Task ProcessSdkStyleProjectAsync(
            Community.VisualStudio.Toolkit.Project project,
            string fileFullPath,
            string testFilePath
            )
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileFullPath is null)
            {
                throw new ArgumentNullException(nameof(fileFullPath));
            }

            if (testFilePath is null)
            {
                throw new ArgumentNullException(nameof(testFilePath));
            }


            if (!General.Instance.OpenNewFile)
            {
                await VS.MessageBox.ShowAsync($"Unit tests file successfully added!");
                return;
            }

            //await VS.Documents.OpenViaProjectAsync(testFilePath);
            await VS.Documents.OpenAsync(testFilePath);
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
            while(true)
            {
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
