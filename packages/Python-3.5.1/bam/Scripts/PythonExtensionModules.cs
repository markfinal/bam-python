#region License
// Copyright (c) 2010-2016, Mark Final
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
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)]
    class PythonExtensionModule :
        C.Plugin
    {
        private string ModuleName;
        private Bam.Core.StringArray SourceFiles;
        private Bam.Core.StringArray Libraries;
        private Bam.Core.Module.PrivatePatchDelegate CompilationPatch;

        protected PythonExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.StringArray libraries,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
        {
            this.ModuleName = moduleName;
            this.SourceFiles = sourceFiles;
            this.Libraries = libraries;
            this.CompilationPatch = compilationPatch;
        }

        protected PythonExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles)
            :
            this(moduleName, sourceFiles, null, null)
        {}

        protected PythonExtensionModule(
            string moduleName,
            string sourceFile)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile))
        {}

        protected PythonExtensionModule(
            string moduleName,
            string sourceFile,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile), null, compilationPatch)
        {}

        protected PythonExtensionModule(
            string moduleName)
            :
            this(moduleName, new Bam.Core.StringArray(moduleName))
        {}

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["pluginext"] = Bam.Core.TokenizedString.CreateVerbatim(".pyd");
            }
            else
            {
                this.Macros["pluginprefix"] = Bam.Core.TokenizedString.CreateVerbatim(string.Empty);
                this.Macros["pluginext"] = Bam.Core.TokenizedString.CreateVerbatim(".so");
            }
            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(this.ModuleName);

            var source = this.CreateCSourceContainer();
            foreach (var basename in this.SourceFiles)
            {
                source.AddFiles(System.String.Format("$(packagedir)/Modules/{0}.c", basename));
            }
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    var winCompiler = settings as C.ICommonCompilerSettingsWin;
                    winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                }
            });
            if (null != this.CompilationPatch)
            {
                source.PrivatePatch(this.CompilationPatch);
            }

            this.CompileAndLinkAgainst<PythonLibrary>(source);

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
        }
    }

    sealed class StructModule :
        PythonExtensionModule
    {
        public StructModule()
            :
            base("_struct")
        {}
    }

    sealed class ArrayModule :
        PythonExtensionModule
    {
        public ArrayModule()
            :
            base("array", "arraymodule")
        {}
    }

    sealed class CMathModule :
        PythonExtensionModule
    {
        public CMathModule()
            :
            base("cmath", new Bam.Core.StringArray("cmathmodule", "_math"))
        {}
    }

    sealed class MathModule :
        PythonExtensionModule
    {
        public MathModule()
            :
            base("math", new Bam.Core.StringArray("mathmodule", "_math"))
        {}
    }

    sealed class TimeModule :
        PythonExtensionModule
    {
        public TimeModule()
            :
            base("time", "timemodule")
        {}
    }

    sealed class DateTimeModule :
        PythonExtensionModule
    {
        public DateTimeModule()
            :
            base("_datetime", "_datetimemodule")
        {}
    }

    sealed class RandomModule :
        PythonExtensionModule
    {
        public RandomModule()
            :
            base("_random", "_randommodule")
        {}
    }

    sealed class BisectModule :
        PythonExtensionModule
    {
        public BisectModule()
            :
            base("_bisect", "_bisectmodule")
        {}
    }

    sealed class HeapqModule :
        PythonExtensionModule
    {
        public HeapqModule()
            :
            base("_heapq", "_heapqmodule")
        {}
    }

    sealed class PickleModule :
        PythonExtensionModule
    {
        public PickleModule()
            :
            base("_pickle")
        {}
    }

    sealed class AtexitModule :
        PythonExtensionModule
    {
        public AtexitModule()
            :
            base("atexit", "atexitmodule")
        {}
    }

    sealed class JsonModule :
        PythonExtensionModule
    {
        public JsonModule()
            :
            base("_json")
        {}
    }

    sealed class TestCAPIModule :
        PythonExtensionModule
    {
        public TestCAPIModule()
            :
            base("_testcapi", "_testcapimodule")
        {}
    }

    sealed class TestBufferModule :
        PythonExtensionModule
    {
        public TestBufferModule()
            :
            base("_testbuffer")
        {}
    }

    sealed class TestImportMultipleModule :
        PythonExtensionModule
    {
        public TestImportMultipleModule()
            :
            base("_testimportmultiple")
        {}
    }

    sealed class TestMultiPhaseModule :
        PythonExtensionModule
    {
        public TestMultiPhaseModule()
            :
            base("_testmultiphase")
        {}
    }

    sealed class LSProfModule :
        PythonExtensionModule
    {
        public LSProfModule()
            :
            base("_lsprof", new Bam.Core.StringArray("_lsprof", "rotatingtree"))
        {}
    }

    sealed class UnicodeDataModule :
        PythonExtensionModule
    {
        public UnicodeDataModule()
            :
            base("unicodedata")
        {}
    }

    sealed class OpCodeModule :
        PythonExtensionModule
    {
        public OpCodeModule()
            :
            base("_opcode")
        {}
    }

    sealed class FcntlModule :
        PythonExtensionModule
    {
        public FcntlModule()
            :
            base("fcntl", "fcntlmodule", settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99;
                    })
        {}
    }

    sealed class PwdModule :
        PythonExtensionModule
    {
        public PwdModule()
            :
            base("pwd", "pwdmodule")
        {}
    }

    sealed class GrpModule :
        PythonExtensionModule
    {
        public GrpModule()
            :
            base("grp", "grpmodule")
        {}
    }

