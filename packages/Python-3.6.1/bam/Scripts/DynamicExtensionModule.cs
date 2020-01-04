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
namespace Python
{
    // TODO: MF: Is this used externally?
    // TODO: MF: check install_name for macOS - looks like has a lib prefix and version number, and doesn't match output name
    [Bam.Core.ModuleGroup("Thirdparty/Python/DynamicModules")]
    abstract class DynamicExtensionModule :
        C.Plugin
    {
        private readonly string ModuleName;
        private readonly Bam.Core.StringArray SourceFiles;
        private readonly Bam.Core.StringArray LibsToLink;
        private readonly Bam.Core.Module.PrivatePatchDelegate CompilationPatch;
        private readonly Bam.Core.Module.PrivatePatchDelegate LinkerPatch;
        private readonly Bam.Core.StringArray AssemblerFiles;
        private readonly Bam.Core.Module.PrivatePatchDelegate AssemblerPatch;

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
            this.LibsToLink = libraries;
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
        Init()
        {
            base.Init();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros.AddVerbatim(C.ModuleMacroNames.PluginFileExtension, ".pyd");

                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                if ((pyConfigHeader.Configuration as IConfigurePython).PyDEBUG)
                {
                    this.Macros.FromName(Bam.Core.ModuleMacroNames.OutputName).SetVerbatim($"{this.ModuleName}_d");
                }
                else
                {
                    this.Macros.FromName(Bam.Core.ModuleMacroNames.OutputName).SetVerbatim(this.ModuleName);
                }
            }
            else
            {
                this.Macros.FromName(Bam.Core.ModuleMacroNames.OutputName).SetVerbatim(this.ModuleName);
                this.Macros.AddVerbatim(C.ModuleMacroNames.PluginPrefix, string.Empty);
                this.Macros.AddVerbatim(C.ModuleMacroNames.PluginFileExtension, ".so");
            }

            this.SetSemanticVersion(Version.Major, Version.Minor, Version.Patch);

            this.moduleSourceModules = this.CreateCSourceCollection();
            foreach (var basename in this.SourceFiles)
            {
                this.moduleSourceModules.AddFiles(System.String.Format("$(packagedir)/{0}.c", basename));
            }
            this.moduleSourceModules.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.WarningsAsErrors = false;

                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }

                    if (settings is C.ICOnlyCompilerSettings cCompiler)
                    {
                        cCompiler.LanguageStandard = C.ELanguageStandard.C99; // // some C99 features are now used from 3.6 (https://www.python.org/dev/peps/pep-0007/#c-dialect)
                    }

                    if (settings is C.ICommonCompilerSettingsWin winCompiler)
                    {
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                    }
                    if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
                    }
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        gccCompiler.AllWarnings = true;
                        gccCompiler.ExtraWarnings = true;
                        gccCompiler.Pedantic = true;
                        /*
                        if ((settings.Module.Tool as C.CompilerTool).Version.AtLeast(GccCommon.ToolchainVersion.GCC_8))
                        {
                            compiler.DisableWarnings.AddUnique("cast-function-type");
                        }
                        */
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
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
                var assemblerSource = this.CreateAssemblerSourceCollection();
                foreach (var leafname in this.AssemblerFiles)
                {
                    assemblerSource.AddFiles($"$(packagedir)/{leafname}");
                }
                if (null != this.AssemblerPatch)
                {
                    assemblerSource.PrivatePatch(this.AssemblerPatch);
                }
            }

            // TODO
            //this.CompileAndLinkAgainst<PythonLibrary>(this.moduleSourceModules);
            this.LinkOnlyAgainst<PythonLibrary>();

            if (this.LibsToLink != null)
            {
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        foreach (var lib in this.LibsToLink)
                        {
                            linker.Libraries.AddUnique(lib);
                        }
                    });
            }
            if (null != this.LinkerPatch)
            {
                this.PrivatePatch(this.LinkerPatch);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.NotWindows))
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                this.moduleSourceModules.UsePublicPatches(pyConfigHeader);
            }
        }
    }
}
