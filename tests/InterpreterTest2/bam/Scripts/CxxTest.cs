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
using Python.StandardDistribution;
using System.Linq;
namespace InterpreterTest2
{
    sealed class CxxTest :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCxxSourceCollection("$(packagedir)/source/*.c");
            source.PrivatePatch(settings =>
                {
                    var winCompiler = settings as C.ICommonCompilerSettingsWin;
                    if (null != winCompiler)
                    {
                        winCompiler.CharacterSet = C.ECharacterSet.Unicode;
                    }
                    var visualcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != visualcCompiler)
                    {
                        // warnings in pyhash.h and pytime.h
                        visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        gccCompiler.AllWarnings = true;
                        gccCompiler.ExtraWarnings = true;
                        gccCompiler.Pedantic = false;
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        clangCompiler.AllWarnings = true;
                        clangCompiler.ExtraWarnings = true;
                        clangCompiler.Pedantic = false;
                    }
                });

            this.CompileAndLinkAgainst<Python.PythonLibrary>(source);

            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN");
                    }
                });
        }
    }

    sealed class CxxTestRuntime :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.RegisterPythonModuleTypesToCollate();

            var appAnchor = this.Include<CxxTest>(C.Cxx.ConsoleApplication.ExecutableKey);
            this.IncludePythonStandardDistribution(appAnchor, this.Find<Python.PythonLibrary>().First());

            this.IncludeFiles(this.CreateTokenizedString("$(packagedir)/resources/*.py"), this.ExecutableDir, appAnchor);

            // note that as this is not using PythonShell, it is not adding a dependency on all of the dynamic
            // modules, so these are not built
        }
    }
}
