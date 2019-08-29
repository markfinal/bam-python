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
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class PythonShell :
        C.ConsoleApplication,
        Bam.Core.ICommandLineTool
    {
        protected override void
        Init()
        {
            base.Init();

            var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();

            var source = this.CreateCSourceCollection("$(packagedir)/Programs/python.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim("python_d");
                }
                else
                {
                    this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim("python");
                }
            }
            else
            {
                this.Macros[Bam.Core.ModuleMacroNames.OutputName] = Bam.Core.TokenizedString.CreateVerbatim("python");
                source.DependsOn(pyConfigHeader);
                source.UsePublicPatches(pyConfigHeader);
            }
            source.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }

                    if (settings is VisualCCommon.ICommonCompilerSettings visualcCompiler)
                    {
                        // warnings in pyhash.h and pytime.h
                        visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                    }
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        gccCompiler.AllWarnings = true;
                        gccCompiler.ExtraWarnings = true;
                        gccCompiler.Pedantic = false;
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        clangCompiler.AllWarnings = true;
                        clangCompiler.ExtraWarnings = true;
                        clangCompiler.Pedantic = false;
                    }
                });

            this.LinkAgainst<PythonLibrary>();

            var allModules = Bam.Core.Graph.Instance.FindReferencedModule<AllDynamicModules>();
            this.Requires(allModules);

            this.InheritedEnvironmentVariables = new Bam.Core.StringArray { "*" }; // seem to require all of them
        }

        System.Type Bam.Core.ITool.SettingsType => typeof(PyDocSettings);

        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> EnvironmentVariables { get; private set; }
        public Bam.Core.StringArray InheritedEnvironmentVariables { get; private set; }
        public Bam.Core.TokenizedString Executable => this.GeneratedPaths[C.ConsoleApplication.ExecutableKey];
        public Bam.Core.TokenizedStringArray InitialArguments => null;
        public Bam.Core.TokenizedStringArray TerminatingArguments => null;
        public string UseResponseFileOption => null;
        public Bam.Core.Array<int> SuccessfulExitCodes => new Bam.Core.Array<int> { 0 };
    }

    class ShellRuntime :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.RegisterPythonModuleTypesToCollate();

            var appAnchor = this.Include<PythonShell>(C.ConsoleApplication.ExecutableKey);
            this.IncludePythonStandardDistribution(appAnchor, this.Find<Python.PythonLibrary>().First());
        }
    }
}
