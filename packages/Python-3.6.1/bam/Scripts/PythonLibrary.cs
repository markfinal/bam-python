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
                if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreaded ||
                    vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDLL)
                {
                    NotPyDEBUGPatch(settings);
                }
                else
                {
                    this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python36_d");
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
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python36");
            }
            else
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");
            }
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("3");
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("6");
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

            var parserSource = this.CreateCSourceContainer("$(packagedir)/Parser/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$"));
            parserSource.PrivatePatch(this.CoreBuildPatch);
            headers.AddFiles("$(packagedir)/Parser/*.h");

            parserSource["grammar.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Parser\grammar.c(83): warning C4244: '=': conversion from 'int' to 'short', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Parser/grammar.c:107:16: error: format '%p' expects argument of type 'void *', but argument 2 has type 'struct labellist *' [-Werror=format=]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("format-pedantic"); // Python-3.5.1/Parser/grammar.c:106:41: error: format specifies type 'void *' but the argument has type 'labellist *' [-Werror,-Wformat-pedantic]
                        }
                    }));

            parserSource["metagrammar.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Parser/metagrammar.c:15:5: error: missing initializer for field 's_lower' of 'state' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Parser/metagrammar.c:15:17: error: missing field 's_lower' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            parserSource["node.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Parser\node.c(13): warning C4244: '=': conversion from 'int' to 'short', possible loss of data
                        }
                    }));

            parserSource["tokenizer.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Parser\tokenizer.c(217): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Parser\tokenizer.c(351): warning C4100: 'set_readline': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\parser\tokenizer.c(623) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Parser/tokenizer.c:351:15: error: unused parameter 'set_readline' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Parser/tokenizer.c:351:15: error: unused parameter 'set_readline' [-Werror,-Wunused-parameter]
                        }
                    }));

            var objectSource = this.CreateCSourceContainer("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(this.CoreBuildPatch);

            objectSource["abstract.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\abstract.c(806): warning C4100: 'op_name': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\abstract.c(1231) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/abstract.c:806:24: error: unused parameter 'op_name' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/abstract.c:806:24: error: unused parameter 'op_name' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["boolobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\boolobject.c(43): warning C4100: 'type': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/boolobject.c:43:24: error: unused parameter 'type' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/boolobject.c:172:1: error: missing initializer for field 'tp_free' of 'PyTypeObject' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/boolobject.c:43:24: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/boolobject.c:129:1: error: missing field 'nb_matrix_multiply' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            objectSource["bytearrayobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\bytearrayobject.c(78): warning C4100: 'view': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\bytearrayobject.c(620): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4127"); // python-3.5.1\objects\stringlib/fastsearch.h(49): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\bytearrayobject.c(1714) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/bytearrayobject.c:78:60: error: unused parameter 'view' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/clinic/bytearrayobject.c.h:107:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/clinic/bytearrayobject.c.h:524:1: error: string length '512' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/bytearrayobject.c:78:60: error: unused parameter 'view' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/clinic/bytearrayobject.c.h:107:32: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/clinic/bytearrayobject.c.h:525:1: error: string literal of length 512 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            objectSource["bytesobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.5.1\objects\clinic/bytesobject.c.h(323): warning C4100: 'null': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\bytesobject.c(429): warning C4244: 'function': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4127"); // python-3.5.1\objects\stringlib/fastsearch.h(49): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\bytesobject.c(2339) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/clinic/bytesobject.c.h:65:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/clinic/bytesobject.c.h:323:23: error: unused parameter 'null' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/clinic/bytesobject.c.h:65:32: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/clinic/bytesobject.c.h:323:23: error: unused parameter 'null' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["bytes_methods.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\bytes_methods.c(297): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                        }
                    }));
            objectSource["capsule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/capsule.c:322:1: error: missing initializer for field 'tp_traverse' of 'PyTypeObject' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/capsule.c:322:1: error: missing field 'tp_traverse' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            objectSource["cellobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\cellobject.c(132): warning C4100: 'closure': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/cellobject.c:144:5: error: missing initializer for field 'doc' of 'PyGetSetDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/cellobject.c:132:43: error: unused parameter 'closure' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/cellobject.c:144:54: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/cellobject.c:132:43: error: unused parameter 'closure' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["classobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\classobject.c(117): warning C4100: 'context': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\codeobject.c(111): warning C4244: '=': conversion from 'Py_ssize_t' to 'unsigned char', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/classobject.c:94:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/classobject.c:117:42: error: unused parameter 'context' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/classobject.c:94:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/classobject.c:117:42: error: unused parameter 'context' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["codeobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\codeobject.c(277): warning C4100: 'kw': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\codeobject.c(111): warning C4244: '=': conversion from 'Py_ssize_t' to 'unsigned char', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/codeobject.c:209:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/codeobject.c:277:24: error: unused parameter 'type' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/codeobject.c:209:73: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/codeobject.c:277:24: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["complexobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\complexobject.c(498): warning C4100: 'w': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\objects\complexobject.c(1030) : warning C4701: potentially uninitialized local variable 'ci' used
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/complexobject.c:747:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/complexobject.c:498:29: error: unused parameter 'v' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/complexobject.c:747:84: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/complexobject.c:498:29: error: unused parameter 'v' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("extended-offsetof"); // Python-3.5.1/Objects/complexobject.c:754:24: error: using extended field designator is an extension [-Werror,-Wextended-offsetof]
                        }
                    }));
            objectSource["descrobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\descrobject.c(122): warning C4100: 'type': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\descrobject.c(1391) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/descrobject.c:418:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/descrobject.c:122:65: error: unused parameter 'type' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/descrobject.c:1575:1: error: string length '759' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/descrobject.c:418:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/descrobject.c:122:65: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/descrobject.c:1576:1: error: string literal of length 759 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            objectSource["dictobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\dictobject.c(1513): warning C4456: declaration of 'key' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\dictobject.c(2698): warning C4100: 'kwds': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\objects\dictobject.c(680) : warning C4702: unreachable code
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\dictobject.c(3835) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/dictobject.c:2649:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/dictobject.c:2698:40: error: unused parameter 'args' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/dictobject.c:2649:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/dictobject.c:2698:40: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["enumobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/enumobject.c:174:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/enumobject.c:174:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            objectSource["exceptions.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\exceptions.c(32): warning C4100: 'kwds': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/exceptions.c:180:4: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/exceptions.c:32:65: error: unused parameter 'kwds' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/exceptions.c:180:66: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/exceptions.c:32:65: error: unused parameter 'kwds' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["fileobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\fileobject.c(29): warning C4100: 'name': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\fileobject.c(297): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/fileobject.c:462:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/fileobject.c:29:35: error: unused parameter 'name' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/fileobject.c:462:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/fileobject.c:29:35: error: unused parameter 'name' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["floatobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\floatobject.c(1447): warning C4100: 'unused': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Objects\floatobject.c(2032): warning C4244: '=': conversion from 'int' to 'unsigned char', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/floatobject.c:134:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/floatobject.c:1447:47: error: unused parameter 'unused' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Objects/floatobject.c:970:5: error: implicit declaration of function 'round' [-Werror=implicit-function-declaration]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/floatobject.c:62:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/floatobject.c:1447:47: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["frameobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\frameobject.c(22): warning C4100: 'closure': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/frameobject.c:13:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/frameobject.c:22:41: error: unused parameter 'closure' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/frameobject.c:13:67: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/frameobject.c:22:41: error: unused parameter 'closure' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["funcobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\funcobject.c(467): warning C4100: 'type': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\funcobject.c(635): warning C4244: 'function': conversion from 'Py_ssize_t' to 'int', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/funcobject.c:238:6: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/funcobject.c:467:24: error: unused parameter 'type' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/funcobject.c:806:1: error: string length '665' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/funcobject.c:238:25: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/funcobject.c:467:24: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/funcobject.c:807:1: error: string literal of length 665 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            objectSource["genobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Objects\genobject.c(157): warning C4457: declaration of 'exc' hides function parameter
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\genobject.c(281): warning C4100: 'args': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\genobject.c(364): warning C4456: declaration of 'val' hides previous local declaration
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/genobject.c:566:6: error: missing initializer for field 'closure' of 'PyGetSetDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/genobject.c:281:39: error: unused parameter 'args' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/genobject.c:566:40: error: missing field 'closure' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/genobject.c:281:39: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["iterobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/iterobject.c:133:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/iterobject.c:133:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            objectSource["listobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\listobject.c(2881): warning C4100: 'unused': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/listobject.c:2383:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/listobject.c:2881:44: error: unused parameter 'unused' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/listobject.c:2383:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/listobject.c:2881:44: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["longobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\longobject.c(4638): warning C4100: 'context': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\longobject.c(2166): warning C4456: declaration of 'i' hides previous local declaration

                            if (objectSource.BitDepth == C.EBit.ThirtyTwo)
                            {
                                compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\longobject.c(2666): warning C4244: '+=': conversion from 'const int' to 'digit', possible loss of data
                            }
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/longobject.c:4638:25: error: unused parameter 'v' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/longobject.c:4979:1: error: string length '792' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/longobject.c:5116:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/longobject.c:5116:76: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/longobject.c:4638:25: error: unused parameter 'v' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/longobject.c:4980:1: error: string literal of length 792 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));
            objectSource["memoryobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\memoryobject.c(947): warning C4100: 'subtype': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\memoryobject.c(2519): warning C4456: declaration of 'ptr' hides previous local declaration
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/memoryobject.c:947:26: error: unused parameter 'subtype' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/memoryobject.c:159:1: error: missing initializer for field 'tp_richcompare' of 'PyTypeObject' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/memoryobject.c:159:1: error: missing field 'tp_richcompare' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/memoryobject.c:947:26: error: unused parameter 'subtype' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["methodobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\methodobject.c(190): warning C4100: 'closure': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Objects\methodobject.c(327): warning C4054: 'type cast': from function pointer 'PyCFunction' to data pointer 'void *'
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/methodobject.c:190:56: error: unused parameter 'closure' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/methodobject.c:186:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            gccCompiler.Pedantic = false; // Python-3.5.1/Objects/methodobject.c:327:25: error: ISO C forbids conversion of function pointer to object pointer type [-Werror=pedantic]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/methodobject.c:186:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/methodobject.c:190:56: error: unused parameter 'closure' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["moduleobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Objects\moduleobject.c(242): warning C4152: nonstandard extension, function/data pointer conversion in expression
                            compiler.DisableWarnings.AddUnique("4055"); // Python-3.5.1\Objects\moduleobject.c(359): warning C4055: 'type cast': from data pointer 'void *' to function pointer 'int (__cdecl *)(PyObject *)'
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\moduleobject.c(705): warning C4100: 'args': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/moduleobject.c:705:38: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/moduleobject.c:19:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                            gccCompiler.Pedantic = false; // Python-3.5.1/Objects/moduleobject.c:242:20: error: ISO C forbids assignment between function pointer and 'void *' [-Werror=pedantic]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/moduleobject.c:19:71: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/moduleobject.c:705:38: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                            clangCompiler.Pedantic = false; // Python-3.5.1/Objects/moduleobject.c:242:20: error: assigning to 'PyObject *(*)(PyObject *, PyModuleDef *)' (aka 'struct _object *(*)(struct _object *, struct PyModuleDef *)') from 'void *' converts between void pointer and function pointer [-Werror,-Wpedantic]
                        }
                    }));
            objectSource["namespaceobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\namespaceobject.c(22): warning C4100: 'kwds': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/namespaceobject.c:22:45: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/namespaceobject.c:14:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/namespaceobject.c:14:75: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/namespaceobject.c:22:45: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["object.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // c:\dev\bam-python\packages\Python-3.5.1\Objects\object.c(1205): warning C4100: 'context': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Objects/object.c:370:17: error: format '%p' expects argument of type 'void *', but argument 4 has type 'struct PyObject *' [-Werror=format=]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/object.c:1205:63: error: unused parameter 'context' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/object.c:1426:1: error: missing initializer for field 'nb_matrix_multiply' of 'PyNumberMethods' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/object.c:1426:1: error: missing field 'nb_matrix_multiply' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/object.c:1205:63: error: unused parameter 'context' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("format-pedantic"); // Python-3.5.1/Objects/object.c:370:38: error: format specifies type 'void *' but the argument has type 'PyObject *' (aka 'struct _object *') [-Werror,-Wformat-pedantic]
                        }
                    }));
            objectSource["obmalloc.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\obmalloc.c(54): warning C4100: 'ctx': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/obmalloc.c:54:24: error: unused parameter 'ctx' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Objects/obmalloc.c:2155:19: error: ISO C90 does not support the 'z' gnu_printf length modifier [-Werror=format=]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Objects/obmalloc.c:54:24: error: unused parameter 'ctx' [-Werror,-Wunused-parameter]
                        }
                    }));
            objectSource["odictobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Pedantic = false; // Python-3.5.1/Objects/odictobject.c:1492:2: error: ISO C does not allow extra ';' outside of a function [-Werror=pedantic]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("extra-semi"); // Python-3.5.1/Objects/odictobject.c:1492:2: error: extra ';' outside of a function [-Werror,-Wextra-semi]
                        }
                    }));
            objectSource["structseq.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\structseq.c(144) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/structseq.c:279:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Objects/structseq.c:279:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));
            objectSource["tupleobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4245"); // Python-3.5.1\Objects\tupleobject.c(360): warning C4245: '=': conversion from 'int' to 'std::Py_uhash_t', signed/unsigned mismatch
                        }
                    }));
            objectSource["typeobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\typeobject.c(2398): warning C4456: declaration of 'tmp' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4055"); // Python-3.5.1\Objects\typeobject.c(5105): warning C4055: 'type cast': from data pointer 'void *' to function pointer 'lenfunc'
                            compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Objects\typeobject.c(6128): warning C4054: 'type cast': from function pointer 'PyObject *(__cdecl *)(PyObject*,PyObject *)' to data pointer 'void *'
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Pedantic = false; // Python-3.5.1/Objects/typeobject.c:5105:20: error: ISO C forbids conversion of object pointer to function pointer type [-Werror=pedantic]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("extended-offsetof"); // Python-3.5.1/Objects/typeslots.inc:4:1: error: using extended field designator is an extension [-Werror,-Wextended-offsetof]
                        }
                    }));
            objectSource["unicodeobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4127"); // python-3.5.1\objects\stringlib/fastsearch.h(49): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\Objects\unicodeobject.c(7116): warning C4189: 'exc': local variable is initialized but not referenced
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Objects\unicodeobject.c(7357): warning C4456: declaration of 'ch' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Objects\unicodeobject.c(8490): warning C4457: declaration of 'ch' hides function parameter
                            compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Objects\unicodeobject.c(10744): warning C4389: '!=': signed/unsigned mismatch
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\objects\unicodeobject.c(4428) : warning C4701: potentially uninitialized local variable 'startinpos' used
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\unicodeobject.c(10818) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/clinic/unicodeobject.c.h:5:1: error: string length '569' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Objects/stringlib/asciilib.h:7:34: error: 'asciilib_parse_args_finds_unicode' defined but not used [-Werror=unused-function]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Objects/clinic/unicodeobject.c.h:6:1: error: string literal of length 569 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                            compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Objects/stringlib/find.h:134:1: error: unused function 'asciilib_parse_args_finds_unicode' [-Werror,-Wunused-function]
                        }
                    }));
            headers.AddFiles("$(packagedir)/Objects/*.h");

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

            pythonSource["ast.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\ast.c(1034): warning C4100: 'c': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Python\ast.c(3970): warning C4457: declaration of 'len' hides function parameter
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\python\ast.c(2996) : warning C4702: unreachable code
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/Python-ast.c:560:43: error: unused parameter 'unused' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/Python-ast.c:585:5: error: missing initializer for field 'doc' of 'PyGetSetDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/ast.c:1034:37: error: unused parameter 'c' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["_warnings.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\_warnings.c(759): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\_warnings.c(131): warning C4456: declaration of 'action' hides previous local declaration
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/_warnings.c:759:25: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/_warnings.c:1034:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/_warnings.c:759:25: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/_warnings.c:1034:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["bltinmodule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\bltinmodule.c(53): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\bltinmodule.c(1718) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/clinic/bltinmodule.c.h:131:1: error: string length '801' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/bltinmodule.c:53:35: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/bltinmodule.c:510:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/bltinmodule.c:53:35: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/clinic/bltinmodule.c.h:132:1: error: string literal of length 801 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/bltinmodule.c:510:26: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["ceval.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4918"); // python-3.5.1\include\pyport.h(286): warning C4918: 'a': invalid character in pragma optimization list
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\ceval.c(217): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\ceval.c(3269): warning C4456: declaration of 'names' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Python\ceval.c(3879): warning C4457: declaration of 'name' hides function parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\ceval.c(4762): warning C4244: '=': conversion from 'Py_ssize_t' to 'int', possible loss of data
                            compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\python\ceval.c(2616) : warning C4701: potentially uninitialized local variable 'function_location' used
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/ceval.c:217:31: error: unused parameter 'self' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/ceval.c:217:31: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["codecs.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4310"); // Python-3.5.1\Python\codecs.c(750): warning C4310: cast truncates constant value
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\codecs.c(841): warning C4244: '=': conversion from 'Py_UCS4' to 'unsigned char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\codecs.c(888): warning C4456: declaration of 'c' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\codecs.c(1379): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\codecs.c(875) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/codecs.c:1379:42: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/codecs.c:1499:13: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/codecs.c:1379:42: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/codecs.c:1499:13: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["compile.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Python\compile.c(553): warning C4457: declaration of 'name' hides function parameter
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\compile.c(793): warning C4100: 'c': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\compile.c(1090): warning C4244: '=': conversion from 'int' to 'unsigned char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4310"); // Python-3.5.1\Python\compile.c(4483): warning C4310: cast truncates constant value
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\python\compile.c(1072) : warning C4702: unreachable code
                            compiler.DisableWarnings.AddUnique("4312"); // Python-3.5.1\Python\compile.c(480): warning C4312: 'type cast': conversion from 'unsigned int' to 'void *' of greater size
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/compile.c:793:38: error: unused parameter 'c' [-Werror=unused-parameter]
                            gccCompiler.Pedantic = false; // Python-3.5.1/Python/compile.c:97:33: error: comma at end of enumerator list [-Werror=pedantic]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/compile.c:793:38: error: unused parameter 'c' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["dtoa.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\dtoa.c(2639): warning C4244: '=': conversion from 'Long' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\dtoa.c(2466) : warning C4706: assignment within conditional expression
                        }

                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("parentheses"); // if (y = value) type expression
                        }
                    }));

            pythonSource["errors.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\errors.c(104) : warning C4706: assignment within conditional expression
                        }
                    }));

            pythonSource["fileutils.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\fileutils.c(481): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\fileutils.c(1206) : warning C4706: assignment within conditional expression
                        }
                    }));

            pythonSource["formatter_unicode.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\formatter_unicode.c(426): warning C4100: 'number': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/formatter_unicode.c:426:49: error: unused parameter 'number' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/formatter_unicode.c:426:49: error: unused parameter 'number' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["getargs.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\getargs.c(146): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Python\getargs.c(918): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\getargs.c(1219) : warning C4706: assignment within conditional expression
                        }

                        // TODO: I cannot see how else some symbols are exported with preprocessor settings
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Visibility = GccCommon.EVisibility.Default;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/getargs.c:146:23: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Python/getargs.c:380:27: error: ISO C90 does not support the 'z' gnu_printf length modifier [-Werror=format=]
                        }

                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            clangCompiler.Visibility = ClangCommon.EVisibility.Default;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/getargs.c:146:23: error: unused parameter 'self' [-Werror,-Wunused-parameter]
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

            pythonSource["graminit.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/graminit.c:18:5: error: missing initializer for field 's_lower' of 'state' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/graminit.c:18:17: error: missing field 's_lower' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["import.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\import.c(245): warning C4100: 'module': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\import.c(1833) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/import.c:245:34: error: unused parameter 'module' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/import.c:2091:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/import.c:245:34: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/import.c:2091:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["marshal.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\marshal.c(585): warning C4100: '_unused_data': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\marshal.c(380): warning C4244: '=': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\marshal.c(1311): warning C4456: declaration of 'code' hides previous local declaration
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/marshal.c:892:24: error: unused parameter 'flag' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/marshal.c:1639:1: error: string length '524' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/marshal.c:1765:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/marshal.c:892:24: error: unused parameter 'flag' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/marshal.c:1640:1: error: string literal of length 524 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/marshal.c:1765:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["peephole.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\peephole.c(354): warning C4100: 'names': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Python\peephole.c(482): warning C4244: '=': conversion from 'Py_ssize_t' to 'int', possible loss of data
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/peephole.c:354:61: error: unused parameter 'names' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/peephole.c:354:61: error: unused parameter 'names' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["pyfpe.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\pyfpe.c(20): warning C4100: 'dummy': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/pyfpe.c:20:19: error: unused parameter 'dummy' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/pyfpe.c:20:19: error: unused parameter 'dummy' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["pyhash.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("sign-compare"); // Python-3.5.1/Python/pyhash.c:275:11: error: comparison between signed and unsigned integer expressions [-Werror=sign-compare]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("sign-compare"); // Python-3.5.1/Python/pyhash.c:275:11: error: comparison of integers of different signs: 'Py_uhash_t' (aka 'unsigned long') and 'int' [-Werror,-Wsign-compare]
                        }
                    }));

            pythonSource["pylifecycle.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4210"); // Python-3.5.1\Python\pylifecycle.c(291): warning C4210: nonstandard extension used: function given file scope
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\pylifecycle.c(907): warning C4456: declaration of 'loader' hides previous local declaration
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\pylifecycle.c(305) : warning C4706: assignment within conditional expression
                            var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                (settings.Module as C.ObjectFile).Compiler;
                            if (compilerUsed.IsAtLeast(18))
                            {
                            }
                            else
                            {
                                compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Python\pylifecycle.c(1540) : warning C4306: 'type cast' : conversion from 'int' to 'void (__cdecl *)(int)' of greater size
                            }
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Python/pylifecycle.c:181:1: error: unused function 'get_codec_name' [-Werror,-Wunused-function]
                        }
                    }));

            pythonSource["pystate.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\pystate.c(66) : warning C4706: assignment within conditional expression
                        }
                    }));

            pythonSource["Python-ast.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\Python-ast.c(4338): warning C4456: declaration of 'value' hides previous local declaration
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/Python-ast.c:560:43: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/Python-ast.c:581:10: error: missing field 'ml_meth' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["pythonrun.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\pythonrun.c(266): warning C4100: 'filename': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/pythonrun.c:266:38: error: unused parameter 'filename' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/pythonrun.c:266:38: error: unused parameter 'filename' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["pytime.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\pytime.c(452): warning C4100: 'raise': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Python\pytime.c(572): warning C4389: '!=': signed/unsigned mismatch
                        }

                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("tautological-constant-out-of-range-compare"); // numbers out of range of comparison
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/pytime.c:562:56: error: unused parameter 'raise' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["random.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/random.c:193:1: error: missing initializer for field 'st_dev' of 'struct <anonymous>' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/random.c:193:24: error: missing field 'st_dev' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["symtable.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\symtable.c(931): warning C4100: 'ast': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Python\symtable.c(1705): warning C4457: declaration of 'elt' hides function parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\symtable.c(623) : warning C4706: assignment within conditional expression
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/symtable.c:110:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/symtable.c:931:48: error: unused parameter 'ast' [-Werror=unused-parameter]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/symtable.c:110:49: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/symtable.c:931:48: error: unused parameter 'ast' [-Werror,-Wunused-parameter]
                        }
                    }));

            pythonSource["sysmodule.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\sysmodule.c(163): warning C4100: 'self': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/sysmodule.c:163:27: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/sysmodule.c:1124:1: error: string length '742' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/sysmodule.c:1287:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/sysmodule.c:163:27: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Python/sysmodule.c:1125:1: error: string literal of length 742 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/sysmodule.c:1287:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["thread.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.5.1\python\thread_nt.h(296): warning C4100: 'intr_flag': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4189"); // python-3.5.1\python\thread_nt.h(218): warning C4189: 'e': local variable is initialized but not referenced
                            if (vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebug ||
                                vcCompiler.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL)
                            {
                                compiler.DisableWarnings.AddUnique("4389"); // python-3.5.1\python\thread_nt.h(203): warning C4389: '==': signed/unsigned mismatch
                            }
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/thread.c:375:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                        }
                    }));

            pythonSource["traceback.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\traceback.c(30): warning C4100: 'self': unreferenced formal parameter
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/traceback.c:30:27: error: unused parameter 'self' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/traceback.c:37:4: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        }
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Python/traceback.c:37:48: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Python/traceback.c:30:27: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        }
                    }));

            headers.AddFiles("$(packagedir)/Python/*.h");

            var builtinModuleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            builtinModuleSource.PrivatePatch(this.CoreBuildPatch);
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
            builtinModuleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            // Windows builds includes dynamic modules builtin the core library
            // see PC/config.c
            var cjkcodecs = this.CreateCSourceContainer(); // empty initially, as only Windows populates it as static modules
            cjkcodecs.PrivatePatch(this.CoreBuildPatch);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var opcode = builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
                opcode.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_opcode.c(23): warning C4100: 'module': unreferenced formal parameter
                        }
                    });
                var lsprof = builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
                lsprof.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_lsprof.c(264): warning C4100: 'pObj': unreferenced formal parameter
                        }
                    });
                builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c"); // part of _lsprof
                var json = builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
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
                var threadmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
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
                var arraymodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
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
                var cmathmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
                cmathmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\cmathmodule.c(436): warning C4100: 'module': unreferenced formal parameter
                        }
                    });
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c"); // part of cmath
                var mathmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
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
                var _struct = builtinModuleSource.AddFiles("$(packagedir)/Modules/_struct.c");
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
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
                var _pickle = builtinModuleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
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
                var _datetimemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
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
                var _bisectmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
                _bisectmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_bisectmodule.c(47): warning C4100: 'self': unreferenced formal parameter
                        }
                    });
                var _heapqmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
                _heapqmodule.First().PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_heapqmodule.c(100): warning C4100: 'self': unreferenced formal parameter
                        }
                    });
                var mmapmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
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
                var _csv = builtinModuleSource.AddFiles("$(packagedir)/Modules/_csv.c");
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
                var audioop = builtinModuleSource.AddFiles("$(packagedir)/Modules/audioop.c");
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
                var md5module = builtinModuleSource.AddFiles("$(packagedir)/Modules/md5module.c");
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
                var sha1module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
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
                var sha256module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
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
                var sha512module = builtinModuleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
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
                var binascii = builtinModuleSource.AddFiles("$(packagedir)/Modules/binascii.c");
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
                var parsermodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
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

