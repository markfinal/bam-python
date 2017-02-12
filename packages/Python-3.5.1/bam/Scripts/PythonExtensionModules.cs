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
    [Bam.Core.ModuleGroup("Thirdparty/Python/Module")]
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
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
            :
            this(moduleName, sourceFiles, null, compilationPatch)
        { }

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
                    var winCompiler = settings as C.ICommonCompilerSettingsWin;
                    if (null != winCompiler)
                    {
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                    }
                    var visualcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != visualcCompiler)
                    {
                        // warnings are present over warning level 3
                        visualcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level3;
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

    // new list
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _posixsubprocess :
        PythonExtensionModule
    {
        public _posixsubprocess()
            :
            base("_posixsubprocess")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class audioop :
        PythonExtensionModule
    {
        public audioop()
            :
            base("audioop")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _md5 :
        PythonExtensionModule
    {
        public _md5()
            :
            base("_md5", "md5module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha1 :
        PythonExtensionModule
    {
        public _sha1()
            :
            base("_sha1", "sha1module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha256 :
        PythonExtensionModule
    {
        public _sha256()
            :
            base("_sha256", "sha256module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha512 :
        PythonExtensionModule
    {
        public _sha512()
            :
            base("_sha512", "sha512module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // no tcl.h
    class _tkinter :
        PythonExtensionModule
    {
        public _tkinter()
            :
            base("_tkinter", new Bam.Core.StringArray("_tkinter", "tkappinit"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class syslog :
        PythonExtensionModule
    {
        public syslog()
            :
            base("syslog", "syslogmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _curses :
        PythonExtensionModule
    {
        public _curses()
            :
            base("_curses", new Bam.Core.StringArray("_cursesmodule"), new Bam.Core.StringArray("-lncurses"), null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _curses_panel :
        PythonExtensionModule
    {
        public _curses_panel()
            :
            base("_curses_panel", new Bam.Core.StringArray("_curses_panel"), new Bam.Core.StringArray("-lncurses", "-lpanel"), null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _dbm :
        PythonExtensionModule
    {
        public _dbm()
            :
            base("_dbm", "_dbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _gdbm :
        PythonExtensionModule
    {
        public _gdbm()
            :
            base("_gdbm", "_gdbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class binascii :
        PythonExtensionModule
    {
        public binascii()
            :
            base("binascii")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class parser :
        PythonExtensionModule
    {
        public parser()
            :
            base("parser", "parsermodule")
        { }
    }

    class fpectl :
        PythonExtensionModule
    {
        public fpectl()
            :
            base("fpectl", "fpectlmodule")
        { }
    }

    class fpetest :
        PythonExtensionModule
    {
        public fpetest()
            :
            base("fpetest", "fpetestmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class zlib :
        PythonExtensionModule
    {
        public zlib()
            :
            base("zlib",
                 new Bam.Core.StringArray(
                     "zlibmodule",
                     "zlib/adler32",
                     "zlib/compress",
                     "zlib/crc32",
                     "zlib/deflate",
                     "zlib/gzclose",
                     "zlib/gzlib",
                     "zlib/gzread",
                     "zlib/gzwrite",
                     "zlib/infback",
                     "zlib/inffast",
                     "zlib/inflate",
                     "zlib/inftrees",
                     "zlib/trees",
                     "zlib/uncompr",
                     "zlib/zutil"))
        { }
    }

    class pyexpat :
        PythonExtensionModule
    {
        public pyexpat()
            :
            base("pyexpat",
                 new Bam.Core.StringArray("expat/xmlparse", "expat/xmlrole", "expat/xmltok", "pyexpat"),
                 settings =>
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         compiler.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");
                         compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         if (settings.Module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                         {
                             compiler.PreprocessorDefines.Add("COMPILED_FROM_DSP"); // to indicate a Windows build
                             compiler.PreprocessorDefines.Add("XML_STATIC"); // to avoid unwanted declspecs
                             compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\expat\xmlparse.c(1844) : warning C4244: 'return' : conversion from '__int64' to 'XML_Index', possible loss of data
                         }
                         else
                         {
                             compiler.PreprocessorDefines.Add("HAVE_MEMMOVE", "1");
                         }
                     })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _multibytecodec :
        PythonExtensionModule
    {
        public _multibytecodec()
            :
            base("_multibytecodec", "cjkcodecs/multibytecodec")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_cn :
        PythonExtensionModule
    {
        public _codecs_cn()
            :
            base("_codecs_cn", "cjkcodecs/_codecs_cn")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_hk :
        PythonExtensionModule
    {
        public _codecs_hk()
            :
            base("_codecs_hk", "cjkcodecs/_codecs_hk")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_iso2022 :
        PythonExtensionModule
    {
        public _codecs_iso2022()
            :
            base("_codecs_iso2022", "cjkcodecs/_codecs_iso2022")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_jp :
        PythonExtensionModule
    {
        public _codecs_jp()
            :
            base("_codecs_jp", "cjkcodecs/_codecs_jp")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_kr :
        PythonExtensionModule
    {
        public _codecs_kr()
            :
            base("_codecs_kr", "cjkcodecs/_codecs_kr")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_tw :
        PythonExtensionModule
    {
        public _codecs_tw()
            :
            base("_codecs_tw", "cjkcodecs/_codecs_tw")
        { }
    }

    class unicodedata :
        PythonExtensionModule
    {
        public unicodedata()
            :
            base("unicodedata")
        { }
    }

    // old list
    class StructModule :
        PythonExtensionModule
    {
        public StructModule()
            :
            base("_struct")
        {}
    }

    class ArrayModule :
        PythonExtensionModule
    {
        public ArrayModule()
            :
            base("array", "arraymodule")
        {}
    }

    class CMathModule :
        PythonExtensionModule
    {
        public CMathModule()
            :
            base("cmath", new Bam.Core.StringArray("cmathmodule", "_math"))
        {}
    }

    class MathModule :
        PythonExtensionModule
    {
        public MathModule()
            :
            base("math", new Bam.Core.StringArray("mathmodule", "_math"))
        {}
    }

    class TimeModule :
        PythonExtensionModule
    {
        public TimeModule()
            :
            base("time", "timemodule")
        {}
    }

    class DateTimeModule :
        PythonExtensionModule
    {
        public DateTimeModule()
            :
            base("_datetime", "_datetimemodule")
        {}
    }

    class RandomModule :
        PythonExtensionModule
    {
        public RandomModule()
            :
            base("_random", "_randommodule")
        {}
    }

    class BisectModule :
        PythonExtensionModule
    {
        public BisectModule()
            :
            base("_bisect", "_bisectmodule")
        {}
    }

    class HeapqModule :
        PythonExtensionModule
    {
        public HeapqModule()
            :
            base("_heapq", "_heapqmodule")
        {}
    }

    class PickleModule :
        PythonExtensionModule
    {
        public PickleModule()
            :
            base("_pickle")
        {}
    }

    class AtexitModule :
        PythonExtensionModule
    {
        public AtexitModule()
            :
            base("atexit", "atexitmodule")
        {}
    }

    class JsonModule :
        PythonExtensionModule
    {
        public JsonModule()
            :
            base("_json")
        {}
    }

    class TestCAPIModule :
        PythonExtensionModule
    {
        public TestCAPIModule()
            :
            base("_testcapi", "_testcapimodule")
        {}
    }

    class TestBufferModule :
        PythonExtensionModule
    {
        public TestBufferModule()
            :
            base("_testbuffer")
        {}
    }

    class TestImportMultipleModule :
        PythonExtensionModule
    {
        public TestImportMultipleModule()
            :
            base("_testimportmultiple")
        {}
    }

    class TestMultiPhaseModule :
        PythonExtensionModule
    {
        public TestMultiPhaseModule()
            :
            base("_testmultiphase")
        {}
    }

    class LSProfModule :
        PythonExtensionModule
    {
        public LSProfModule()
            :
            base("_lsprof", new Bam.Core.StringArray("_lsprof", "rotatingtree"))
        {}
    }

    class OpCodeModule :
        PythonExtensionModule
    {
        public OpCodeModule()
            :
            base("_opcode")
        {}
    }

    class FcntlModule :
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

    class PwdModule :
        PythonExtensionModule
    {
        public PwdModule()
            :
            base("pwd", "pwdmodule")
        {}
    }

    class GrpModule :
        PythonExtensionModule
    {
        public GrpModule()
            :
            base("grp", "grpmodule")
        {}
    }

#if false
    class SPwdModule :
        PythonExtensionModule
    {
        public SPwdModule()
            :
            base("spwd", "spwdmodule")
        {}
    }
#endif

    class SelectModule :
        PythonExtensionModule
    {
        public SelectModule()
            :
            base("select", "selectmodule")
        {}
    }

    class MMapModule :
        PythonExtensionModule
    {
        public MMapModule()
            :
            base("mmap", "mmapmodule")
        {}
    }

    class CryptModule :
        PythonExtensionModule
    {
        public CryptModule()
            :
            base("_crypt", "_cryptmodule")
        {}
    }

    class CSVModule :
        PythonExtensionModule
    {
        public CSVModule()
            :
            base("_csv")
        {}
    }

    class SocketModule :
        PythonExtensionModule
    {
        public SocketModule()
            :
            base("_socket", "socketmodule")
        {}
    }

    // TODO: deprecated APIs called on OSX
#if false
    class SSLModule :
        PythonExtensionModule
    {
        public SSLModule()
            :
            base("_ssl")
        {}
    }

    class HashLibModule :
        PythonExtensionModule
    {
        public HashLibModule()
            :
            base("_hashlib", "_hashopenssl")
        {}
    }
#endif

    // TODO sqlite3
    // TODO dbm
    // TODO gdbm

    class TermiosModule :
        PythonExtensionModule
    {
        public TermiosModule()
            :
            base("termios")
        {}
    }

    class ResourceModule :
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
    class CursesModule :
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
