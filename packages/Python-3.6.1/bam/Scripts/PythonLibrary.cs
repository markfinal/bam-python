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
    [C.Thirdparty("$(packagedir)/PC/python_nt.rc")]
    class PythonLibrary :
        C.DynamicLibrary
    {
        public Bam.Core.TokenizedString LibraryDirectory => this.Macros["PythonLibDirectory"];

        private void
        CoreBuildPatch(
            Bam.Core.Settings settings)
        {
            var compiler = settings as C.ICommonCompilerSettings;
            compiler.WarningsAsErrors = false;

            var preprocessor = settings as C.ICommonPreprocessorSettings;
            preprocessor.PreprocessorDefines.Add("Py_BUILD_CORE");
            preprocessor.PreprocessorDefines.Add("Py_ENABLE_SHARED");

            var cCompiler = settings as C.ICOnlyCompilerSettings;
            cCompiler.LanguageStandard = C.ELanguageStandard.C99; // some C99 features are now used from 3.6 (https://www.python.org/dev/peps/pep-0007/#c-dialect)

            if (settings is C.ICommonCompilerSettingsWin winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            }
            if (settings is VisualCCommon.ICommonCompilerSettings visualcCompiler)
            {
                visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
            }
            if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
            {
                gccCompiler.AllWarnings = true;
                gccCompiler.ExtraWarnings = true;
                gccCompiler.Pedantic = true;
            }
            if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
            {
                clangCompiler.AllWarnings = true;
                clangCompiler.ExtraWarnings = true;
                clangCompiler.Pedantic = true;
            }
        }

        private void
        NotPyDEBUGPatch(
            Bam.Core.Settings settings)
        {
            // need asserts to dissolve to nothing, since they contain expressions which
            // depende on Py_DEBUG being set
            var preprocessor = settings as C.ICommonPreprocessorSettings;
            preprocessor.PreprocessorDefines.Add("NDEBUG");
        }

        private void
        WinNotUnicodePatch(
            Bam.Core.Settings settings)
        {
            if (settings is C.ICommonCompilerSettingsWin winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            }
        }

        protected override void
        Init()
        {
            base.Init();

            // although PyConfigHeader is only explicitly used on non-Windows platforms in the main library, it's needed
            // for the closing patch on Windows, and the output name of the library
            var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim(Version.WindowsDebugOutputName);
                }
                else
                {
                    this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim(Version.WindowsOutputName);
                }
            }
            else
            {
                this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim(Version.NixOutputName);
            }
            this.SetSemanticVersion(Version.Major, Version.Minor, Version.Patch);

            this.Macros["PythonLibDirectory"] = this.CreateTokenizedString("$(packagedir)/Lib");

            /*
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonCompilerSettings compiler)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.SystemIncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));

                            if (settings is VisualCCommon.ICommonCompilerSettings)
                            {
                                compiler.DisableWarnings.AddUnique("4115"); // python-3.5.1\include\pytime.h(112): warning C4115: 'timeval': named type definition in parentheses
                            }
                        }

                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("long-long"); // Python-3.5.1/Include/pyport.h:58:27: error: ISO C90 does not support 'long long' [-Werror=long-long]
                        }
                    }
                });
                */

            var headers = this.CreateHeaderCollection("$(packagedir)/Include/*.h");

            var parserSource = this.CreateCSourceCollection(
                "$(packagedir)/Parser/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$")
            );
            parserSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Parser/*.h");


            var objectSource = this.CreateCSourceCollection("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Objects/*.h");


            var pythonSource = this.CreateCSourceCollection("$(packagedir)/Python/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup)(?!.*frozenmain)(?!.*sigcheck).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var dynload = pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
                /*
                dynload.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\dynload_win.c(191): warning C4100: 'fp': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4057"); // Python\dynload_win.c(148): warning C4057: 'function': 'const char *' differs in indirection to slightly different base types from 'unsigned char *'

                            if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebug ||
                                vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL)
                            {
                                compiler.DisableWarnings.AddUnique("4389"); // python-3.5.1\python\thread_nt.h(203): warning C4389: '==': signed/unsigned mismatch
                            }
                        }
                    });
                    */
            }
            else
            {
                // don't use dynload_next, as it's for older OSX (10.2 or below)
                var dynload = pythonSource.AddFiles("$(packagedir)/Python/dynload_shlib.c");
                /*
                dynload.First().PrivatePatch(settings =>
                    {
                        if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                        {
                            gccCompiler.Pedantic = false; // Python-3.5.1/Python/dynload_shlib.c:82:21: error: ISO C forbids conversion of object pointer to function pointer type [-Werror=pedantic]
                        }
                    });

                pythonSource["getargs.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        // TODO: I cannot see how else some symbols are exported with preprocessor settings
                        // there's an error of __PyArg_ParseTuple_SizeT is not exported
                        if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                        {
                            gccCompiler.Visibility = GccCommon.EVisibility.Default;
                        }

                        if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                        {
                            clangCompiler.Visibility = ClangCommon.EVisibility.Default;
                        }
                    }));

                pythonSource["sysmodule.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        // TODO
                        // no ABI defined, see sysconfig.py: TODO could get this from the compiler version?
                        if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                        {
                            var preprocessor = settings as C.ICommonPreprocessorSettings;
                            preprocessor.PreprocessorDefines.Add("ABIFLAGS", "\"\"");
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                        {
                            var preprocessor = settings as C.ICommonPreprocessorSettings;
                            preprocessor.PreprocessorDefines.Add("ABIFLAGS", "\"\"");
                        }
                    }));

                pythonSource["getplatform.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                        {
                            preprocessor.PreprocessorDefines.Add("PLATFORM", "\"linux\"");
                        }
                        else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                        {
                            preprocessor.PreprocessorDefines.Add("PLATFORM", "\"darwin\"");
                        }
                    }));
                    */
            }
            pythonSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Python/*.h");


            var builtinModuleSource = this.CreateCSourceCollection("$(packagedir)/Modules/main.c");
            builtinModuleSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Modules/*.h");
            headers.AddFiles("$(packagedir)/Modules/cjkcodecs/*.h");
#if !PYTHON_USE_ZLIB_PACKAGE
            headers.AddFiles("$(packagedir)/Modules/zlib/*.h");
#endif
            headers.AddFiles("$(packagedir)/Modules/_io/*.h");

            builtinModuleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            // Windows builds includes dynamic modules builtin the core library
            // see PC/config.c
            var cjkcodecs = this.CreateCSourceCollection(); // empty initially, as only Windows populates it as static modules
            cjkcodecs.PrivatePatch(this.CoreBuildPatch);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c"); // part of _lsprof
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c"); // part of cmath
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

#if PYTHON_USE_ZLIB_PACKAGE
#else
                var zlib = this.CreateCSourceCollection("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));
                zlib.PrivatePatch(this.WinNotUnicodePatch);
#endif

                var zlibmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
                zlibmodule.First().PrivatePatch(settings =>
                    {
#if !PYTHON_USE_ZLIB_PACKAGE
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
#endif
                    });

#if PYTHON_USE_ZLIB_PACKAGE
                this.CompileAndLinkAgainst<global::zlib.ZLib>(zlibmodule.First() as C.CModule);
#endif

                cjkcodecs.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c"); // _multibytecodec, _codecs_cn, _codecs_hk, _codecs_iso2022, _codecs_jp, _codecs_kr, _codecs_tw

                builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");

                builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2s_impl.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2b_impl.c");

                builtinModuleSource.AddFiles("$(packagedir)/Modules/_sha3/sha3module.c");
            }
            else
            {
                // TODO: this should be following the rules in Modules/makesetup and Modules/Setup.dist
                // for which modules are static (and thus part of the Python library) and which are shared
                // and separate in the distribution
                // note that you need to read Setup.dist backward, as some modules are mentioned twice
                // and it is the 'topmost' that overrules
                builtinModuleSource.AddFiles("$(packagedir)/Modules/pwdmodule.c");
            }

            // common statically compiled extension modules
            builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c"); // implements nt module on Windows
            builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_sre.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_weakref.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_stat.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");

            var _io = this.CreateCSourceCollection("$(packagedir)/Modules/_io/*.c");
            _io.PrivatePatch(this.CoreBuildPatch);
            /*
            _io["_iomodule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        // C:\Program Files (x86)\Windows Kits\10\include\10.0.17134.0\um\winnt.h(154): fatal error C1189: #error:  "No Target Architecture"
                        if (Bam.Core.OSUtilities.Is64Bit(item.BuildEnvironment.Platform))
                        {
                            preprocessor.PreprocessorDefines.Add("_AMD64_");
                        }
                        else
                        {
                            preprocessor.PreprocessorDefines.Add("_X86_");
                        }
                    }
                }));
                */

            builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c"); // part of _tracemalloc
            builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");

            // TODO: review
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
                builtinModuleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
            }
            else
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/getpath.c");

                var ModuleConfigSourceFile = Bam.Core.Graph.Instance.FindReferencedModule<ModuleConfigSourceFile>();
                builtinModuleSource.AddFile(ModuleConfigSourceFile);

                /*
                builtinModuleSource["getpath.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var preprocessor = settings as C.ICommonPreprocessorSettings;
                            // TODO: these should be configurables
                            preprocessor.PreprocessorDefines.Add("PREFIX", "\".\"");
                            preprocessor.PreprocessorDefines.Add("EXEC_PREFIX", "\".\"");
                            preprocessor.PreprocessorDefines.Add("PYTHONPATH", "\".:./lib-dynload\""); // TODO: this was in pyconfig.h for PC, so does it need moving?
                            preprocessor.PreprocessorDefines.Add("VERSION", $"\"{Version.MajorDotMinor}\"");
                            preprocessor.PreprocessorDefines.Add("VPATH", "\".\"");
                        }));
                        */
            }

