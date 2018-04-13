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
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python/DynamicModules")]
    class DynamicExtensionModule :
        C.Plugin
    {
        private string ModuleName;
        private Bam.Core.StringArray SourceFiles;
        private Bam.Core.StringArray Libraries;
        private Bam.Core.Module.PrivatePatchDelegate CompilationPatch;
        private Bam.Core.Module.PrivatePatchDelegate LinkerPatch;
        private Bam.Core.StringArray AssemblerFiles;
        private Bam.Core.Module.PrivatePatchDelegate AssemblerPatch;

        protected C.CObjectFileCollection moduleSourceModules;

        protected DynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.StringArray libraries,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch,
            Bam.Core.Module.PrivatePatchDelegate linkerPatch,
            Bam.Core.StringArray assemblerFiles,
            Bam.Core.Module.PrivatePatchDelegate assemberPatch)
        {
            this.ModuleName = moduleName;
            this.SourceFiles = sourceFiles;
            this.Libraries = libraries;
            this.CompilationPatch = compilationPatch;
            this.LinkerPatch = linkerPatch;
            this.AssemblerFiles = (null != assemblerFiles) ? assemblerFiles : null;
            this.AssemblerPatch = assemberPatch;
        }

        protected DynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles)
            :
            this(moduleName, sourceFiles, null, null, null, null, null)
        {}

        protected DynamicExtensionModule(
            string moduleName,
            string sourceFile)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile))
        {}

        protected DynamicExtensionModule(
            string moduleName,
            string sourceFile,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch,
            Bam.Core.Module.PrivatePatchDelegate linkerPatch)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile), null, compilationPatch, linkerPatch, null, null)
        { }

        protected DynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
            :
            this(moduleName, sourceFiles, null, compilationPatch, null, null, null)
        { }

        protected DynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch,
            Bam.Core.Module.PrivatePatchDelegate linkerPatch)
            :
            this(moduleName, sourceFiles, null, compilationPatch, linkerPatch, null, null)
        { }

        protected DynamicExtensionModule(
            string moduleName)
            :
            this(moduleName, new Bam.Core.StringArray(System.String.Format("Modules/{0}", moduleName)))
        {}

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["pluginext"] = Bam.Core.TokenizedString.CreateVerbatim(".pyd");
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
#if BAM_FEATURE_MODULE_CONFIGURATION
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
#else
                if (pyConfigHeader.PyDEBUG)
#endif
                {
                    this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(this.ModuleName + "_d");
                }
                else
                {
                    this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(this.ModuleName);
                }
            }
            else
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(this.ModuleName);
                this.Macros["pluginprefix"] = Bam.Core.TokenizedString.CreateVerbatim(string.Empty);
                this.Macros["pluginext"] = Bam.Core.TokenizedString.CreateVerbatim(".so");
            }

            this.moduleSourceModules = this.CreateCSourceContainer();
            foreach (var basename in this.SourceFiles)
            {
                this.moduleSourceModules.AddFiles(System.String.Format("$(packagedir)/{0}.c", basename));
            }
            this.moduleSourceModules.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    var cCompiler = settings as C.ICOnlyCompilerSettings;
                    cCompiler.LanguageStandard = C.ELanguageStandard.C99; // // some C99 features are now used from 3.6 (https://www.python.org/dev/peps/pep-0007/#c-dialect)
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
                });
            if (null != this.CompilationPatch)
            {
                this.moduleSourceModules.PrivatePatch(this.CompilationPatch);
            }

            if (null != this.AssemblerFiles)
            {
                var assemblerSource = this.CreateAssemblerSourceContainer();
                foreach (var leafname in this.AssemblerFiles)
                {
                    assemblerSource.AddFiles(System.String.Format("$(packagedir)/{0}", leafname));
                }
                if (null != this.AssemblerPatch)
                {
                    assemblerSource.PrivatePatch(this.AssemblerPatch);
                }
            }

            this.CompileAndLinkAgainst<PythonLibrary>(this.moduleSourceModules);

            if (this.Libraries != null)
            {
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        foreach (var lib in this.Libraries)
                        {
                            linker.Libraries.AddUnique(lib);
                        }
                    });
            }
            if (null != this.LinkerPatch)
            {
                this.PrivatePatch(this.LinkerPatch);
            }
        }
    }
}