#if false
    sealed class SPwdModule :
        PythonExtensionModule
    {
        public SPwdModule()
            :
            base("spwd", "spwdmodule")
        {}
    }
#endif

    sealed class SelectModule :
        PythonExtensionModule
    {
        public SelectModule()
            :
            base("select", "selectmodule")
        {}
    }

    sealed class ParserModule :
        PythonExtensionModule
    {
        public ParserModule()
            :
            base("parser", "parsermodule")
        {}
    }

    sealed class MMapModule :
        PythonExtensionModule
    {
        public MMapModule()
            :
            base("mmap", "mmapmodule")
        {}
    }

    sealed class SysLogModule :
        PythonExtensionModule
    {
        public SysLogModule()
            :
            base("syslog", "syslogmodule")
        {}
    }

    sealed class AudioOpModule :
        PythonExtensionModule
    {
        public AudioOpModule()
            :
            base("audioop")
        {}
    }

    sealed class CryptModule :
        PythonExtensionModule
    {
        public CryptModule()
            :
            base("_crypt", "_cryptmodule")
        {}
    }

    sealed class CSVModule :
        PythonExtensionModule
    {
        public CSVModule()
            :
            base("_csv")
        {}
    }

    sealed class PosixSubprocessModule :
        PythonExtensionModule
    {
        public PosixSubprocessModule()
            :
            base("_posixsubprocess")
        {}
    }

    sealed class SocketModule :
        PythonExtensionModule
    {
        public SocketModule()
            :
            base("_socket", "socketmodule")
        {}
    }

    // TODO: deprecated APIs called on OSX
#if false
    sealed class SSLModule :
        PythonExtensionModule
    {
        public SSLModule()
            :
            base("_ssl")
        {}
    }

    sealed class HashLibModule :
        PythonExtensionModule
    {
        public HashLibModule()
            :
            base("_hashlib", "_hashopenssl")
        {}
    }
#endif

    sealed class SHA256Module :
        PythonExtensionModule
    {
        public SHA256Module()
            :
            base("_sha256", "sha256module")
        {}
    }

    sealed class SHA512Module :
        PythonExtensionModule
    {
        public SHA512Module()
            :
            base("_sha512", "sha512module")
        {}
    }

    sealed class MD5Module :
        PythonExtensionModule
    {
        public MD5Module()
            :
            base("_md5", "md5module")
        {}
    }

    sealed class SHA1Module :
        PythonExtensionModule
    {
        public SHA1Module()
            :
            base("_sha1", "sha1module")
        {}
    }

    // TODO sqlite3
    // TODO dbm
    // TODO gdbm

    sealed class TermiosModule :
        PythonExtensionModule
    {
        public TermiosModule()
            :
            base("termios")
        {}
    }

    sealed class ResourceModule :
        PythonExtensionModule
    {
        public ResourceModule()
            :
            base("resource")
        {}
    }

    // TODO nis

    // TODO: needs a library
    #if false
    sealed class CursesModule :
        PythonExtensionModule
    {
        public CursesModule()
            :
            base("_curses", "_cursesmodule")
        {}
    }
    #endif

    // TODO: curses_panel
}