#if false
            // sigcheck has a simplified error check compared to signalmodule
            var signalSource = this.CreateCSourceCollection("$(packagedir)/Python/sigcheck.c");
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

            /*
            // TODO: is there a call for a CompileWith function?
            this.UsePublicPatches(pyConfigHeader);
            parserSource.DependsOn(pyConfigHeader);
            objectSource.DependsOn(pyConfigHeader);
            pythonSource.DependsOn(pyConfigHeader);
            builtinModuleSource.DependsOn(pyConfigHeader);
            cjkcodecs.DependsOn(pyConfigHeader);
            _io.DependsOn(pyConfigHeader);
            // TODO: end of function
            */

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceCollection("$(packagedir)/PC/dl_nt.c");
                pcSource.PrivatePatch(this.CoreBuildPatch);
                /*
                pcSource["dl_nt.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            if (settings is VisualCCommon.ICommonCompilerSettings)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\dl_nt.c(90): warning C4100: 'lpReserved': unreferenced formal parameter
                            }
                        }));
                        */
                var pcConfig = pcSource.AddFiles("$(packagedir)/PC/config.c");
                /*
                pcConfig.First().PrivatePatch(settings =>
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("WIN32"); // required to register two extension modules
                    });
                    */
                //pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                var getpathp = pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                /*
                getpathp.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\PC\getpathp.c(144): warning C4267: '=': conversion from 'size_t' to 'int', possible loss of data
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\PC\getpathp.c(289): warning C4456: declaration of 'keyBuf' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\PC\getpathp.c(324): warning C4189: 'reqdSize': local variable is initialized but not referenced
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\pc\getpathp.c(548) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4459"); // Python-3.6.1\PC\getpathp.c(541): warning C4459: declaration of 'prefix' hides global declaration
                        }
                    });
                    */
                var winreg = pcSource.AddFiles("$(packagedir)/PC/winreg.c");
                /*
                winreg.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4311"); // Python-3.5.1\PC\winreg.c(885): warning C4311: 'type cast': pointer truncation from 'void *' to 'DWORD'
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\winreg.c(118): warning C4100: 'ob': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\PC\winreg.c(729): warning C4456: declaration of 'len' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\PC\winreg.c(1392): warning C4057: 'function': 'PLONG' differs in indirection to slightly different base types from 'DWORD *'
                            if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebug ||
                                vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL)
                            {
                                compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\PC\winreg.c(578): warning C4389: '==': signed/unsigned mismatch
                            }
                            var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                (settings.Module as C.CCompilableModuleCollection<C.ObjectFile>).Compiler :
                                (settings.Module as C.ObjectFile).Compiler;
                            if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2013))
                            {
                                compiler.DisableWarnings.AddUnique("4305"); // Python-3.5.1\PC\winreg.c(885) : warning C4305: 'type cast' : truncation from 'void *' to 'DWORD'
                            }
                        }
                    });
                    */
                var invalid_parameter_handle = pcSource.AddFiles("$(packagedir)/PC/invalid_parameter_handler.c"); // required by VS2015+
                /*
                invalid_parameter_handle.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\invalid_parameter_handler.c(16): warning C4100: 'pReserved': unreferenced formal parameter
                        }
                    });
                    */
                /*
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("Advapi32.lib");
                        linker.Libraries.Add("Ws2_32.lib");
                        linker.Libraries.Add("User32.lib");
                        linker.Libraries.Add("Shlwapi.lib");
                        linker.Libraries.Add("version.lib");
                    });
                    */
                headers.AddFiles("$(packagedir)/PC/*.h");

                if (!(pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    pcSource.ClosingPatch(NotPyDEBUGPatch);
                }

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
                    versionHeader.PrivatePatch(settings =>
                        {
                            if (parserSource.Settings is VisualCCommon.ICommonCompilerSettings)
                            {
                                var crt = (parserSource.Settings as VisualCCommon.ICommonCompilerSettings).RuntimeLibrary;
                                (versionHeader.Configuration as ConfigurePythonResourceHeader).DebugCRT =
                                    (VisualCCommon.ERuntimeLibrary.MultiThreadedDebug == crt) ||
                                    (VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL == crt);
                            }
                        });

                    /*
                    this.PrivatePatch(settings =>
                        {
                            var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                            vcLinker.GenerateManifest = false; // as the .rc file refers to this already
                        });
                        */
                }
            }
            else
            {
                var sysConfigDataPy = Bam.Core.Graph.Instance.FindReferencedModule<SysConfigDataPythonFile>();
                this.Requires(sysConfigDataPy);

                var pyMakeFile = Bam.Core.Graph.Instance.FindReferencedModule<PyMakeFile>();
                this.Requires(pyMakeFile);

                /*
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-lpthread");
                        linker.Libraries.Add("-lm");
                        linker.Libraries.Add("-ldl");
                    });
                    */

                headers.AddFile(pyConfigHeader);
            }

            if (!(pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
            {
                parserSource.PrivatePatch(NotPyDEBUGPatch);
                objectSource.PrivatePatch(NotPyDEBUGPatch);
                pythonSource.PrivatePatch(NotPyDEBUGPatch);
                builtinModuleSource.PrivatePatch(NotPyDEBUGPatch);
                cjkcodecs.PrivatePatch(NotPyDEBUGPatch);
                _io.PrivatePatch(NotPyDEBUGPatch);
            }
        }
    }
}
