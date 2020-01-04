#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core;
using System.Linq;
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class PythonZip :
        Installer.ZipModule
    {
        protected override void
        Init()
        {
            var basename = Version.WindowsOutputName; // pythonMN.zip - applicable to all platforms
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    basename = Version.WindowsDebugOutputName; // pythonMN_d.zip
                }
            }
            this.Macros.AddVerbatim("zipoutputbasename", basename);

            var pylib = Bam.Core.Graph.Instance.FindReferencedModule<Python.PythonLibrary>();
            this.DependsOn(pylib);
            this.Macros.Add("pathtozip", pylib.LibraryDirectory);

            base.Init();

            this.PrivatePatch(settings =>
                {
                    var zipSettings = settings as Installer.IZipSettings;
                    zipSettings.RecursivePaths = true;
                    zipSettings.Update = true;
                }
            );
        }

        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                // since there is a dependency on PythonLibrary which does not need to be passed
                // through to Zip
                return System.Linq.Enumerable.Empty<(Bam.Core.Module, string)>();
            }
        }
    }
}
namespace Python.StandardDistribution
{
    static class PublisherExtensions
    {
        public static void
        RegisterPythonModuleTypesToCollate(
            this Publisher.Collation collator)
        {
            collator.Mapping.Register(
                typeof(SysConfigDataPythonFile),
                SysConfigDataPythonFile.SysConfigDataPythonFileKey,
                collator.CreateTokenizedString(
                    "$(0)/lib/python" + Version.MajorDotMinor,
                    new[] { collator.ExecutableDir }
                ),
                true);

            var zipCollationPath = "$(0)";
            if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                // next to executable
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                if (Publisher.Collation.EPublishingType.WindowedApplication == collator.PublishingType)
                {
                    zipCollationPath += "/../lib";
                }
                else
                {
                    zipCollationPath += "/lib";
                }
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                zipCollationPath += "/lib";
            }
            collator.Mapping.Register(
                typeof(Python.PythonZip),
                Installer.ZipModule.ZipKey,
                collator.CreateTokenizedString(
                    zipCollationPath,
                    new[] { collator.ExecutableDir }
                ),
                true
            );

            // required by distutils
            collator.Mapping.Register(
                typeof(PyConfigHeader),
                PyConfigHeader.HeaderFileKey,
                collator.CreateTokenizedString(
                    "$(0)/include/python" + Version.MajorDotMinor,
                    new[] { collator.ExecutableDir }
                ),
                true);
            collator.Mapping.Register(
                typeof(PyMakeFile),
                PyConfigHeader.HeaderFileKey,
                collator.CreateTokenizedString(
                    "$(0)/lib/python" + Version.MajorDotMinor + "/config-" + Version.MajorDotMinor,
                    new[] { collator.ExecutableDir }
                ),
                true);
        }

