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
    [Bam.Core.ModuleGroup("Thirdparty/Python/DynamicModules")]
    class PythonDynamicExtensionModule :
        C.Plugin
    {
        private string ModuleName;
        private Bam.Core.StringArray SourceFiles;
        private Bam.Core.StringArray Libraries;
        private Bam.Core.Module.PrivatePatchDelegate CompilationPatch;
        private Bam.Core.Module.PrivatePatchDelegate LinkerPatch;

        protected PythonDynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.StringArray libraries,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch,
            Bam.Core.Module.PrivatePatchDelegate linkerPatch)
        {
            this.ModuleName = moduleName;
            this.SourceFiles = sourceFiles;
            this.Libraries = libraries;
            this.CompilationPatch = compilationPatch;
            this.LinkerPatch = linkerPatch;
        }

        protected PythonDynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles)
            :
            this(moduleName, sourceFiles, null, null, null)
        {}

        protected PythonDynamicExtensionModule(
            string moduleName,
            string sourceFile)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile))
        {}

        protected PythonDynamicExtensionModule(
            string moduleName,
            string sourceFile,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile), null, compilationPatch, null)
        {}

        protected PythonDynamicExtensionModule(
            string moduleName,
            string sourceFile,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch,
            Bam.Core.Module.PrivatePatchDelegate linkerPatch)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile), null, compilationPatch, linkerPatch)
        { }

        protected PythonDynamicExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.Module.PrivatePatchDelegate compilationPatch)
            :
            this(moduleName, sourceFiles, null, compilationPatch, null)
        { }

        protected PythonDynamicExtensionModule(
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
                source.AddFiles(System.String.Format("$(packagedir)/{0}.c", basename));
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
            if (null != this.LinkerPatch)
            {
                this.PrivatePatch(this.LinkerPatch);
            }
        }
    }

    // new list
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires sqlite
    class _sqlite :
        PythonDynamicExtensionModule
    {
        public _sqlite()
            :
            base("_sqlite",
                 "Modules/_sqlite/*",
                settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("MODULE_NAME", "\"sqlite3\"");
                    })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires OpenSSL
    class _hashlib :
        PythonDynamicExtensionModule
    {
        public _hashlib()
            :
            base("_hashlib", "Modules/_hashopenssl")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _opcode :
        PythonDynamicExtensionModule
    {
        public _opcode()
            :
            base("_opcode")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _lsprof :
        PythonDynamicExtensionModule
    {
        public _lsprof()
            :
            base("_lsprof", new Bam.Core.StringArray("Modules/_lsprof", "Modules/rotatingtree"))
        { }
    }

    class _testmultiphase :
        PythonDynamicExtensionModule
    {
        public _testmultiphase()
            :
            base("_testmultiphase")
        { }
    }

    class _testimportmultiple :
        PythonDynamicExtensionModule
    {
        public _testimportmultiple()
            :
            base("_testimportmultiple")
        { }
    }

    class _testbuffer :
        PythonDynamicExtensionModule
    {
        public _testbuffer()
            :
            base("_testbuffer")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _json :
        PythonDynamicExtensionModule
    {
        public _json()
            :
            base("_json")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _thread :
        PythonDynamicExtensionModule
    {
        public _thread()
            :
            base("_thread", "Modules/_threadmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class array :
        PythonDynamicExtensionModule
    {
        public array()
            :
            base("array", "Modules/arraymodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class cmath :
        PythonDynamicExtensionModule
    {
        public cmath()
            :
            base("cmath", new Bam.Core.StringArray("Modules/cmathmodule", "Modules/_math"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class math :
        PythonDynamicExtensionModule
    {
        public math()
            :
            base("math", new Bam.Core.StringArray("Modules/mathmodule", "Modules/_math"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _struct :
        PythonDynamicExtensionModule
    {
        public _struct()
            :
            base("_struct")
        { }
    }

    class _testcapi :
        PythonDynamicExtensionModule
    {
        public _testcapi()
            :
            base("_testcapi", "Modules/_testcapimodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _random :
        PythonDynamicExtensionModule
    {
        public _random()
            :
            base("_random", "Modules/_randommodule")
        { }
    }

    class _elementtree :
        PythonDynamicExtensionModule
    {
        public _elementtree()
            :
            base("_elementtree",
                 "Modules/_elementtree",
                 settings =>
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         compiler.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         compiler.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");
                     })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _pickle :
        PythonDynamicExtensionModule
    {
        public _pickle()
            :
            base("_pickle")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _datetime :
        PythonDynamicExtensionModule
    {
        public _datetime()
            :
            base("_datetime", "Modules/_datetimemodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _bisect :
        PythonDynamicExtensionModule
    {
        public _bisect()
            :
            base("_bisect", "Modules/_bisectmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _heapq :
        PythonDynamicExtensionModule
    {
        public _heapq()
            :
            base("_heapq", "Modules/_heapqmodule")
        { }
    }

    class unicodedata :
        PythonDynamicExtensionModule
    {
        public unicodedata()
            :
            base("unicodedata")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class fcntl :
        PythonDynamicExtensionModule
    {
        public fcntl()
            :
            base("fcntl", "Modules/fcntlmodule", settings =>
            {
                var compiler = settings as C.ICOnlyCompilerSettings;
                compiler.LanguageStandard = C.ELanguageStandard.C99;
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class spwd :
        PythonDynamicExtensionModule
    {
        public spwd()
            :
            base("spwd", "Modules/spwdmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class grp :
        PythonDynamicExtensionModule
    {
        public grp()
            :
            base("grp", "Modules/grpmodule")
        { }
    }

    class select :
        PythonDynamicExtensionModule
    {
        public select()
            :
            base("select", "Modules/selectmodule", null, settings =>
                {
                    var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                    if (null != vcLinker)
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("Ws2_32.lib");
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class mmap :
        PythonDynamicExtensionModule
    {
        public mmap()
            :
            base("mmap", "Modules/mmapmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _csv :
        PythonDynamicExtensionModule
    {
        public _csv()
            :
            base("_csv")
        { }
    }

    class _socket :
        PythonDynamicExtensionModule
    {
        public _socket()
            :
            base("_socket", "Modules/socketmodule", settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("_WINSOCK_DEPRECATED_NO_WARNINGS");
                        compiler.DisableWarnings.AddUnique("4244");
                    }
                },
                settings =>
                {
                    var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                    if (null != vcLinker)
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("Ws2_32.lib");
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires OpenSLL
    class _ssl :
        PythonDynamicExtensionModule
    {
        public _ssl()
            :
            base("_ssl")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not linkable on Windows
    class _crypt :
        PythonDynamicExtensionModule
    {
        public _crypt()
            :
            base("_crypt", "Modules/_cryptmodule", settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4013"); // Python-3.5.1\Modules\_cryptmodule.c(39) : warning C4013: 'crypt' undefined; assuming extern returning int
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class nis :
        PythonDynamicExtensionModule
    {
        public nis()
            :
            base("nis", "Modules/nismodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class termios :
        PythonDynamicExtensionModule
    {
        public termios()
            :
            base("termios")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class resource :
        PythonDynamicExtensionModule
    {
        public resource()
            :
            base("resource")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _posixsubprocess :
        PythonDynamicExtensionModule
    {
        public _posixsubprocess()
            :
            base("_posixsubprocess")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class audioop :
        PythonDynamicExtensionModule
    {
        public audioop()
            :
            base("audioop")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _md5 :
        PythonDynamicExtensionModule
    {
        public _md5()
            :
            base("_md5", "Modules/md5module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha1 :
        PythonDynamicExtensionModule
    {
        public _sha1()
            :
            base("_sha1", "Modules/sha1module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha256 :
        PythonDynamicExtensionModule
    {
        public _sha256()
            :
            base("_sha256", "Modules/sha256module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha512 :
        PythonDynamicExtensionModule
    {
        public _sha512()
            :
            base("_sha512", "Modules/sha512module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // no tcl.h
    class _tkinter :
        PythonDynamicExtensionModule
    {
        public _tkinter()
            :
            base("_tkinter", new Bam.Core.StringArray("Modules/_tkinter", "Modules/tkappinit"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class syslog :
        PythonDynamicExtensionModule
    {
        public syslog()
            :
            base("syslog", "Modules/syslogmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses :
        PythonDynamicExtensionModule
    {
        public _curses()
            :
            base("_curses", new Bam.Core.StringArray("Modules/_cursesmodule"), new Bam.Core.StringArray("-lncurses"), null, null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses_panel :
        PythonDynamicExtensionModule
    {
        public _curses_panel()
            :
            base("_curses_panel", new Bam.Core.StringArray("Modules/_curses_panel"), new Bam.Core.StringArray("-lncurses", "-lpanel"), null, null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _dbm :
        PythonDynamicExtensionModule
    {
        public _dbm()
            :
            base("_dbm", "Modules/_dbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _gdbm :
        PythonDynamicExtensionModule
    {
        public _gdbm()
            :
            base("_gdbm", "Modules/_gdbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class binascii :
        PythonDynamicExtensionModule
    {
        public binascii()
            :
            base("binascii")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class parser :
        PythonDynamicExtensionModule
    {
        public parser()
            :
            base("parser", "Modules/parsermodule")
        { }
    }

    class fpectl :
        PythonDynamicExtensionModule
    {
        public fpectl()
            :
            base("fpectl", "Modules/fpectlmodule")
        { }
    }

    class fpetest :
        PythonDynamicExtensionModule
    {
        public fpetest()
            :
            base(
                "fpetest",
                "Modules/fpetestmodule",
                settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            if (settings.Module.BuildEnvironment.Configuration != EConfiguration.Debug)
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("4723"); // python-3.5.1\modules\fpetestmodule.c(162) : warning C4723: potential divide by 0
                            }
                        }
                    })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class zlib :
        PythonDynamicExtensionModule
    {
        public zlib()
            :
            base("zlib",
                 new Bam.Core.StringArray(
                     "Modules/zlibmodule",
                     "Modules/zlib/adler32",
                     "Modules/zlib/compress",
                     "Modules/zlib/crc32",
                     "Modules/zlib/deflate",
                     "Modules/zlib/gzclose",
                     "Modules/zlib/gzlib",
                     "Modules/zlib/gzread",
                     "Modules/zlib/gzwrite",
                     "Modules/zlib/infback",
                     "Modules/zlib/inffast",
                     "Modules/zlib/inflate",
                     "Modules/zlib/inftrees",
                     "Modules/zlib/trees",
                     "Modules/zlib/uncompr",
                     "Modules/zlib/zutil"))
        { }
    }

    class pyexpat :
        PythonDynamicExtensionModule
    {
        public pyexpat()
            :
            base("pyexpat",
                 new Bam.Core.StringArray("Modules/expat/xmlparse", "Modules/expat/xmlrole", "Modules/expat/xmltok", "Modules/pyexpat"),
                 settings =>
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         compiler.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");
                         compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                         if (null != vcCompiler)
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
        PythonDynamicExtensionModule
    {
        public _multibytecodec()
            :
            base("_multibytecodec", "Modules/cjkcodecs/multibytecodec")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_cn :
        PythonDynamicExtensionModule
    {
        public _codecs_cn()
            :
            base("_codecs_cn", "Modules/cjkcodecs/_codecs_cn")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_hk :
        PythonDynamicExtensionModule
    {
        public _codecs_hk()
            :
            base("_codecs_hk", "Modules/cjkcodecs/_codecs_hk")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_iso2022 :
        PythonDynamicExtensionModule
    {
        public _codecs_iso2022()
            :
            base("_codecs_iso2022", "Modules/cjkcodecs/_codecs_iso2022")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_jp :
        PythonDynamicExtensionModule
    {
        public _codecs_jp()
            :
            base("_codecs_jp", "Modules/cjkcodecs/_codecs_jp")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_kr :
        PythonDynamicExtensionModule
    {
        public _codecs_kr()
            :
            base("_codecs_kr", "Modules/cjkcodecs/_codecs_kr")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_tw :
        PythonDynamicExtensionModule
    {
        public _codecs_tw()
            :
            base("_codecs_tw", "Modules/cjkcodecs/_codecs_tw")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class xxsubtype :
        PythonDynamicExtensionModule
    {
        public xxsubtype()
            :
            base("xxsubtype")
        { }
    }
}