#if PYTHON_USE_ZLIB_PACKAGE
#else
                var zlib = this.CreateCSourceContainer("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));
                zlib.PrivatePatch(this.WinNotUnicodePatch);
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

                var zlibmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
                zlibmodule.First().PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
#if !PYTHON_USE_ZLIB_PACKAGE
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
#endif
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\zlibmodule.c(122): warning C4100: 'ctx': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\zlibmodule.c(308) : warning C4706: assignment within conditional expression
                        }
                    });

#if PYTHON_USE_ZLIB_PACKAGE
                this.CompileAndLinkAgainst<global::zlib.ZLib>(zlibmodule.First() as C.CModule);
#endif

                cjkcodecs.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c"); // _multibytecodec, _codecs_cn, _codecs_hk, _codecs_iso2022, _codecs_jp, _codecs_kr, _codecs_tw
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

                var xxsubtype = builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");
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
            }
            else
            {
                // TODO: this should be following the rules in Modules/makesetup and Modules/Setup.dist
                // for which modules are static (and thus part of the Python library) and which are shared
                // and separate in the distribution
                // note that you need to read Setup.dist backward, as some modules are mentioned twice
                // and it is the 'topmost' that overrules
                var pwdmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/pwdmodule.c");
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
            }

            // common statically compiled extension modules
            var signalmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
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
            var gcmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            gcmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\gcmodule.c(370): warning C4100: 'data': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\gcmodule.c(1633) : warning C4706: assignment within conditional expression
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
            var posixmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c"); // implements nt module on Windows
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
            var errnomodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
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
            var _sre = builtinModuleSource.AddFiles("$(packagedir)/Modules/_sre.c");
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
            var _codecsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
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
            var _weakref = builtinModuleSource.AddFiles("$(packagedir)/Modules/_weakref.c");
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
            var _functoolsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            _functoolsmodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_functoolsmodule.c(263): warning C4100: 'unused': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_functoolsmodule.c(954) : warning C4706: assignment within conditional expression
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
            var _operator = builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
            _operator.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_operator.c(68): warning C4100: 's': unreferenced formal parameter
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
            var _collectionsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
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
            var itertoolsmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
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
            var atexitmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
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
            var _stat = builtinModuleSource.AddFiles("$(packagedir)/Modules/_stat.c");
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
            var timemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
            timemodule.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\timemodule.c(38): warning C4100: 'unused': unreferenced formal parameter
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
                    }
                });
            var _localemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
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

            var _io = this.CreateCSourceContainer("$(packagedir)/Modules/_io/*.c");
            _io.PrivatePatch(this.CoreBuildPatch);
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

            var zipimport = builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            zipimport.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\zipimport.c(914): warning C4244: 'function': conversion from 'Py_ssize_t' to 'long', possible loss of data
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\zipimport.c(1355): warning C4100: 'ispackage': unreferenced formal parameter
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
            var faulthandler = builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            faulthandler.First().PrivatePatch(settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\faulthandler.c(251): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\faulthandler.c(934) : warning C4702: unreachable code
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\faulthandler.c(1103) : warning C4706: assignment within conditional expression
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
            var traceMallocModule = builtinModuleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
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
            builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c"); // part of _tracemalloc
            var symtablemodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
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

            // TODO: review
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var _winapi = builtinModuleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
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
                var msvcrtmodule = builtinModuleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
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

            headers.AddFiles("$(packagedir)/Modules/*.h");
            headers.AddFiles("$(packagedir)/Modules/cjkcodecs/*.h");
#if !PYTHON_USE_ZLIB_PACKAGE
            headers.AddFiles("$(packagedir)/Modules/zlib/*.h");
#endif
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
                cjkcodecs.DependsOn(pyConfigHeader);
                _io.DependsOn(pyConfigHeader);
                // TODO: end of function

#if BAM_FEATURE_MODULE_CONFIGURATION
                if (!(pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
#else
                if (!pyConfigHeader.PyDEBUG)
#endif
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
