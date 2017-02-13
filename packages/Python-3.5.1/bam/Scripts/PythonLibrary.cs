#region License
// Copyright (c) 2010-2017, Mark Final
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
    [C.Thirdparty("$(packagedir)/PC/python_nt.rc")]
    class PythonLibrary :
        C.DynamicLibrary
    {
        public Bam.Core.TokenizedString
        LibraryDirectory
        {
            get
            {
                return this.Macros["PythonLibDirectory"];
            }
        }

        private void
        CoreBuildPatch(
            Bam.Core.Settings settings)
        {
            var compiler = settings as C.ICommonCompilerSettings;
            compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
            compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
            var winCompiler = settings as C.ICommonCompilerSettingsWin;
            if (null != winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            }
            var visualcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
            if (null != visualcCompiler)
            {
                // warnings are present over warning level 2
                visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
            }
            var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
            if (null != gccCompiler)
            {
                gccCompiler.AllWarnings = false;
                gccCompiler.ExtraWarnings = false;
                gccCompiler.Pedantic = false;
            }
            var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
            if (null != clangCompiler)
            {
                clangCompiler.AllWarnings = false;
                clangCompiler.ExtraWarnings = false;
                clangCompiler.Pedantic = false;
            }
        }

        private void
        NotPyDEBUGPatch(
            Bam.Core.Settings settings)
        {
            var compiler = settings as C.ICommonCompilerSettings;
            compiler.PreprocessorDefines.Add("NDEBUG"); // ignore asserts, which depend on Py_DEBUG
        }

        private void
        VCNotPyDEBUGClosingPatch(
            Bam.Core.Settings settings)
        {
            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
            if (null != vcCompiler)
            {
                if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreaded ||
                    vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDLL)
                {
                    NotPyDEBUGPatch(settings);
                }
                else
                {
                    this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python35_d");
                }
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python35");
            }
            else
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");
            }
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("3");
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("5");
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim("1");

            this.Macros["PythonLibDirectory"] = this.CreateTokenizedString("$(packagedir)/Lib");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        }
                    }
                });

            var headers = this.CreateHeaderContainer("$(packagedir)/Include/*.h");

            var parserSource = this.CreateCSourceContainer("$(packagedir)/Parser/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$"));
            parserSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Parser/*.h");

            var objectSource = this.CreateCSourceContainer("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(this.CoreBuildPatch);

            objectSource["bytesobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));
            objectSource["odictobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));
            headers.AddFiles("$(packagedir)/Objects/*.h");

            var pythonSource = this.CreateCSourceContainer("$(packagedir)/Python/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup)(?!.*frozenmain)(?!.*sigcheck).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
            }
            else
            {
                // don't use dynload_next, as it's for older OSX (10.2 or below)
                pythonSource.AddFiles("$(packagedir)/Python/dynload_shlib.c");
            }
            pythonSource.PrivatePatch(this.CoreBuildPatch);

            pythonSource["_warnings.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                pythonSource["dtoa.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("parentheses"); // if (y = value) type expression
                    }));
                pythonSource["pytime.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("tautological-constant-out-of-range-compare"); // numbers out of range of comparison
                    }));
            }
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.NotWindows))
            {
                // TODO: I cannot see how else some symbols are exported with preprocessor settings
                pythonSource["getargs.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Visibility = GccCommon.EVisibility.Default;
                        }

                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            clangCompiler.Visibility = ClangCommon.EVisibility.Default;
                        }
                    }));
                pythonSource["getplatform.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                            {
                                compiler.PreprocessorDefines.Add("PLATFORM", "\"linux\"");
                            }
                            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                            {
                                compiler.PreprocessorDefines.Add("PLATFORM", "\"darwin\"");
                            }
                        }));
            }
            else
            {
                pythonSource["compile.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4312"); // Python-3.5.1\Python\compile.c(480): warning C4312: 'type cast': conversion from 'unsigned int' to 'void *' of greater size
                        }
                    }));
            }
            headers.AddFiles("$(packagedir)/Python/*.h");

            var builtinModuleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            // Windows builds includes dynamic modules builtin the core library
            // see PC/config.c
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_struct.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_csv.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/audioop.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/md5module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/binascii.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));
                builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
                builtinModuleSource["zlibmodule.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
                        }));
                builtinModuleSource.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c"); // _multibytecodec, _codecs_cn, _codecs_hk, _codecs_iso2022, _codecs_jp, _codecs_kr, _codecs_tw
                // old
                builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_hashopenssl.c"); // needs OpenSSL
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_ssl.c"); // needs OpenSSL
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testbuffer.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_testcapimodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testimportmultiple.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testmultiphase.c");
            }
            else
            {
                // TODO: this should be following the rules in Modules/makesetup and Modules/Setup.dist
                // for which modules are static (and thus part of the Python library) and which are shared
                // and separate in the distribution
                // note that you need to read Setup.dist backward, as some modules are mentioned twice
                // and it is the 'topmost' that overrules
            }

            // common statically compiled extension modules
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_stat.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_io/*.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            var traceMallocModule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
            traceMallocModule[0].PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4359"); // Python-3.5.1\Modules\_tracemalloc.c(67): warning C4359: '<unnamed-tag>': Alignment specifier is less than actual alignment (8), and will be ignored
                    }
                });
            builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_weakref.c");

            // TODO: review
            builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");

            builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_sre.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
                var msvcrtmodule = builtinModuleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
                msvcrtmodule[0].PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4312"); // Python-3.5.1\PC\msvcrtmodule.c(391): warning C4312: 'type cast': conversion from 'int' to '_HFILE' of greater size
                            compiler.DisableWarnings.AddUnique("4311"); // Python-3.5.1\PC\msvcrtmodule.c(391): warning C4311: 'type cast': pointer truncation from '_HFILE' to 'long'
                        }
                    });
            }
            else
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/getpath.c");

                var ModuleConfigSourceFile = Bam.Core.Graph.Instance.FindReferencedModule<ModuleConfigSourceFile>();
                builtinModuleSource.AddFile(ModuleConfigSourceFile);

                builtinModuleSource["getpath.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.PreprocessorDefines.Add("VERSION", "\"3.5\"");
                            compiler.PreprocessorDefines.Add("PYTHONPATH", "\".\"");
                        }));
            }

            builtinModuleSource.PrivatePatch(this.CoreBuildPatch);

            headers.AddFiles("$(packagedir)/Modules/*.h");
            headers.AddFiles("$(packagedir)/Modules/cjkcodecs/*.h");
            headers.AddFiles("$(packagedir)/Modules/zlib/*.h");
            headers.AddFiles("$(packagedir)/Modules/_io/*.h");

#if false
            // sigcheck has a simplified error check compared to signalmodule
            var signalSource = this.CreateCSourceContainer("$(packagedir)/Python/sigcheck.c");
            signalSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                    }
                });
#endif

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceContainer("$(packagedir)/PC/dl_nt.c");
                var pcConfig = pcSource.AddFiles("$(packagedir)/PC/config.c");
                pcConfig[0].PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("WIN32"); // required to register two extension modules
                    });
                //pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                var winreg = pcSource.AddFiles("$(packagedir)/PC/winreg.c");
                winreg[0].PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4311"); // Python-3.5.1\PC\winreg.c(885): warning C4311: 'type cast': pointer truncation from 'void *' to 'DWORD'
                        }
                    });
                pcSource.AddFiles("$(packagedir)/PC/invalid_parameter_handler.c"); // required by VS2015+
                pcSource.PrivatePatch(this.CoreBuildPatch);
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(parserSource);
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("Advapi32.lib");
                        linker.Libraries.Add("Ws2_32.lib");
                        linker.Libraries.Add("User32.lib");
                    });
                headers.AddFiles("$(packagedir)/PC/*.h");

                parserSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                objectSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                pythonSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                builtinModuleSource.ClosingPatch(VCNotPyDEBUGClosingPatch);

                if (null != this.WindowsVersionResource)
                {
                    this.WindowsVersionResource.PrivatePatch(settings =>
                        {
                            var rcCompiler = settings as C.ICommonWinResourceCompilerSettings;
                            rcCompiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                        });
                    this.WindowsVersionResource.UsePublicPatchesPrivately(C.DefaultToolchain.C_Compiler(this.BitDepth));

                    var versionHeader = Bam.Core.Graph.Instance.FindReferencedModule<PythonMakeVersionHeader>();
                    this.WindowsVersionResource.DependsOn(versionHeader);
                    this.WindowsVersionResource.UsePublicPatchesPrivately(versionHeader);
                    headers.AddFile(versionHeader);

                    this.PrivatePatch(settings =>
                        {
                            var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                            vcLinker.GenerateManifest = false; // as the .rc file refers to this already
                        });
                }
            }
            else
            {
                // TODO: is there a call for a CompileWith function?
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                this.UsePublicPatches(pyConfigHeader);
                parserSource.DependsOn(pyConfigHeader);
                objectSource.DependsOn(pyConfigHeader);
                pythonSource.DependsOn(pyConfigHeader);
                builtinModuleSource.DependsOn(pyConfigHeader);
                // TODO: end of function

                if (!pyConfigHeader.PyDEBUG)
                {
                    parserSource.PrivatePatch(NotPyDEBUGPatch);
                    objectSource.PrivatePatch(NotPyDEBUGPatch);
                    pythonSource.PrivatePatch(NotPyDEBUGPatch);
                    builtinModuleSource.PrivatePatch(NotPyDEBUGPatch);
                }

                var sysConfigDataPy = Bam.Core.Graph.Instance.FindReferencedModule<SysConfigDataPythonFile>();
                this.Requires(sysConfigDataPy);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-lpthread");
                        linker.Libraries.Add("-lm");
                        linker.Libraries.Add("-ldl");
                    });

                // TODO: would like to do this, but can't, see bug#101
                //headers.AddFile(pyConfigHeader);
                headers.AddFile(pyConfigHeader.GeneratedPaths[PyConfigHeader.Key].Parse());
            }
        }
    }
}
