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
    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class libffiheader :
        C.ProceduralHeaderFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("templateConfig", this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/include/ffi.h.in"));
        }

        protected override Bam.Core.TokenizedString OutputPath => this.CreateTokenizedString("$(packagebuilddir)/$(config)/PublicHeaders/ffi.h");
        protected override string GuardString => null; // the template file already has one
        protected override bool UseSystemIncludeSearchPaths => true;

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                using (System.IO.TextReader reader = new System.IO.StreamReader(this.Macros["templateConfig"].ToString()))
                {
                    contents.Append(reader.ReadToEnd());
                }

                // macro replacements
                contents.Replace("@TARGET@", "xX86_64");
                contents.Replace("@HAVE_LONG_DOUBLE@", "0");
                contents.Replace("@FFI_EXEC_TRAMPOLINE_TABLE@", "0");

                return contents.ToString();
            }
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class libfficonfig :
        C.ProceduralHeaderFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
        }

        protected override Bam.Core.TokenizedString OutputPath => this.CreateTokenizedString("$(packagebuilddir)/$(config)/fficonfig.h"); // not public
        protected override bool UseSystemIncludeSearchPaths => true;

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine("#define STDC_HEADERS 1");
                contents.AppendLine("#ifdef LIBFFI_ASM");
                contents.AppendLine("#define FFI_HIDDEN(name) .hidden name");
                contents.AppendLine("#define EH_FRAME_FLAGS \"aw\"");
                contents.AppendLine("#else");
                contents.AppendLine("#define FFI_HIDDEN __attribute__ ((visibility (\"hidden\")))");
                contents.AppendLine("#endif");
                return contents.ToString();
            }
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class CopyNonPublicHeadersToPublic :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var publishRoot = this.CreateTokenizedString("$(packagebuilddir)/$(config)/PublicHeaders");

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(publishRoot);
                    }
                });

            var headerPaths = new Bam.Core.StringArray
            {
                "src/x86/ffitarget.h",
                "include/ffi_common.h"
            };

            foreach (var header in headerPaths)
            {
                this.IncludeFiles<CopyNonPublicHeadersToPublic>("$(packagedir)/Modules/_ctypes/libffi/" + header, publishRoot, null);
            }
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    class ffi :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            var asmSource = this.CreateAssemblerSourceContainer();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                source.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/*.c");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/x86/ffi.c");
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/x86/sysv.S");
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/x86/win32.S");
                }
                else
                {
                    var ffi64 = source.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/x86/ffi64.c");
                    ffi64.First().PrivatePatch(settings =>
                    {
                        // TODO: Gcc 7+
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-fallthrough");
                        }
                    });
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi/src/x86/unix64.S");
                }

                var copyheaders = Bam.Core.Graph.Instance.FindReferencedModule<CopyNonPublicHeadersToPublic>();
                source.DependsOn(copyheaders);
                source.UsePublicPatches(copyheaders);

                var ffiHeader = Bam.Core.Graph.Instance.FindReferencedModule<libffiheader>();
                this.UsePublicPatches(ffiHeader);
                source.DependsOn(ffiHeader);

                var ffiConfig = Bam.Core.Graph.Instance.FindReferencedModule<libfficonfig>();
                source.UsePublicPatches(ffiConfig);
                source.DependsOn(ffiConfig);
                asmSource.UsePublicPatches(ffiConfig);
                asmSource.DependsOn(ffiConfig);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                var ffi = source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_osx/ffi.c");
                ffi.First().PrivatePatch(settings =>
                    {
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Modules/_ctypes/libffi_osx/ffi.c:108:1: error: unused function 'struct_on_stack' [-Werror,-Wunused-function]
                        }
                    });
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_osx/x86/x86-ffi_darwin.c");
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi_osx/x86/x86-darwin.S");
                }
                else
                {
                    source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_osx/x86/x86-ffi64.c");
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi_osx/x86/darwin64.S");
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var ffi = source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_msvc/ffi.c");
                ffi.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\ffi.c(293): warning C4244: '=': conversion from 'unsigned int' to 'unsigned short', possible loss of data
                            compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\ffi.c(466): warning C4054: 'type cast': from function pointer 'void (__cdecl *)()' to data pointer 'void *'
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\ffi.c(416): warning C4100: 'codeloc': unreferenced formal parameter
                        }
                    });
                var prep_cif = source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_msvc/prep_cif.c");
                prep_cif.First().PrivatePatch(settings =>
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\prep_cif.c(170): warning C4267: '+=': conversion from 'size_t' to 'unsigned int', possible loss of data
                        }
                    });
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    var win32 = source.AddFiles("$(packagedir)/Modules/_ctypes/libffi_msvc/win32.c");
                    win32.First().PrivatePatch(settings =>
                        {
                            if (settings is VisualCCommon.ICommonCompilerSettings)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\win32.c(46): warning C4100: 'fn': unreferenced formal parameter
                            }
                        });
                }
                else
                {
                    asmSource.AddFiles("$(packagedir)/Modules/_ctypes/libffi_msvc/win64.asm");
                }
            }

            source.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        gccCompiler.AllWarnings = true;
                        gccCompiler.ExtraWarnings = true;
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/_ctypes/libffi/src/x86/ffi.c:867:0: error: ISO C forbids an empty translation unit [-Werror=pedantic]

                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/include"));
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_ctypes/libffi/src/debug.c:50:30: error: unused parameter 'a' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("empty-body"); // Python-3.5.1/Modules/_ctypes/libffi/src/debug.c:50:30: error: unused parameter 'a' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("sign-compare"); // Python-3.5.1/Modules/_ctypes/libffi/src/debug.c:50:30: error: unused parameter 'a' [-Werror=unused-parameter]
                        if (this.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug)
                        {
                            compiler.DisableWarnings.AddUnique("unused-result"); // Python-3.5.1/Modules/_ctypes/libffi/src/closures.c:460:17: error: ignoring return value of 'ftruncate', declared with attribute warn_unused_result [-Werror=unused-result]
                        }

                        var cOnly = settings as C.ICOnlyCompilerSettings;
                        cOnly.LanguageStandard = C.ELanguageStandard.C99; // for C++ style comments, etc

                        gccCompiler.PositionIndependentCode = true; // since it's being included into a dynamic library
                    }

                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        clangCompiler.AllWarnings = true;
                        clangCompiler.ExtraWarnings = true;
                        clangCompiler.Pedantic = false; // Python-3.5.1/Modules/_ctypes/libffi_osx/x86/x86-ffi64.c:602:30: error: assigning to 'void *volatile' from 'void (void)' converts between void pointer and function pointer [-Werror,-Wpedantic]

                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("MACOSX");

                        var cOnly = settings as C.ICOnlyCompilerSettings;
                        cOnly.LanguageStandard = C.ELanguageStandard.C99; // for C++ style comments, etc
                    }

                    if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
                    }
                });

            asmSource.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonAssemblerSettings)
                    {
                        var assembler = settings as C.ICommonAssemblerSettings;
                        assembler.PreprocessorDefines.Add("HAVE_AS_X86_PCREL", "1");
                        if (this.BitDepth == C.EBit.ThirtyTwo)
                        {
                            assembler.PreprocessorDefines.Add("HAVE_AS_ASCII_PSEUDO_OP", "1");
                        }
                    }

                    if (settings is ClangCommon.ICommonAssemblerSettings)
                    {
                        var assembler = settings as C.ICommonAssemblerSettings;
                        assembler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_osx/include"));
                        assembler.PreprocessorDefines.Add("MACOSX");
                    }
                });

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.SystemIncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_osx/include"));
                        preprocessor.PreprocessorDefines.Add("MACOSX");
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("comment"); // Python-3.5.1/Modules/_ctypes/libffi_osx/include/x86-ffitarget.h:74:8: error: // comments are not allowed in this language [-Werror,-Wcomment]
                        compiler.DisableWarnings.AddUnique("newline-eof"); // Python-3.5.1/Modules/_ctypes/libffi_osx/include/x86-ffitarget.h:88:34: error: no newline at end of file [-Werror,-Wnewline-eof]
                    }
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_msvc"));
                    }
                });
        }
    }
}
