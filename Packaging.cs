﻿using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Annotations;
using System.Linq;

namespace Dnn.CakeUtils
{
    public static class Packaging
    {
        [CakeMethodAlias]
        public static void AddResourceZip(this ICakeContext context, Solution solution, Project project, string devPath, string packagePath)
        {
            var releaseFiles = project.pathsAndFiles.releaseFiles == null ? solution.dnn.pathsAndFiles.releaseFiles : project.pathsAndFiles.releaseFiles;
            if (releaseFiles.Length > 0)
            {
                var excludeFiles = project.pathsAndFiles.excludeFilter;
                if (excludeFiles == null)
                {
                    excludeFiles = solution.dnn.pathsAndFiles.excludeFilter;
                }
                else
                {
                    excludeFiles = excludeFiles.Concat(solution.dnn.pathsAndFiles.excludeFilter).ToArray();
                }
                var files = context.GetFilesByPatterns(devPath, releaseFiles, excludeFiles);
                if (files.Count > 0)
                {
                    var resZip = Compression.ZipToBytes(devPath, files);
                    context.Information("Zipped resources file");
                    Compression.AddBinaryFileToZip(packagePath, resZip, project.packageName.NoSlashes() + ".zip", true);
                }
                context.Information("Added resources from " + devPath);
            }
        }
        [CakeMethodAlias]
        public static void AddScripts(this ICakeContext context, Solution solution, Project project, string packagePath)
        {
            if (!string.IsNullOrEmpty(project.pathsAndFiles.pathToScripts))
            {
                var files = context.GetFiles(project.pathsAndFiles.pathToScripts + "/*.SqlDataProvider");
                Compression.AddFilesToZip(packagePath, project.pathsAndFiles.pathToScripts, solution.dnn.pathsAndFiles.packageScriptsFolder + "/" + project.packageName.NoSlashes(), files, true);
            }
        }
        [CakeMethodAlias]
        public static void AddCleanupFiles(this ICakeContext context, Solution solution, Project project, string packagePath)
        {
            if (!string.IsNullOrEmpty(project.pathsAndFiles.pathToCleanupFiles))
            {
                var files = context.GetFiles(project.pathsAndFiles.pathToCleanupFiles + "/*.txt");
                Compression.AddFilesToZip(packagePath, project.pathsAndFiles.pathToCleanupFiles, solution.dnn.pathsAndFiles.packageCleanupFolder + "/" + project.packageName.NoSlashes(), files, true);
            }
        }
        [CakeMethodAlias]
        public static void AddLicenseAndReleaseNotes(this ICakeContext context, Solution solution, Project project, string packagePath)
        {
            if (!string.IsNullOrEmpty(project.pathsAndFiles.licenseFile))
            {
                var license = Utilities.GetTextOrMdFile(project.pathsAndFiles.licenseFile);
                if (license != "")
                {
                    Compression.AddTextFileToZip(packagePath, license, project.packageName.NoSlashes() + "/License.txt", true);
                }
            }
            if (!string.IsNullOrEmpty(project.pathsAndFiles.releaseNotesFile))
            {
                var releaseNotes = Utilities.GetTextOrMdFile(project.pathsAndFiles.releaseNotesFile);
                if (releaseNotes != "")
                {
                    Compression.AddTextFileToZip(packagePath, releaseNotes, project.packageName.NoSlashes() + "/ReleaseNotes.txt", true);
                }
            }
        }
    }
}
