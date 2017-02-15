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
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    class libffiheader :
        C.ProceduralHeaderFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
        }

        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/$(config)/PublicHeaders/ffi.h");
            }
        }

        protected override string GuardString
        {
            get
            {
                return null; // the template file already has one
            }
        }

        protected override string Contents
        {
            get
            {
                var templatePath = this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/include/ffi.h.in");
                var contents = new System.Text.StringBuilder();
                using (System.IO.TextReader reader = new System.IO.StreamReader(templatePath.Parse()))
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
    class libfficonfig :
        C.ProceduralHeaderFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
        }

        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/$(config)/fficonfig.h"); // not public
            }
        }

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine("#define STDC_HEADERS 1");
                contents.AppendLine("#define FFI_HIDDEN __attribute__ ((visibility (\"hidden\")))"); // TODO : might have to do something else with this with inline ASM
                return contents.ToString();
            }
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    class CopyNonPublicHeadersToPublic :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // the build mode depends on whether this path has been set or not
            if (this.GeneratedPaths.ContainsKey(Key))
            {
                this.GeneratedPaths[Key].Aliased(this.CreateTokenizedString("$(packagebuilddir)/$(config)/PublicHeaders"));
            }
            else
            {
                this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(config)/PublicHeaders"));
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.GeneratedPaths[Key]);
                    }
                });

            var baseHeader = this.IncludeFile(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/src/x86/ffitarget.h"), ".");
            this.IncludeFile(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/include/ffi_common.h"), ".", baseHeader);
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python/libffi")]
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class ffi :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/Modules/_ctypes/libffi/src/*.c");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi/include"));

                    var cOnly = settings as C.ICOnlyCompilerSettings;
                    cOnly.LanguageStandard = C.ELanguageStandard.C99; // for C++ style comments, etc

                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    gccCompiler.PositionIndependentCode = true; // since it's being included into a dynamic library
                });

            var copyheaders = Bam.Core.Graph.Instance.FindReferencedModule<CopyNonPublicHeadersToPublic>();
            source.DependsOn(copyheaders);
            source.UsePublicPatches(copyheaders);

            var ffiHeader = Bam.Core.Graph.Instance.FindReferencedModule<libffiheader>();
            this.UsePublicPatches(ffiHeader);
            source.DependsOn(ffiHeader);

            var ffiConfig = Bam.Core.Graph.Instance.FindReferencedModule<libfficonfig>();
            source.UsePublicPatches(ffiConfig);
            source.DependsOn(ffiConfig);
        }
    }
}