        public static string
        PythonBinaryModuleSubdirectory(
            this Publisher.Collation collator)
        {
            if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                return "DLLs";
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                if (Publisher.Collation.EPublishingType.WindowedApplication == collator.PublishingType)
                {
                    return System.String.Format("python{0}/lib-dynload", Version.MajorDotMinor);
                }
                else
                {
                    return System.String.Format("lib/python{0}/lib-dynload", Version.MajorDotMinor);
                }
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                return System.String.Format("lib/python{0}/lib-dynload", Version.MajorDotMinor);
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported platform");
            }
        }

        public static void
        SetPublishingDirectoryForPythonBinaryModule(
            this Publisher.Collation collator,
            Publisher.ICollatedObject module)
        {
            if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                (module as Publisher.CollatedObject).SetPublishingDirectory(
                    "$(0)/" + collator.PythonBinaryModuleSubdirectory(),
                    new[] { collator.ExecutableDir });
            }
            else
            {
                (module as Publisher.CollatedObject).SetPublishingDirectory(
                    "$(0)/" + collator.PythonBinaryModuleSubdirectory(),
                    new[] { collator.DynamicLibraryDir });
            }
        }

        public static void
        IncludePythonPlatformDependentModules(
            this Publisher.Collation collator,
            Publisher.ICollatedObject anchor,
            Bam.Core.Array<Publisher.ICollatedObject> platIndependentModules = null)
        {
            anchor.SourceModule.PrivatePatch(settings =>
                {
                    var linuxLinker = settings as C.ICommonLinkerSettingsLinux;
                    if (null != linuxLinker)
                    {
                        linuxLinker.CanUseOrigin = true;
                    }
                    if (Publisher.Collation.EPublishingType.WindowedApplication == collator.PublishingType)
                    {
                        linuxLinker?.RPath.AddUnique("$ORIGIN/../lib");
                        if (settings is ClangCommon.ICommonLinkerSettings clangLinker)
                        {
                            clangLinker.RPath.AddUnique("@executable_path/../Frameworks/");
                        }
                    }
                    else
                    {
                        linuxLinker?.RPath.AddUnique("$ORIGIN");
                    }
                }
            );

            // put dynamic modules in the right place
            foreach (var dynmodule in collator.Find<Python.DynamicExtensionModule>())
            {
                var collatedDynModule = (dynmodule as Publisher.CollatedObject);
                collator.SetPublishingDirectoryForPythonBinaryModule(collatedDynModule);
                if (null != platIndependentModules && !collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    // copy dynamic modules last, since they go inside the platform independent directory structure
                    collatedDynModule.Requires(platIndependentModules.First() as Bam.Core.Module);
                }
            }

            var sysConfigData = collator.Find<SysConfigDataPythonFile>().FirstOrDefault();
            if (null != sysConfigData)
            {
                var sysConfigDataModule = (sysConfigData as Publisher.CollatedObject);
                if (null != platIndependentModules)
                {
                    sysConfigDataModule.DependsOn(platIndependentModules.First() as Bam.Core.Module);
                }
                collator.SetPublishingDirectoryForPythonBinaryModule(sysConfigDataModule);
            }
            var pyConfigHeader = collator.Find<PyConfigHeader>().FirstOrDefault();
            if (null != pyConfigHeader)
            {
                var headerModule = (pyConfigHeader as Publisher.CollatedObject);
                if (null != platIndependentModules)
                {
                    headerModule.DependsOn(platIndependentModules.First() as Bam.Core.Module);
                }
                headerModule.SetPublishingDirectory("$(0)", new[] { collator.HeaderDir });
            }
            var pyMakeFile = collator.Find<PyMakeFile>().FirstOrDefault();
            if (null != pyMakeFile)
            {
                var makeFileModule = (pyMakeFile as Publisher.CollatedObject);
                if (null != platIndependentModules)
                {
                    makeFileModule.DependsOn(platIndependentModules.First() as Bam.Core.Module);
                }
                collator.SetPublishingDirectoryForPythonBinaryModule(makeFileModule);
            }
        }

        public static Bam.Core.Array<Publisher.ICollatedObject>
        IncludePythonStandardDistribution(
            this Publisher.Collation collator,
            Publisher.ICollatedObject anchor,
            Publisher.ICollatedObject pythonLib)
        {
            var pyLibCopy = collator.Find<PythonLibrary>().First();
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;

            Bam.Core.Array<Publisher.ICollatedObject> platIndependentModules = null;
            if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                platIndependentModules = collator.IncludeDirectories(pyLibDir, collator.DynamicLibraryDir, anchor as Publisher.CollatedObject, renameLeaf: "lib");
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                if (Publisher.Collation.EPublishingType.WindowedApplication == collator.PublishingType)
                {
                    platIndependentModules = collator.IncludeDirectories(pyLibDir, collator.DynamicLibraryDir, anchor as Publisher.CollatedObject, renameLeaf: System.String.Format("python{0}", Version.MajorDotMinor));
                }
                else
                {
                    platIndependentModules = collator.IncludeDirectories(pyLibDir, collator.CreateTokenizedString("$(0)/lib", collator.DynamicLibraryDir), anchor as Publisher.CollatedObject, renameLeaf: System.String.Format("python{0}", Version.MajorDotMinor));
                }
            }
            else if (collator.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                platIndependentModules = collator.IncludeDirectories(pyLibDir, collator.CreateTokenizedString("$(0)/lib", collator.ExecutableDir), anchor as Publisher.CollatedObject, renameLeaf: System.String.Format("python{0}", Version.MajorDotMinor));
            }
            else
            {
                throw new Bam.Core.Exception("Unknown platform");
            }

            IncludePythonPlatformDependentModules(collator, anchor, platIndependentModules);

            // standard distribution should copy AFTER the Python library
            foreach (var module in platIndependentModules)
            {
                (module as Bam.Core.Module).DependsOn(pythonLib as Bam.Core.Module);
            }

            return platIndependentModules;
        }
    }
}
