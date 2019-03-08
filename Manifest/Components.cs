﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Dnn.CakeUtils.Manifest
{
    public static class Components
    {
        public static void AddScripts(this XmlNode parent, Project project, string version, string packageScriptsFolder)
        {
            if (string.IsNullOrEmpty(project.pathsAndFiles.pathToScripts))
            {
                return;
            }
            var folderName = packageScriptsFolder.EnsureEndsWith("/");
            folderName += project.packageName + "/";
            var scriptsFound = false;
            var newNode = parent.AddChildElement("component").AddAttribute("type", "Script");
            var scripts = newNode.AddChildElement("scripts");
            scripts.AddChildElement("basePath", "DesktopModules/" + project.folder + "/SqlScripts");
            var d = new System.IO.DirectoryInfo(project.pathsAndFiles.pathToScripts);
            Console.WriteLine("Looking for scripts in {0}", d.FullName);
            if (!d.Exists)
            {
                return;
            }

            foreach (var f in d.GetFiles("*.SqlDataProvider"))
            {
                Console.WriteLine("Adding {0}", f.Name);
                var m = Regex.Match(f.Name, @"(?i)^(\d+)\.(\d+)\.(\d+)\.SqlDataProvider(?-i)");
                if (m.Success)
                {
                    var script = scripts.AddChildElement("script").AddAttribute("type", "Install");
                    script.AddChildElement("name", f.Name);
                    script.AddChildElement("sourceFileName", folderName + f.Name);
                    script.AddChildElement("version", System.IO.Path.GetFileNameWithoutExtension(f.Name));
                    scriptsFound = true;
                }
                else
                {
                    m = Regex.Match(f.Name, @"(?i)Install\.(\d+)\.(\d+)\.(\d+)\.SqlDataProvider(?-i)");
                    if (m.Success)
                    {
                        var script = scripts.AddChildElement("script").AddAttribute("type", "Install");
                        script.AddChildElement("name", f.Name);
                        script.AddChildElement("sourceFileName", folderName + f.Name);
                        script.AddChildElement("version", string.Format("{0}.{1}.{2}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value));
                        scriptsFound = true;
                    }
                    else
                    {
                        m = Regex.Match(f.Name, @"(?i)(\w+)\.SqlDataProvider(?-i)");
                        if (m.Success)
                        {
                            switch (m.Groups[1].Value.ToLower())
                            {
                                case "uninstall":
                                    var script = scripts.AddChildElement("script").AddAttribute("type", "UnInstall");
                                    script.AddChildElement("name", f.Name);
                                    script.AddChildElement("sourceFileName", folderName + f.Name);
                                    script.AddChildElement("version", version.ToNormalizedVersion());
                                    scriptsFound = true;
                                    break;
                            }
                        }
                    }
                }

            }
            if (scriptsFound)
            {
                parent.AppendChild(newNode);
            }
        }

        public static void AddAssemblies(this XmlNode parent, Project project, string pathToAssemblies, string packageAssembliesFolder)
        {
            var folderName = packageAssembliesFolder.EnsureEndsWith("/");
            var newNode = parent.AddChildElement("component").AddAttribute("type", "Assembly");
            var assemblies = newNode.AddChildElement("assemblies");
            assemblies.AddChildElement("basePath", "bin");
            var d = new System.IO.DirectoryInfo(pathToAssemblies);
            Console.WriteLine("Looking for assemblies in {0}", d.FullName);
            if (!d.Exists)
            {
                return;
            }
            var filesToPack = new List<System.IO.FileInfo>();
            if (project.pathsAndFiles.assemblies != null)
            {
                foreach (var a in project.pathsAndFiles.assemblies)
                {
                    var fileName = System.IO.Path.Combine(d.FullName, a);
                    if (System.IO.File.Exists(fileName))
                    {
                        filesToPack.Add(new System.IO.FileInfo(fileName));
                    }
                }
            }
            else
            {
                filesToPack = d.GetFiles("*.dll").ToList();
            }
            Console.WriteLine("Found {0} assemblies", filesToPack.Count);
            foreach (var f in filesToPack)
            {
                var version = FileVersionInfo.GetVersionInfo(f.FullName).FileVersion;
                var a = assemblies.AddChildElement("assembly");
                a.AddChildElement("name", f.Name);
                a.AddChildElement("sourceFileName", folderName + f.Name);
                a.AddChildElement("version", version);
            }
        }

        public static void AddCleanupFiles(this XmlNode parent, Project project)
        {
            if (string.IsNullOrEmpty(project.pathsAndFiles.pathToCleanupFiles))
            {
                return;
            }
            var d = new System.IO.DirectoryInfo(project.pathsAndFiles.pathToCleanupFiles);
            if (!d.Exists)
            {
                return;
            }
            foreach (var f in d.GetFiles("*.txt"))
            {
                var m = Regex.Match(f.Name, @"(?i)(\d+)\.(\d+)\.(\d+)\.txt(?-i)");
                if (m.Success)
                {
                    parent.AddChildElement("component")
                        .AddAttribute("type", "CleanUp")
                        .AddAttribute("version", System.IO.Path.GetFileNameWithoutExtension(f.Name))
                        .AddAttribute("fileName", f.Name);
                }
            }
        }

        public static void AddResourceComponent(this XmlNode parent, Project project)
        {
            var rf = parent.AddChildElement("component").AddAttribute("type", "ResourceFile").AddChildElement("resourceFiles");
            rf.AddChildElement("basePath", "DesktopModules/" + project.folder);
            rf.AddChildElement("resourceFile").AddChildElement("name", project.packageName + ".zip");
        }
    }
}