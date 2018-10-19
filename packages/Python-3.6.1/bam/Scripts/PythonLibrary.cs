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
            var cCompiler = settings as C.ICOnlyCompilerSettings;
            cCompiler.LanguageStandard = C.ELanguageStandard.C99; // some C99 features are now used from 3.6 (https://www.python.org/dev/peps/pep-0007/#c-dialect)
            var winCompiler = settings as C.ICommonCompilerSettingsWin;
            if (null != winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            }
            var visualcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
            if (null != visualcCompiler)
            {
                visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;

                // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                    (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                    (settings.Module as C.ObjectFile).Compiler;
                if (compilerUsed.IsAtLeast(19))
                {
                }
                else
                {
                    compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                }
            }
            var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
            if (null != gccCompiler)
            {
                gccCompiler.AllWarnings = true;
                gccCompiler.ExtraWarnings = true;
                gccCompiler.Pedantic = true;
            }
            var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
            if (null != clangCompiler)
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
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>(settings.Module.BuildEnvironment);

                if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreaded ||
                    vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDLL)
                {
                    if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                    {
                        throw new Bam.Core.Exception("VisualStudio non-debug runtime detected, but Python was configured in Py_DEBUG mode. Inconsistent states.");
                    }

                    NotPyDEBUGPatch(settings);
                }
                else
                {
                    this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(Version.WindowsDebugOutputName);

                    if (!(pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                    {
                        throw new Bam.Core.Exception("VisualStudio debug runtime detected, but Python was not configured in Py_DEBUG mode. Inconsistent states.");
                    }
                }
            }
        }

        private void
        WinNotUnicodePatch(
            Bam.Core.Settings settings)
        {
            var winCompiler = settings as C.ICommonCompilerSettingsWin;
            if (null != winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.NotSet;
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(Version.WindowsOutputName);
            }
            else
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(Version.NixOutputName);
            }
            this.SetSemanticVersion(Version.Major, Version.Minor, Version.Patch);

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

                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                compiler.DisableWarnings.AddUnique("4115"); // python-3.5.1\include\pytime.h(112): warning C4115: 'timeval': named type definition in parentheses
                            }
                        }

                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("long-long"); // Python-3.5.1/Include/pyport.h:58:27: error: ISO C90 does not support 'long long' [-Werror=long-long]
                        }
                    }
                });

            var headers = this.CreateHeaderContainer("$(packagedir)/Include/*.h");

            var parserSource = this.CreateCSourceContainer(
                "$(packagedir)/Parser/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$")
            );
            parserSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Parser/*.h");
            if (parserSource.Compiler is VisualCCommon.CompilerBase)
            {
                parserSource.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryParser());
            }
            else if (parserSource.Compiler is GccCommon.CompilerBase)
            {
                parserSource.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryParser());
            }
            else if (parserSource.Compiler is ClangCommon.CompilerBase)
            {
                parserSource.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryParser());
            }


            var objectSource = this.CreateCSourceContainer("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Objects/*.h");
            if (objectSource.Compiler is VisualCCommon.CompilerBase)
            {
                objectSource.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryObjects());
            }
            else if (objectSource.Compiler is GccCommon.CompilerBase)
            {
                objectSource.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryObjects());
            }
            else if (objectSource.Compiler is ClangCommon.CompilerBase)
            {
                objectSource.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryObjects());
            }


            var pythonSource = this.CreateCSourceContainer("$(packagedir)/Python/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup)(?!.*frozenmain)(?!.*sigcheck).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var dynload = pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
                dynload.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
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
            }
            else
            {
                // don't use dynload_next, as it's for older OSX (10.2 or below)
                var dynload = pythonSource.AddFiles("$(packagedir)/Python/dynload_shlib.c");
                dynload.First().PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Pedantic = false; // Python-3.5.1/Python/dynload_shlib.c:82:21: error: ISO C forbids conversion of object pointer to function pointer type [-Werror=pedantic]
                        }
                    });
            }
            pythonSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Python/*.h");

            if (pythonSource.Compiler is VisualCCommon.CompilerBase)
            {
                pythonSource.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryPython());
            }
            else if (pythonSource.Compiler is GccCommon.CompilerBase)
            {
                pythonSource.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryPython());
            }
            else if (pythonSource.Compiler is ClangCommon.CompilerBase)
            {
                pythonSource.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryPython());
            }

            var builtinModuleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            builtinModuleSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Modules/*.h");
            headers.AddFiles("$(packagedir)/Modules/cjkcodecs/*.h");
#if !PYTHON_USE_ZLIB_PACKAGE
            headers.AddFiles("$(packagedir)/Modules/zlib/*.h");
#endif
            headers.AddFiles("$(packagedir)/Modules/_io/*.h");

#if false
            builtinModuleSource["main.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\main.c(510) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/main.c:49:1: error: string length '525' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Modules/main.c:738:21: error: ISO C90 does not support the '%ls' gnu_printf format [-Werror=format=]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/main.c:49:24: error: string literal of length 525 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
#endif
            builtinModuleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            // Windows builds includes dynamic modules builtin the core library
            // see PC/config.c
            var cjkcodecs = this.CreateCSourceContainer(); // empty initially, as only Windows populates it as static modules
            cjkcodecs.PrivatePatch(this.CoreBuildPatch);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var opcode = builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
#if false
                opcode.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_opcode.c(23): warning C4100: 'module': unreferenced formal parameter
                        }
                    });
#endif
                var lsprof = builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
#if false
                lsprof.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_lsprof.c(264): warning C4100: 'pObj': unreferenced formal parameter
                        }
                    });
#endif
                builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c"); // part of _lsprof
                var json = builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
#if false
                json.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_json.c(590): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_json.c(308): warning C4244: '=': conversion from 'Py_UCS4' to 'Py_UCS1', possible loss of data
                        }
                    });
#endif
                var threadmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
#if false
                threadmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_threadmodule.c(445): warning C4100: 'kwds': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_threadmodule.c(784) : warning C4706: assignment within conditional expression
                        }
                    });
#endif
                var arraymodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
#if false
                arraymodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\arraymodule.c(777): warning C4100: 'unused': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\arraymodule.c(1803): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\arraymodule.c(2087): warning C4456: declaration of 'descr' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\arraymodule.c(2192): warning C4244: 'function': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\arraymodule.c(3021): warning C4152: nonstandard extension, function/data pointer conversion in expression
                        }
                    });
#endif
                var cmathmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
#if false
                cmathmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\cmathmodule.c(436): warning C4100: 'module': unreferenced formal parameter
                        }
                    });
#endif
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c"); // part of cmath
                var mathmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
#if false
                mathmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\mathmodule.c(689): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\mathmodule.c(1217) : warning C4701: potentially uninitialized local variable 'lo' used
                        }
                    });
#endif
                var _struct = builtinModuleSource.AddFiles("$(packagedir)/Modules/_struct.c");
#if false
                _struct.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_struct.c(346): warning C4100: 'f': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_struct.c(678): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                        }
                    });
#endif
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
                var _pickle = builtinModuleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
#if false
                _pickle.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_pickle.c(875): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\_pickle.c(3267): warning C4456: declaration of 'st' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Modules\_pickle.c(3552): warning C4457: declaration of 'args' hides function parameter
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_pickle.c(4652): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_pickle.c(398) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\_pickle.c(741) : warning C4702: unreachable code
                        }
                    });
#endif
                var _datetimemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
#if false
                _datetimemodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_datetimemodule.c(652): warning C4244: '=': conversion from 'int' to 'unsigned char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Modules\_datetimemodule.c(1290): warning C4457: declaration of 'format' hides function parameter
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_datetimemodule.c(2398): warning C4100: 'unused': unreferenced formal parameter
                        }
                    });
#endif
                var _bisectmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
#if false
                _bisectmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_bisectmodule.c(47): warning C4100: 'self': unreferenced formal parameter
                        }
                    });
#endif
                var _heapqmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
#if false
                _heapqmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_heapqmodule.c(100): warning C4100: 'self': unreferenced formal parameter
                        }
                    });
#endif
                var mmapmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
#if false
                mmapmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\mmapmodule.c(145): warning C4100: 'unused': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\Modules\mmapmodule.c(514): warning C4057: 'function': 'PLONG' differs in indirection to slightly different base types from 'DWORD *'
                            var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                (settings.Module as C.ObjectFile).Compiler;
                            if (compilerUsed.IsAtLeast(18))
                            {
                            }
                            else
                            {
                                compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\mmapmodule.c(1335) : warning C4306: 'type cast' : conversion from 'int' to 'HANDLE' of greater size
                            }
                        }
                    });
#endif
                var _csv = builtinModuleSource.AddFiles("$(packagedir)/Modules/_csv.c");
#if false
                _csv.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_csv.c(193): warning C4100: 'name': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4245"); // Python-3.5.1\Modules\_csv.c(1119): warning C4245: 'initializing': conversion from 'int' to 'unsigned int', signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_csv.c(1279) : warning C4706: assignment within conditional expression
                        }
                    });
#endif
                var audioop = builtinModuleSource.AddFiles("$(packagedir)/Modules/audioop.c");
#if false
                audioop.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\audioop.c(66): warning C4244: 'return': conversion from 'int' to 'PyInt16', possible loss of data
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\audioop.c(410): warning C4100: 'module': unreferenced formal parameter
                        }
                    });
#endif
                var md5module = builtinModuleSource.AddFiles("$(packagedir)/Modules/md5module.c");
#if false
                md5module.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\md5module.c(432): warning C4100: 'closure': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\md5module.c(536) : warning C4701: potentially uninitialized local variable 'buf' used
                        }
                    });
#endif
                var sha1module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
#if false
                sha1module.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\sha1module.c(409): warning C4100: 'closure': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\sha1module.c(513) : warning C4701: potentially uninitialized local variable 'buf' used
                        }
                    });
#endif
                var sha256module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
#if false
                sha256module.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\sha256module.c(499): warning C4100: 'closure': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\sha256module.c(634) : warning C4701: potentially uninitialized local variable 'buf' used
                        }
                    });
#endif
                var sha512module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
#if false
                sha512module.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\sha512module.c(570): warning C4100: 'closure': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\sha512module.c(705) : warning C4701: potentially uninitialized local variable 'buf' used
                        }
                    });
#endif
                var binascii = builtinModuleSource.AddFiles("$(packagedir)/Modules/binascii.c");
#if false
                binascii.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\binascii.c(256): warning C4100: 'module': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\binascii.c(1203): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                        }
                    });
#endif
                var parsermodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
#if false
                parsermodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\parsermodule.c(396): warning C4100: 'unused': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\parsermodule.c(793): warning C4456: declaration of 'err' hides previous local declaration
                        }
                    });
#endif

#if PYTHON_USE_ZLIB_PACKAGE
#else
                var zlib = this.CreateCSourceContainer("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));
                zlib.PrivatePatch(this.WinNotUnicodePatch);
#if false
                zlib.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4131"); // Python-3.5.1\Modules\zlib\adler32.c(66): warning C4131: 'adler32': uses old-style declarator
                        }
                    });
                zlib["crc32.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\zlib\crc32.c(217): warning C4127: conditional expression is constant
                            }
                        }));
                zlib["deflate.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zlib\deflate.c(811): warning C4244: '=': conversion from 'int' to 'Bytef', possible loss of data
                                compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\zlib\deflate.c(1404): warning C4127: conditional expression is constant
                            }
                        }));
                zlib["gzlib.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.PreprocessorDefines.Add("_CRT_SECURE_NO_WARNINGS"); // Python-3.5.1\Modules\zlib\gzlib.c(193): warning C4996: 'wcstombs': This function or variable may be unsafe.
                                compiler.DisableWarnings.AddUnique("4996"); // Python-3.5.1\Modules\zlib\gzlib.c(245): warning C4996: 'open': The POSIX name for this item is deprecated.
                            }
                        }));
                zlib["gzread.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.PreprocessorDefines.Add("_CRT_SECURE_NO_WARNINGS"); // Python-3.5.1\Modules\zlib\gzread.c(36): warning C4996: 'strerror': This function or variable may be unsafe.
                                compiler.DisableWarnings.AddUnique("4996"); // Python-3.5.1\Modules\zlib\gzread.c(30): warning C4996: 'read': The POSIX name for this item is deprecated.
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zlib\gzread.c(454): warning C4244: '=': conversion from 'int' to 'unsigned char', possible loss of data
                            }
                        }));
                zlib["gzwrite.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.PreprocessorDefines.Add("_CRT_SECURE_NO_WARNINGS"); // Python-3.5.1\Modules\zlib\gzwrite.c(86): warning C4996: 'strerror': This function or variable may be unsafe.
                                compiler.DisableWarnings.AddUnique("4996"); // Python-3.5.1\Modules\zlib\gzwrite.c(84): warning C4996: 'write': The POSIX name for this item is deprecated.
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zlib\gzwrite.c(278): warning C4244: '=': conversion from 'int' to 'unsigned char', possible loss of data
                            }
                        }));
                zlib["inflate.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zlib\inflate.c(764): warning C4244: '=': conversion from 'unsigned int' to 'Bytef', possible loss of data
                            }
                        }));
                zlib["trees.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zlib\trees.c(602): warning C4244: '=': conversion from 'unsigned int' to 'ush', possible loss of data
                            }
                        }));
#endif
#endif

                var zlibmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
                zlibmodule.First().PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
#if !PYTHON_USE_ZLIB_PACKAGE
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
#endif
#if false
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\zlibmodule.c(122): warning C4100: 'ctx': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\zlibmodule.c(308) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.6.1\Modules\zlibmodule.c(145): warning C4267: '=': conversion from 'size_t' to 'uInt', possible loss of data
                        }
#endif
                    });

#if PYTHON_USE_ZLIB_PACKAGE
                this.CompileAndLinkAgainst<global::zlib.ZLib>(zlibmodule.First() as C.CModule);
#endif

                cjkcodecs.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c"); // _multibytecodec, _codecs_cn, _codecs_hk, _codecs_iso2022, _codecs_jp, _codecs_kr, _codecs_tw
#if false
                cjkcodecs.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // various
                            compiler.DisableWarnings.AddUnique("4127"); // various
                            compiler.DisableWarnings.AddUnique("4244"); // various
                            // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                            var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                (settings.Module as C.ObjectFile).Compiler;
                            if (compilerUsed.IsAtLeast(18))
                            {
                            }
                            else
                            {
                                compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\cjkcodecs\multibytecodec.c(72) : warning C4306: 'type cast' : conversion from 'int' to 'PyObject *' of greater size
                            }
                        }
                    });
#endif

                var xxsubtype = builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");
#if false
                xxsubtype.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\xxsubtype.c(236): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\xxsubtype.c(293): warning C4152: nonstandard extension, function/data pointer conversion in expression
                        }
                    });
#endif

                builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2module.c");
                var blake2s_impl = builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2s_impl.c");
#if false
                blake2s_impl.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4245"); // python-3.6.1\modules\_blake2\impl/blake2s-ref.c(45): warning C4245: '=': conversion from 'int' to 'uint32_t', signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.6.1\modules\_blake2\clinic/blake2s_impl.c.h(76): warning C4100: '_unused_ignored': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.6.1\Modules\_blake2\blake2s_impl.c(120): warning C4244: '=': conversion from 'int' to 'uint8_t', possible loss of data
                        }
                    });
#endif
                var blake2b_impl = builtinModuleSource.AddFiles("$(packagedir)/Modules/_blake2/blake2b_impl.c");
#if false
                blake2b_impl.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4245"); // python-3.6.1\modules\_blake2\impl/blake2b-ref.c(50): warning C4245: '=': conversion from 'int' to 'uint64_t', signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.6.1\modules\_blake2\clinic/blake2b_impl.c.h(76): warning C4100: '_unused_ignored': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.6.1\Modules\_blake2\blake2b_impl.c(120): warning C4244: '=': conversion from 'int' to 'uint8_t', possible loss of data
                        }
                    });
#endif

                var sha3module = builtinModuleSource.AddFiles("$(packagedir)/Modules/_sha3/sha3module.c");
#if false
                sha3module.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4324"); // python-3.6.1\modules\_sha3\kcp\KeccakSponge.h(168): warning C4324: 'KeccakWidth1600_SpongeInstanceStruct': structure was padded due to alignment specifier
                            compiler.DisableWarnings.AddUnique("4245"); // python-3.6.1\modules\_sha3\kcp/KeccakP-1600-opt64.c(254): warning C4245: '=': conversion from 'int' to 'UINT64', signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.6.1\modules\_sha3\clinic/sha3module.c.h(45): warning C4100: '_unused_ignored': unreferenced formal parameter
                        }
                    });
#endif
            }
            else
            {
                // TODO: this should be following the rules in Modules/makesetup and Modules/Setup.dist
                // for which modules are static (and thus part of the Python library) and which are shared
                // and separate in the distribution
                // note that you need to read Setup.dist backward, as some modules are mentioned twice
                // and it is the 'topmost' that overrules
                var pwdmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/pwdmodule.c");
#if false
                pwdmodule.First().PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/pwdmodule.c:108:27: error: unused parameter 'module' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/pwdmodule.c:205:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/pwdmodule.c:23:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/pwdmodule.c:108:27: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        }
                    });
#endif
            }

            // common statically compiled extension modules
            var signalmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
#if false
            signalmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\signalmodule.c(178): warning C4100: 'args': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Modules\signalmodule.c(1222): warning C4054: 'type cast': from function pointer '_crt_signal_t' to data pointer 'void *'
                        compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\Modules\signalmodule.c(258): warning C4057: 'function': 'const char *' differs in indirection to slightly different base types from 'unsigned char *'
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\signalmodule.c(1570) : warning C4706: assignment within conditional expression
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(18))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\signalmodule.c(434) : warning C4306: 'type cast' : conversion from 'int' to 'void (__cdecl *)(int)' of greater size
                        }
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/signalmodule.c:178:38: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/signalmodule.c:1141:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/signalmodule.c:1145:1: error: string length '1461' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/signalmodule.c:1222:45: error: ISO C forbids conversion of function pointer to object pointer type [-Werror=pedantic]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/signalmodule.c:1141:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/signalmodule.c:178:38: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/signalmodule.c:1146:1: error: string literal of length 1461 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                });
#endif
            var gcmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
#if false
            gcmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\gcmodule.c(370): warning C4100: 'data': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\gcmodule.c(1633) : warning C4706: assignment within conditional expression
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.6.1\Modules\gcmodule.c(1078): warning C4244: 'function': conversion from 'Py_ssize_t' to 'int', possible loss of data
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/gcmodule.c:370:34: error: unused parameter 'data' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/gcmodule.c:1484:1: error: string length '875' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/gcmodule.c:1520:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/gcmodule.c:370:34: error: unused parameter 'data' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/gcmodule.c:1485:1: error: string literal of length 875 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/gcmodule.c:1520:21: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var posixmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c"); // implements nt module on Windows
#if false
            posixmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4201"); // python-3.5.1\modules\winreparse.h(40): warning C4201: nonstandard extension used: nameless struct/union
                        compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Modules\posixmodule.c(869): warning C4389: '!=': signed/unsigned mismatch
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\posixmodule.c(1223): warning C4100: 'function': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\Modules\posixmodule.c(3321): warning C4267: 'function': conversion from 'size_t' to 'int', possible loss of data
                        compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\Modules\posixmodule.c(3466): warning C4189: 'po': local variable is initialized but not referenced
                        compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\Modules\posixmodule.c(3912): warning C4057: 'function': 'Py_ssize_t *' differs in indirection to slightly different base types from 'size_t *'
                        compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\posixmodule.c(4885) : warning C4702: unreachable code
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\posixmodule.c(5136) : warning C4701: potentially uninitialized local variable 'argc' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\posixmodule.c(4863) : warning C4703: potentially uninitialized local pointer variable 'hFile' used
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\posixmodule.c(7023) : warning C4706: assignment within conditional expression
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(18))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\posixmodule.c(10628) : warning C4306: 'type cast' : conversion from 'int' to 'HINSTANCE' of greatersize
                        }
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/posixmodule.c:1836:5: error: missing initializer for field 'doc' of 'PyStructSequence_Field' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/posixmodule.c:1928:28: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/posixmodule.c:4644:5: error: implicit declaration of function 'utime' [-Werror=implicit-function-declaration]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/posixmodule.c.h:5:1: error: string length '783' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Modules/posixmodule.c:1281:1: error: 'fildes_converter' defined but not used [-Werror=unused-function]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/posixmodule.c:1928:28: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/posixmodule.c:1775:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/posixmodule.c:4644:12: error: implicit declaration of function 'utime' [-Werror,-Wimplicit-function-declaration]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/posixmodule.c.h:6:1: error: string literal of length 783 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Modules/posixmodule.c:1281:1: error: unused function 'fildes_converter' [-Werror,-Wunused-function]
                    }
                });
#endif
            var errnomodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
#if false
            errnomodule.First().PrivatePatch(settings =>
                {
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/errnomodule.c:44:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/errnomodule.c:44:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _sre = builtinModuleSource.AddFiles("$(packagedir)/Modules/_sre.c");
#if false
            _sre.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4918"); // Python-3.5.1\Modules\_sre.c(74): warning C4918: 'a': invalid character in pragma optimization list
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_sre.c(281): warning C4100: 'module': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_sre.c:281:36: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_sre.c:2672:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_sre.c:281:36: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_sre.c:2672:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _codecsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
#if false
            _codecsmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_codecsmodule.c(67): warning C4100: 'module': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_codecsmodule.c:67:31: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/_codecsmodule.c.h:160:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_codecsmodule.c:67:31: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/_codecsmodule.c.h:160:33: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _weakref = builtinModuleSource.AddFiles("$(packagedir)/Modules/_weakref.c");
#if false
            _weakref.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_weakref.c(25): warning C4100: 'module': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_weakref.c:25:44: error: unused parameter 'module' [-Werror=unused-parameter]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_weakref.c:25:44: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                    }
                });
#endif
            var _functoolsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
#if false
            _functoolsmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_functoolsmodule.c(263): warning C4100: 'unused': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_functoolsmodule.c(954) : warning C4706: assignment within conditional expression
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.6.1\modules\_functoolsmodule.c(182) : warning C4701: potentially uninitialized local variable 'nargs' used
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_functoolsmodule.c:263:46: error: unused parameter 'unused' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_functoolsmodule.c:209:5: error: missing initializer for field 'doc' of 'PyGetSetDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_functoolsmodule.c:263:46: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_functoolsmodule.c:205:10: error: missing field 'type' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _operator = builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
#if false
            _operator.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_operator.c(68): warning C4100: 's': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4456"); // Python-3.6.1\Modules\_operator.c(1114): warning C4456: declaration of 'newargs' hides previous local declaration
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_operator.c:40:53: error: unused parameter 's' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_operator.c:398:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_operator.c:68:1: error: unused parameter 's' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_operator.c:398:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _collectionsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
#if false
            _collectionsmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_collectionsmodule.c(166): warning C4100: 'kwds': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_collectionsmodule.c:166:41: error: unused parameter 'args' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_collectionsmodule.c:1476:6: error: missing initializer for field 'closure' of 'PyGetSetDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_collectionsmodule.c:166:41: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_collectionsmodule.c:1476:52: error: missing field 'closure' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var itertoolsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
#if false
            itertoolsmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\itertoolsmodule.c(244): warning C4100: 'kwds': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\itertoolsmodule.c(1895) : warning C4702: unreachable code
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/itertoolsmodule.c:181:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/itertoolsmodule.c:244:28: error: unused parameter 'type' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/itertoolsmodule.c:2281:1: error: string length '731' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/itertoolsmodule.c:244:28: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/itertoolsmodule.c:181:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/itertoolsmodule.c:2282:1: error: string literal of length 731 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                });
#endif
            var atexitmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
#if false
            atexitmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\atexitmodule.c(186): warning C4100: 'unused': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\atexitmodule.c(96) : warning C4701: potentially uninitialized local variable 'exc_value' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\atexitmodule.c(96) : warning C4703: potentially uninitialized local pointer variable 'exc_value' used
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/atexitmodule.c:300:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/atexitmodule.c:186:32: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/atexitmodule.c:186:32: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/atexitmodule.c:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var _stat = builtinModuleSource.AddFiles("$(packagedir)/Modules/_stat.c");
#if false
            _stat.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_stat.c(279): warning C4100: 'self': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_stat.c:268:31: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_stat.c:425:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_stat.c:429:1: error: string length '1479' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_stat.c:277:1: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_stat.c:425:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_stat.c:430:1: error: string literal of length 1479 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                });
#endif
            var timemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
#if false
            timemodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\timemodule.c(38): warning C4100: 'unused': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.6.1\Modules\timemodule.c(402): warning C4244: '=': conversion from 'time_t' to 'int', possible loss of data
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/timemodule.c:38:21: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/timemodule.c:697:1: error: string length '969' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/timemodule.c:1278:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/timemodule.c:38:21: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/timemodule.c:257:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/timemodule.c:698:1: error: string literal of length 969 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.6.1/Modules/timemodule.c:1160:1: error: unused function 'get_zone' [-Werror,-Wunused-function]
                    }
                });
#endif
            var _localemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
#if false
            _localemodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_localemodule.c(90): warning C4100: 'self': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_localemodule.c:90:30: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_localemodule.c:605:3: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_localemodule.c:90:30: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_localemodule.c:605:14: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif

            var _io = this.CreateCSourceContainer("$(packagedir)/Modules/_io/*.c");
            _io.PrivatePatch(this.CoreBuildPatch);
#if true
            _io["_iomodule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        // C:\Program Files (x86)\Windows Kits\10\include\10.0.17134.0\um\winnt.h(154): fatal error C1189: #error:  "No Target Architecture"
                        if (Bam.Core.OSUtilities.Is64Bit(item.BuildEnvironment.Platform))
                        {
                            compiler.PreprocessorDefines.Add("_AMD64_");
                        }
                        else
                        {
                            compiler.PreprocessorDefines.Add("_X86_");
                        }
                    }
                }));
#else
            _io["bufferedio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\bufferedio.c(147): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\_io\bufferedio.c(522): warning C4456: declaration of 'r' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_io\bufferedio.c(546) : warning C4701: potentially uninitialized local variable 'tb' used
                            compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_io\bufferedio.c(546) : warning C4703: potentially uninitialized local pointer variable 'tb' used
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/bufferedio.c:147:43: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/bufferedio.c:153:1: error: string length '599' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/bufferedio.c.h:20:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/bufferedio.c:147:43: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/bufferedio.c.h:20:35: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/bufferedio.c:154:5: error: string literal of length 599 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            _io["bytesio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\bytesio.c(928): warning C4100: 'kwds': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/bytesio.c:928:43: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/bytesio.c.h:277:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/bytesio.c:928:43: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/bytesio.c.h:277:35: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            _io["fileio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\fileio.c(183): warning C4100: 'kwds': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Modules\_io\fileio.c(288): warning C4389: '!=': signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_io\fileio.c(176) : warning C4701: potentially uninitialized local variable 'tb' used
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_io\fileio.c(397) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_io\fileio.c(176) : warning C4703: potentially uninitialized local pointer variable 'tb' used
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/fileio.c:183:42: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/fileio.c.h:155:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/clinic/fileio.c.h:26:1: error: string length '832' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/fileio.c:183:42: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/clinic/fileio.c.h:155:35: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/clinic/fileio.c.h:27:1: error: string literal of length 832 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            _io["iobase.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\iobase.c(108): warning C4100: 'args': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/iobase.c:108:23: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/iobase.c:756:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/iobase.c:41:1: error: string length '1241' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/iobase.c:108:23: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/iobase.c:756:59: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/iobase.c:42:5: error: string literal of length 1241 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            _io["stringio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\stringio.c(659): warning C4100: 'kwds': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/stringio.c:659:44: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/stringio.c:1027:5: error: missing initializer for field 'ml_doc' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/stringio.c:659:44: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/stringio.c:1027:65: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            _io["textio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\stringio.c(659): warning C4100: 'kwds': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\_io\textio.c(456): warning C4456: declaration of 'kind' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_io\textio.c(957): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_io\textio.c(2672) : warning C4701: potentially uninitialized local variable 'tb' used
                            compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_io\textio.c(2672) : warning C4703: potentially uninitialized local pointer variable 'tb' used
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/textio.c:76:29: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/textio.c:162:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/clinic/textio.c.h:112:1: error: string length '1469' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/textio.c:76:29: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/textio.c:162:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/clinic/textio.c.h:113:1: error: string literal of length 1469 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            _io["winconsoleio.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.6.1\Modules\_io\winconsoleio.c(230): warning C4100: 'kwds': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4389"); // Python-3.6.1\Modules\_io\winconsoleio.c(327): warning C4389: '!=': signed/unsigned mismatch
                        compiler.DisableWarnings.AddUnique("4189"); // Python-3.6.1\Modules\_io\winconsoleio.c(277): warning C4189: 'fd_is_own': local variable is initialized but not referenced
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.6.1\modules\_io\winconsoleio.c(223) : warning C4701: potentially uninitialized local variable 'tb' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.6.1\modules\_io\winconsoleio.c(223) : warning C4703: potentially uninitialized local pointer variable 'tb' used
                    }
                }));
            _io["_iomodule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_io\_iomodule.c(230): warning C4100: 'module': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_io\_iomodule.c(482) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/_iomodule.c:230:28: error: unused parameter 'module' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/_iomodule.c:56:1: error: string length '1473' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/_iomodule.c:616:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_io/_iomodule.c:230:28: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_io/_iomodule.c:616:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_io/clinic/_iomodule.c.h:6:1: error: string literal of length 6353 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
#endif

            var zipimport = builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
#if false
            zipimport.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zipimport.c(914): warning C4244: 'function': conversion from 'Py_ssize_t' to 'long', possible loss of data
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\zipimport.c(1355): warning C4100: 'ispackage': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.6.1\Modules\zipimport.c(1001): warning C4127: conditional expression is constant
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/zipimport.c:1355:43: error: unused parameter 'ispackage' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/zipimport.c:743:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/zipimport.c:1445:1: error: string length '591' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/zipimport.c:1355:43: error: unused parameter 'ispackage' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/zipimport.c:743:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/zipimport.c:1446:1: error: string literal of length 591 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                });
#endif
            var faulthandler = builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
#if false
            faulthandler.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\faulthandler.c(251): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\faulthandler.c(934) : warning C4702: unreachable code
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\faulthandler.c(1103) : warning C4706: assignment within conditional expression
                        compiler.DisableWarnings.AddUnique("4459"); // Python-3.6.1\Modules\faulthandler.c(994): warning C4459: declaration of 'thread' hides global declaration
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(18))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\faulthandler.c(412) : warning C4306: 'type cast' : conversion from 'int' to 'void (__cdecl *)(int)' of greater size
                        }
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/faulthandler.c:251:42: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/faulthandler.c:58:1: error: missing initializer for field 'interp' of 'struct <anonymous>' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/faulthandler.c:251:42: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/faulthandler.c:58:32: error: missing field 'interp' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var traceMallocModule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
#if false
            traceMallocModule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4359"); // Python-3.5.1\Modules\_tracemalloc.c(67): warning C4359: '<unnamed-tag>': Alignment specifier is less than actual alignment (8), and will be ignored
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_tracemalloc.c(206): warning C4232: nonstandard extension used: 'malloc': address of dllimport 'malloc' is not static, identity not guaranteed
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_tracemalloc.c(719): warning C4100: 'user_data': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_tracemalloc.c(1407) : warning C4706: assignment within conditional expression
                        compiler.DisableWarnings.AddUnique("4204"); // Python-3.6.1\Modules\_tracemalloc.c(583): warning C4204: nonstandard extension used: non-constant aggregate initializer
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_tracemalloc.c:719:64: error: unused parameter 'user_data' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_tracemalloc.c:1340:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_tracemalloc.c:719:64: error: unused parameter 'user_data' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_tracemalloc.c:1340:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif
            var hashtable = builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c"); // part of _tracemalloc
#if false
            hashtable.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.6.1\Modules\hashtable.c(108): warning C4100: 'ht': unreferenced formal parameter
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/hashtable.c:108:48: error: unused parameter 'ht' [-Werror,-Wunused-parameter]
                        if (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug)
                        {
                            compiler.DisableWarnings.AddUnique("format-pedantic"); // Python-3.6.1/Modules/hashtable.c:243:12: error: format specifies type 'void *' but the argument has type '_Py_hashtable_t *' (aka 'struct _Py_hashtable_t *') [-Werror,-Wformat-pedantic]
                        }
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/hashtable.c:108:48: error: unused parameter 'ht' [-Werror=unused-parameter]
                        if (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug)
                        {
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.6.1/Modules/hashtable.c:243:12: error: format ‘%p’ expects argument of type ‘void *’, but argument 2 has type ‘struct _Py_hashtable_t *’ [-Werror=format=]
                        }
                    }
                });
#endif
            var symtablemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
#if false
            symtablemodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\symtablemodule.c(8): warning C4100: 'self': unreferenced formal parameter
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/symtablemodule.c:8:29: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/symtablemodule.c:48:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/symtablemodule.c:8:29: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/symtablemodule.c:48:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                });
#endif

            // TODO: review
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var _winapi = builtinModuleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
#if false
                _winapi.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4201"); // python-3.5.1\modules\winreparse.h(40): warning C4201: nonstandard extension used: nameless struct/union
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_winapi.c(371): warning C4100: 'module': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4204"); // Python-3.5.1\Modules\_winapi.c(1238): warning C4204: nonstandard extension used: non-constant aggregate initializer
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\_winapi.c(954) : warning C4702: unreachable code
                            if (objectSource.BitDepth == C.EBit.ThirtyTwo)
                            {
                                compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Modules\_winapi.c(231): warning C4389: '!=': signed/unsigned mismatch
                            }
                        }
                    });
#endif
                var msvcrtmodule = builtinModuleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
#if false
                msvcrtmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4312"); // Python-3.5.1\PC\msvcrtmodule.c(391): warning C4312: 'type cast': conversion from 'int' to '_HFILE' of greater size
                            compiler.DisableWarnings.AddUnique("4311"); // Python-3.5.1\PC\msvcrtmodule.c(391): warning C4311: 'type cast': pointer truncation from '_HFILE' to 'long'
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\msvcrtmodule.c(81): warning C4100: 'module': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\PC\msvcrtmodule.c(329): warning C4244: 'function': conversion from 'int' to 'wchar_t', possible loss of data
                            if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebug ||
                                vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL)
                            {
                                compiler.DisableWarnings.AddUnique("4310"); // Python-3.5.1\PC\msvcrtmodule.c(538): warning C4310: cast truncates constant value
                            }
                        }
                    });
#endif
            }
            else
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/getpath.c");

                var ModuleConfigSourceFile = Bam.Core.Graph.Instance.FindReferencedModule<ModuleConfigSourceFile>();
                builtinModuleSource.AddFile(ModuleConfigSourceFile);

#if false
                builtinModuleSource["getpath.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            // TODO: these should be configurables
                            compiler.PreprocessorDefines.Add("PREFIX", "\".\"");
                            compiler.PreprocessorDefines.Add("EXEC_PREFIX", "\".\"");
                            compiler.PreprocessorDefines.Add("PYTHONPATH", "\".:./lib-dynload\""); // TODO: this was in pyconfig.h for PC, so does it need moving?
                            compiler.PreprocessorDefines.Add("VERSION", System.String.Format("\"{0}\"", Version.MajorDotMinor));
                            compiler.PreprocessorDefines.Add("VPATH", "\".\"");
                        }));
#endif
            }

            if (builtinModuleSource.Compiler is VisualCCommon.CompilerBase)
            {
                builtinModuleSource.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryBuiltinModules());
                cjkcodecs.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryCJKCodecs());
                _io.SuppressWarningsDelegate(new VisualC.WarningSuppression.PythonLibraryIO());
            }
            else if (builtinModuleSource.Compiler is GccCommon.CompilerBase)
            {
                builtinModuleSource.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryBuiltinModules());
                cjkcodecs.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryCJKCodecs());
                _io.SuppressWarningsDelegate(new Gcc.WarningSuppression.PythonLibraryIO());
            }
            else if (pythonSource.Compiler is ClangCommon.CompilerBase)
            {
                builtinModuleSource.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryBuiltinModules());
                cjkcodecs.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryCJKCodecs());
                _io.SuppressWarningsDelegate(new Clang.WarningSuppression.PythonLibraryIO());
            }

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

            // although PyConfigHeader is only explicitly used on non-Windows platforms in the main library, it's needed
            // for the closing patch on Windows

            // TODO: is there a call for a CompileWith function?
            var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
            this.UsePublicPatches(pyConfigHeader);
            parserSource.DependsOn(pyConfigHeader);
            objectSource.DependsOn(pyConfigHeader);
            pythonSource.DependsOn(pyConfigHeader);
            builtinModuleSource.DependsOn(pyConfigHeader);
            cjkcodecs.DependsOn(pyConfigHeader);
            _io.DependsOn(pyConfigHeader);
            // TODO: end of function

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceContainer("$(packagedir)/PC/dl_nt.c");
                pcSource.PrivatePatch(this.CoreBuildPatch);
                pcSource["dl_nt.c"].ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\dl_nt.c(90): warning C4100: 'lpReserved': unreferenced formal parameter
                            }
                        }));
                var pcConfig = pcSource.AddFiles("$(packagedir)/PC/config.c");
                pcConfig.First().PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("WIN32"); // required to register two extension modules
                    });
                //pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                var getpathp = pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                getpathp.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\PC\getpathp.c(144): warning C4267: '=': conversion from 'size_t' to 'int', possible loss of data
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\PC\getpathp.c(289): warning C4456: declaration of 'keyBuf' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\PC\getpathp.c(324): warning C4189: 'reqdSize': local variable is initialized but not referenced
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\pc\getpathp.c(548) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4459"); // Python-3.6.1\PC\getpathp.c(541): warning C4459: declaration of 'prefix' hides global declaration
                        }
                    });
                var winreg = pcSource.AddFiles("$(packagedir)/PC/winreg.c");
                winreg.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
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
                                (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                (settings.Module as C.ObjectFile).Compiler;
                            if (compilerUsed.IsAtLeast(18))
                            {
                            }
                            else
                            {
                                compiler.DisableWarnings.AddUnique("4305"); // Python-3.5.1\PC\winreg.c(885) : warning C4305: 'type cast' : truncation from 'void *' to 'DWORD'
                            }
                        }
                    });
                var invalid_parameter_handle = pcSource.AddFiles("$(packagedir)/PC/invalid_parameter_handler.c"); // required by VS2015+
                invalid_parameter_handle.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\invalid_parameter_handler.c(16): warning C4100: 'pReserved': unreferenced formal parameter
                        }
                    });
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("Advapi32.lib");
                        linker.Libraries.Add("Ws2_32.lib");
                        linker.Libraries.Add("User32.lib");
                        linker.Libraries.Add("Shlwapi.lib");
                        linker.Libraries.Add("version.lib");
                    });
                headers.AddFiles("$(packagedir)/PC/*.h");

                parserSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                objectSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                pythonSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                builtinModuleSource.ClosingPatch(VCNotPyDEBUGClosingPatch);
                cjkcodecs.ClosingPatch(VCNotPyDEBUGClosingPatch);
                _io.ClosingPatch(VCNotPyDEBUGClosingPatch);
                pcSource.ClosingPatch(VCNotPyDEBUGClosingPatch);

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

                    this.PrivatePatch(settings =>
                        {
                            var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                            vcLinker.GenerateManifest = false; // as the .rc file refers to this already
                        });
                }
            }
            else
            {
                if (!(pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    parserSource.PrivatePatch(NotPyDEBUGPatch);
                    objectSource.PrivatePatch(NotPyDEBUGPatch);
                    pythonSource.PrivatePatch(NotPyDEBUGPatch);
                    builtinModuleSource.PrivatePatch(NotPyDEBUGPatch);
                    cjkcodecs.PrivatePatch(NotPyDEBUGPatch);
                    _io.PrivatePatch(NotPyDEBUGPatch);
                }

                var sysConfigDataPy = Bam.Core.Graph.Instance.FindReferencedModule<SysConfigDataPythonFile>();
                this.Requires(sysConfigDataPy);

                var pyMakeFile = Bam.Core.Graph.Instance.FindReferencedModule<PyMakeFile>();
                this.Requires(pyMakeFile);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-lpthread");
                        linker.Libraries.Add("-lm");
                        linker.Libraries.Add("-ldl");
                    });

                headers.AddFile(pyConfigHeader);
            }
        }
    }
}
