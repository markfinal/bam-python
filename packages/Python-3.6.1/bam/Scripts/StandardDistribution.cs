#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Collections.Generic;

namespace Python
{
    class AllDynamicModules :
        Bam.Core.Module
    {
        private void
        RequiredToExist<T>() where T : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<T>();
            this.Requires(dependent);
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // common to all platforms
            this.RequiredToExist<_multiprocessing>();
            this.RequiredToExist<_ctypes>();
            this.RequiredToExist<_testmultiphase>();
            this.RequiredToExist<_testimportmultiple>();
            this.RequiredToExist<_testbuffer>();
            this.RequiredToExist<_testcapi>();
            this.RequiredToExist<_elementtree>();
            this.RequiredToExist<unicodedata>();
            this.RequiredToExist<select>();
            this.RequiredToExist<_socket>();
            this.RequiredToExist<fpectl>();
            this.RequiredToExist<fpetest>();
            this.RequiredToExist<pyexpat>();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.NotWindows))
            {
                // extension modules
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    this.RequiredToExist<_scproxy>();
                }
                this.RequiredToExist<_opcode>();
                this.RequiredToExist<_lsprof>();
                this.RequiredToExist<_json>();
                this.RequiredToExist<_thread>();
                this.RequiredToExist<array>();
                this.RequiredToExist<cmath>();
                this.RequiredToExist<math>();
                this.RequiredToExist<_struct>();
                this.RequiredToExist<_random>();
                this.RequiredToExist<_pickle>();
                this.RequiredToExist<_datetime>();
                this.RequiredToExist<_bisect>();
                this.RequiredToExist<_heapq>();
                this.RequiredToExist<fcntl>();
                this.RequiredToExist<grp>();
                this.RequiredToExist<mmap>();
                this.RequiredToExist<_csv>();
                this.RequiredToExist<_crypt>();
                this.RequiredToExist<nis>();
                this.RequiredToExist<termios>();
                this.RequiredToExist<resource>();
                this.RequiredToExist<_posixsubprocess>();
                this.RequiredToExist<audioop>();
                this.RequiredToExist<_md5>();
                this.RequiredToExist<_sha1>();
                this.RequiredToExist<_sha256>();
                this.RequiredToExist<_sha512>();
                this.RequiredToExist<syslog>();
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    this.RequiredToExist<_curses>();
                    this.RequiredToExist<_curses_panel>();
                }
                this.RequiredToExist<binascii>();
                this.RequiredToExist<parser>();
                this.RequiredToExist<zlib>();
                this.RequiredToExist<_multibytecodec>();
                this.RequiredToExist<_codecs_cn>();
                this.RequiredToExist<_codecs_hk>();
                this.RequiredToExist<_codecs_iso2022>();
                this.RequiredToExist<_codecs_jp>();
                this.RequiredToExist<_codecs_kr>();
                this.RequiredToExist<_codecs_tw>();
                this.RequiredToExist<xxsubtype>();
                this.RequiredToExist<_blake2>();
                this.RequiredToExist<_sha3>();

#if PYTHON_WITH_OPENSSL
                this.RequiredToExist<_ssl>();
                this.RequiredToExist<_hashlib>();
#endif
#if PYTHON_WITH_SQLITE
                this.RequiredToExist<_sqlite3>();
#endif
            }
        }

        protected override void EvaluateInternal()
        {
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    public class PythonZip :
        Publisher.ZipModule
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            var basename = Version.WindowsOutputName; // pythonMN.zip - applicable to all platforms
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
#if BAM_FEATURE_MODULE_CONFIGURATION
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
#else
                if (pyConfigHeader.PyDEBUG)
#endif
                {
                    basename = Version.WindowsDebugOutputName; // pythonMN_d.zip
                }
            }
            this.Macros.Add("zipoutputbasename", basename);

            var pylib = Bam.Core.Graph.Instance.FindReferencedModule<Python.PythonLibrary>();
            this.DependsOn(pylib);
            this.Macros.Add("pathtozip", pylib.LibraryDirectory);

            base.Init(parent);

            this.PrivatePatch(settings =>
                {
                    var zipSettings = settings as Publisher.IZipSettings;
                    zipSettings.RecursivePaths = true;
                    zipSettings.Update = true;
                }
            );
        }

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                // since there is a dependency on PythonLibrary which does not need to be passed
                // through to Zip
                return System.Linq.Enumerable.Empty<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>>();
            }
        }
    }
}
namespace Python.StandardDistribution
{
    public static class PublisherExtensions
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
                Publisher.ZipModule.ZipKey,
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
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                    }
                    if (Publisher.Collation.EPublishingType.WindowedApplication == collator.PublishingType)
                    {
                        if (null != gccLinker)
                        {
                            gccLinker.RPath.AddUnique("$ORIGIN/../lib");
                        }
                        var clangLinker = settings as ClangCommon.ICommonLinkerSettings;
                        if (null != clangLinker)
                        {
                            clangLinker.RPath.AddUnique("@executable_path/../Frameworks/");
                        }
                    }
                    else
                    {
                        if (null != gccLinker)
                        {
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        }
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
