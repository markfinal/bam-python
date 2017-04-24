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
                visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
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

                            var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                            if (null != vcCompiler)
                            {
                                compiler.DisableWarnings.AddUnique("4115"); // python-3.5.1\include\pytime.h(112): warning C4115: 'timeval': named type definition in parentheses
                            }
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
                    }));
            objectSource["bytesobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var cCompiler = settings as C.ICOnlyCompilerSettings;
                        cCompiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.5.1\objects\clinic/bytesobject.c.h(323): warning C4100: 'null': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Objects\bytesobject.c(429): warning C4244: 'function': conversion from 'int' to 'char', possible loss of data
                            compiler.DisableWarnings.AddUnique("4127"); // python-3.5.1\objects\stringlib/fastsearch.h(49): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\objects\bytesobject.c(2339) : warning C4706: assignment within conditional expression
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
            objectSource["cellobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Objects\cellobject.c(132): warning C4100: 'closure': unreferenced formal parameter
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
                    }));
            objectSource["odictobject.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
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
                        }
                    });
            }
            else
            {
                // don't use dynload_next, as it's for older OSX (10.2 or below)
                pythonSource.AddFiles("$(packagedir)/Python/dynload_shlib.c");
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
                    }));

            pythonSource["getargs.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\getargs.c(146): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Python\getargs.c(918): warning C4127: conditional expression is constant
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\python\getargs.c(1219) : warning C4706: assignment within conditional expression
                        }

                        // TODO: I cannot see how else some symbols are exported with preprocessor settings
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
                    }));

            pythonSource["_warnings.c"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var cCompiler = settings as C.ICOnlyCompilerSettings;
                        cCompiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Python\_warnings.c(759): warning C4100: 'self': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Python\_warnings.c(131): warning C4456: declaration of 'action' hides previous local declaration
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

                // TODO: use an external zlib?
                var zlib = this.CreateCSourceContainer("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));
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

                var zlibmodule = builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
                zlibmodule.First().PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\zlibmodule.c(122): warning C4100: 'ctx': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\zlibmodule.c(308) : warning C4706: assignment within conditional expression
                        }
                    });
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
                builtinModuleSource.AddFiles("$(packagedir)/Modules/pwdmodule.c");
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
                    }
                });
            builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
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
