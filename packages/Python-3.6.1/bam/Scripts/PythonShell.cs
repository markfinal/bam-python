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
using Python.StandardDistribution;
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class PythonShell :
        C.ConsoleApplication,
        Bam.Core.ICommandLineTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");

            var source = this.CreateCSourceContainer("$(packagedir)/Programs/python.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.Linker is VisualCCommon.LinkerBase)
                {
                    this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
                }
            }
            else
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                source.DependsOn(pyConfigHeader);
                source.UsePublicPatches(pyConfigHeader);
            }
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
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

            this.LinkAgainst<PythonLibrary>();
            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                if (null != gccLinker)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.AddUnique("$ORIGIN");
                }
                var clangLinker = settings as ClangCommon.ICommonLinkerSettings;
                if (null != clangLinker)
                {
                    // standard distribution path
                    clangLinker.RPath.AddUnique(System.String.Format("@executable_path/{0}", StandardDistribution.PublisherExtensions.ModuleDirectory));
                }
            });

            var allModules = Bam.Core.Graph.Instance.FindReferencedModule<AllDynamicModules>();
            this.Requires(allModules);
        }

        public Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module) where T : Bam.Core.Module
        {
            return new PyDocSettings(this); // TODO: currently, only pydoc is support, but this should be more generic, or not have any settings at all
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> EnvironmentVariables
        {
            get;
            private set;
        }

        public Bam.Core.StringArray InheritedEnvironmentVariables
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.GeneratedPaths[C.ConsoleApplication.Key];
            }
        }

        public Bam.Core.TokenizedStringArray InitialArguments
        {
            get
            {
                return null;
            }
        }

        public Bam.Core.TokenizedStringArray TerminatingArguments
        {
            get
            {
                return null;
            }
        }

        public string UseResponseFileOption
        {
            get
            {
                return null;
            }
        }
    }

    class ShellRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.RegisterPythonModuleTypesToCollate();

            var appAnchor = this.Include<PythonShell>(C.ConsoleApplication.Key);
            this.IncludePythonStandardDistribution(appAnchor);
#else
            var app = this.Include<PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            StandardDistribution.Publish(this, app);
#endif
        }
    }
}
