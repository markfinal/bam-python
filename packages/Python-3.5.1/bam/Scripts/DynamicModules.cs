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
    class _multiprocessing :
        DynamicExtensionModule
    {
        public _multiprocessing()
            :
            base("_multiprocessing",
            new Bam.Core.StringArray("Modules/_multiprocessing/multiprocessing"
#if BAM_HOST_WIN64 || BAM_HOST_LINUX64
                ,"Modules/_multiprocessing/semaphore"
#endif
            ),
            settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(54): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(175): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(158): warning C4189: 'value': local variable is initialized but not referenced
                        compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\Modules\_multiprocessing\semaphore.c(528): warning C4057: 'function': 'long *' differs in indirection to slightly different base types from 'int *'
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_multiprocessing\semaphore.c(120) : warning C4701: potentially uninitialized local variable 'sigint_event' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_multiprocessing\semaphore.c(120) : warning C4703: potentially uninitialized local pointer variable 'sigint_event' used
                    }
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("HAVE_SEM_OPEN");
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("POSIX_SEMAPHORES_NOT_ENABLED"); // macOS does not support semaphores
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

#if !PYTHON_WITH_SQLITE
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires sqlite
#endif
    class _sqlite3 :
        DynamicExtensionModule
    {
        public _sqlite3()
            :
            base("_sqlite3",
                 new Bam.Core.StringArray("Modules/_sqlite/*"),
                 settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("MODULE_NAME", "\"sqlite3\"");
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            compiler.DisableWarnings.AddUnique("4013"); // Python-3.5.1\Modules\_sqlite\statement.c(334): warning C4013: 'sqlite3_transfer_bindings' undefined; assuming extern returning int
                        }
                    },
                 settings =>
                    {
                        var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                        if (null != gccLinker)
                        {
                            gccLinker.CanUseOrigin = true;
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        }
                    })
        { }

#if PYTHON_WITH_SQLITE
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CompileAndLinkAgainst<sqlite.SqliteShared>(this.moduleSourceModules);
        }
#endif
    }

#if !PYTHON_WITH_OPENSSL
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires OpenSSL
#endif
    class _hashlib :
        DynamicExtensionModule
    {
        public _hashlib()
            :
            base(
                "_hashlib",
                "Modules/_hashopenssl",
                null,
                settings =>
                {
                    var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                    if (null != vcLinker)
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        //linker.Libraries.AddUnique("Crypt32.lib");
                        linker.Libraries.AddUnique("User32.lib");
                        linker.Libraries.AddUnique("Advapi32.lib");
                        //linker.Libraries.AddUnique("Ws2_32.lib");
                        linker.Libraries.AddUnique("Gdi32.lib");
                    }
                })
        { }

#if PYTHON_WITH_OPENSSL
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CompileAndLinkAgainst<openssl.OpenSSL>(this.moduleSourceModules);
        }
#endif
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _opcode :
        DynamicExtensionModule
    {
        public _opcode()
            :
            base("_opcode")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _lsprof :
        DynamicExtensionModule
    {
        public _lsprof()
            :
            base("_lsprof", new Bam.Core.StringArray("Modules/_lsprof", "Modules/rotatingtree"))
        { }
    }

    class _testmultiphase :
        DynamicExtensionModule
    {
        public _testmultiphase()
            :
            base(
                "_testmultiphase",
                "Modules/_testmultiphase",
                settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testmultiphase.c(30): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\_testmultiphase.c(84): warning C4152: nonstandard extension, function/data pointer conversion in expression
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(19))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
                    }
                },
                null)
        { }
    }

    class _testimportmultiple :
        DynamicExtensionModule
    {
        public _testimportmultiple()
            :
            base("_testimportmultiple")
        { }
    }

    class _testbuffer :
        DynamicExtensionModule
    {
        public _testbuffer()
            :
            base(
                "_testbuffer",
                "Modules/_testbuffer",
                settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testbuffer.c(208): warning C4100: 'kwds': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_testbuffer.c(2643): warning C4232: nonstandard extension used: 'tp_getattro': address of dllimport 'PyObject_GenericGetAttr' is not static, identity not guaranteed
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(19))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
                        if (compilerUsed.IsAtLeast(18))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\_testbuffer.c(1450) : warning C4306: 'type cast' : conversion from 'int' to 'PyObject *' of greater size
                        }
                    }
                },
                null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _json :
        DynamicExtensionModule
    {
        public _json()
            :
            base("_json")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _thread :
        DynamicExtensionModule
    {
        public _thread()
            :
            base("_thread", "Modules/_threadmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class array :
        DynamicExtensionModule
    {
        public array()
            :
            base("array", "Modules/arraymodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class cmath :
        DynamicExtensionModule
    {
        public cmath()
            :
            base("cmath", new Bam.Core.StringArray("Modules/cmathmodule", "Modules/_math"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class math :
        DynamicExtensionModule
    {
        public math()
            :
            base("math", new Bam.Core.StringArray("Modules/mathmodule", "Modules/_math"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _struct :
        DynamicExtensionModule
    {
        public _struct()
            :
            base("_struct")
        { }
    }

    class _testcapi :
        DynamicExtensionModule
    {
        public _testcapi()
            :
            base(
                "_testcapi",
                "Modules/_testcapimodule",
                settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_testcapimodule.c(58): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testcapimodule.c(52): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_testcapimodule.c(255): warning C4232: nonstandard extension used: 'tp_dealloc': address of dllimport'PyObject_Free' is not static, identity not guaranteed
                        compiler.DisableWarnings.AddUnique("4221"); // Python-3.5.1\Modules\_testcapimodule.c(2504): warning C4221: nonstandard extension used: 'buf': cannot be initialized using address of automatic variable 'data'
                        compiler.DisableWarnings.AddUnique("4204"); // Python-3.5.1\Modules\_testcapimodule.c(2504): warning C4204: nonstandard extension used: non-constant aggregate initializer
                    }
                },
                null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _random :
        DynamicExtensionModule
    {
        public _random()
            :
            base("_random", "Modules/_randommodule")
        { }
    }

    class _elementtree :
        DynamicExtensionModule
    {
        public _elementtree()
            :
            base("_elementtree",
                 new Bam.Core.StringArray("Modules/_elementtree"),
                 settings =>
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         compiler.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         compiler.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");

                         var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                         if (null != vcCompiler)
                         {
                             compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_elementtree.c(315): warning C4100: 'kwds': unreferenced formal parameter
                             compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Modules\_elementtree.c(1680): warning C4457: declaration of 'item' hides function parameter
                             compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\_elementtree.c(1729): warning C4456: declaration of 'cur' hides previous local declaration
                             compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_elementtree.c(2187): warning C4232: nonstandard extension used: 'tp_iter': address of dllimport 'PyObject_SelfIter' is not static, identity not guaranteed
                             compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_elementtree.c(1749) : warning C4706: assignment within conditional expression
                             // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                             var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                                 (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                                 (settings.Module as C.ObjectFile).Compiler;
                             if (compilerUsed.IsAtLeast(19))
                             {
                             }
                             else
                             {
                                 compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                             }
                         }
                     })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _pickle :
        DynamicExtensionModule
    {
        public _pickle()
            :
            base("_pickle")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _datetime :
        DynamicExtensionModule
    {
        public _datetime()
            :
            base("_datetime", "Modules/_datetimemodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _bisect :
        DynamicExtensionModule
    {
        public _bisect()
            :
            base("_bisect", "Modules/_bisectmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _heapq :
        DynamicExtensionModule
    {
        public _heapq()
            :
            base("_heapq", "Modules/_heapqmodule")
        { }
    }

    class unicodedata :
        DynamicExtensionModule
    {
        public unicodedata()
            :
            base(
            "unicodedata",
            new Bam.Core.StringArray("Modules/unicodedata"),
            settings =>
            {
                var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                if (null != vcCompiler)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\unicodedata.c(175): warning C4100: 'self': unreferenced formal parameter
                    compiler.DisableWarnings.AddUnique("4459"); // Python-3.5.1\Modules\unicodedata.c(639): warning C4459: declaration of 'index1' hides global declaration
                    compiler.DisableWarnings.AddUnique("4459"); // Python-3.5.1\Modules\unicodedata.c(639): warning C4459: declaration of 'index1' hides global declaration
                    compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\unicodedata.c(145) : warning C4701: potentially uninitialized local variable 'rc' used
                    compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\unicodedata.c(1273): warning C4232: nonstandard extension used: 'tp_dealloc': address of dllimport 'PyObject_Free' is not static, identity not guaranteed
                    // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                    var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                        (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                        (settings.Module as C.ObjectFile).Compiler;
                    if (compilerUsed.IsAtLeast(19))
                    {
                    }
                    else
                    {
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                    }
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class fcntl :
        DynamicExtensionModule
    {
        public fcntl()
            :
            base("fcntl",
            new Bam.Core.StringArray("Modules/fcntlmodule"),
            settings =>
                {
                    var compiler = settings as C.ICOnlyCompilerSettings;
                    compiler.LanguageStandard = C.ELanguageStandard.C99;
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class spwd :
        DynamicExtensionModule
    {
        public spwd()
            :
            base("spwd", "Modules/spwdmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class grp :
        DynamicExtensionModule
    {
        public grp()
            :
            base("grp", "Modules/grpmodule")
        { }
    }

    class select :
        DynamicExtensionModule
    {
        public select()
            :
            base("select",
            "Modules/selectmodule",
            settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\selectmodule.c(179): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\selectmodule.c(260) : warning C4701: potentially uninitialized local variable 'timeout' used
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\selectmodule.c(98) : warning C4706: assignment within conditional expression
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(19))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
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

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class mmap :
        DynamicExtensionModule
    {
        public mmap()
            :
            base("mmap", "Modules/mmapmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _csv :
        DynamicExtensionModule
    {
        public _csv()
            :
            base("_csv")
        { }
    }

    class _socket :
        DynamicExtensionModule
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
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\socketmodule.c(1134): warning C4100: 'proto': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4245"); // Python-3.5.1\Modules\socketmodule.c(1388): warning C4245: '=': conversion from 'int' to 'std::size_t', signed/unsigned mismatch
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\socketmodule.c(1597): warning C4244: '=': conversion from 'int' to 'ADDRESS_FAMILY', possible loss of data
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\socketmodule.c(2241): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\socketmodule.c(4356): warning C4232: nonstandard extension used: 'tp_getattro': address of dllimport 'PyObject_GenericGetAttr' is not static, identity not guaranteed
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.IsAtLeast(19))
                        {
                        }
                        else
                        {
                            compiler.DisableWarnings.AddUnique("4996"); // Python-3.5.1\Modules\socketmodule.c(6081) : warning C4996: 'GetVersion': was declared deprecated
                        }
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

#if !PYTHON_WITH_OPENSSL
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // requires OpenSSL
#endif
    class _ssl :
        DynamicExtensionModule
    {
        public _ssl()
            :
            base(
                "_ssl",
                "Modules/_ssl",
                settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_ssl.c(2496): warning C4244: '=': conversion from 'Py_ssize_t' to 'int', possible loss of data
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\Modules\_ssl.c(3630): warning C4267: 'function': conversion from 'size_t' to 'long', possible loss of data
                        }
                    },
                settings =>
                    {
                        var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                        if (null != vcLinker)
                        {
                            var linker = settings as C.ICommonLinkerSettings;
                            linker.Libraries.AddUnique("Crypt32.lib");
                            linker.Libraries.AddUnique("User32.lib");
                            linker.Libraries.AddUnique("Advapi32.lib");
                            linker.Libraries.AddUnique("Ws2_32.lib");
                            linker.Libraries.AddUnique("Gdi32.lib");
                        }
                    })
        { }

#if PYTHON_WITH_OPENSSL
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CompileAndLinkAgainst<openssl.OpenSSL>(this.moduleSourceModules);
        }
#endif
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not linkable on Windows
    class _crypt :
        DynamicExtensionModule
    {
        public _crypt()
            :
            base("_crypt",
            new Bam.Core.StringArray("Modules/_cryptmodule"),
            settings =>
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
        DynamicExtensionModule
    {
        public nis()
            :
            base("nis", "Modules/nismodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class termios :
        DynamicExtensionModule
    {
        public termios()
            :
            base("termios")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class resource :
        DynamicExtensionModule
    {
        public resource()
            :
            base("resource")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _posixsubprocess :
        DynamicExtensionModule
    {
        public _posixsubprocess()
            :
            base("_posixsubprocess")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class audioop :
        DynamicExtensionModule
    {
        public audioop()
            :
            base("audioop")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _md5 :
        DynamicExtensionModule
    {
        public _md5()
            :
            base("_md5", "Modules/md5module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha1 :
        DynamicExtensionModule
    {
        public _sha1()
            :
            base("_sha1", "Modules/sha1module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha256 :
        DynamicExtensionModule
    {
        public _sha256()
            :
            base("_sha256", "Modules/sha256module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha512 :
        DynamicExtensionModule
    {
        public _sha512()
            :
            base("_sha512", "Modules/sha512module")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)] // no tcl.h
    class _tkinter :
        DynamicExtensionModule
    {
        public _tkinter()
            :
            base("_tkinter", new Bam.Core.StringArray("Modules/_tkinter", "Modules/tkappinit"))
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class syslog :
        DynamicExtensionModule
    {
        public syslog()
            :
            base("syslog", "Modules/syslogmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses :
        DynamicExtensionModule
    {
        public _curses()
            :
            base("_curses", new Bam.Core.StringArray("Modules/_cursesmodule"), new Bam.Core.StringArray("-lncurses"), null, null, null, null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses_panel :
        DynamicExtensionModule
    {
        public _curses_panel()
            :
            base("_curses_panel", new Bam.Core.StringArray("Modules/_curses_panel"), new Bam.Core.StringArray("-lncurses", "-lpanel"), null, null, null, null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _dbm :
        DynamicExtensionModule
    {
        public _dbm()
            :
            base("_dbm", "Modules/_dbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Invalid)]
    class _gdbm :
        DynamicExtensionModule
    {
        public _gdbm()
            :
            base("_gdbm", "Modules/_gdbmmodule")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class binascii :
        DynamicExtensionModule
    {
        public binascii()
            :
            base("binascii")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class parser :
        DynamicExtensionModule
    {
        public parser()
            :
            base("parser", "Modules/parsermodule")
        { }
    }

    class fpectl :
        DynamicExtensionModule
    {
        public fpectl()
            :
            base("fpectl",
            new Bam.Core.StringArray("Modules/fpectlmodule"),
            settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\fpectlmodule.c(100): warning C4100: 'args': unreferenced formal parameter
                    }
                })
        { }
    }

    class fpetest :
        DynamicExtensionModule
    {
        public fpetest()
            :
            base(
                "fpetest",
                new Bam.Core.StringArray("Modules/fpetestmodule"),
                settings =>
                    {
                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\fpetestmodule.c(62): warning C4100: 'args': unreferenced formal parameter
                            if (settings.Module.BuildEnvironment.Configuration != EConfiguration.Debug)
                            {
                                compiler.DisableWarnings.AddUnique("4723"); // python-3.5.1\modules\fpetestmodule.c(162) : warning C4723: potential divide by 0
                            }
                        }
                    })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class zlib :
        DynamicExtensionModule
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

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.moduleSourceModules.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;

                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        if (this.moduleSourceModules.Compiler.IsAtLeast(700))
                        {
                            compiler.DisableWarnings.AddUnique("shift-negative-value"); // Python-3.5.1/Modules/zlib/inflate.c:1507:61: error: shifting a negative signed value is undefined [-Werror,-Wshift-negative-value]
                        }
                    }
                });

            this.moduleSourceModules["zlibmodule"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/zlib"));
                    }));
        }
    }

    class pyexpat :
        DynamicExtensionModule
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
                             compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\pyexpat.c(18): warning C4232: nonstandard extension used: 'malloc_fcn': address of dllimport 'PyObject_Malloc' is not static, identity not guaranteed
                             compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\expat\xmlparse.c(4916): warning C4100: 'nextPtr': unreferenced formal parameter
                             compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\expat\xmlparse.c(1844) : warning C4244: 'return' : conversion from '__int64' to 'XML_Index', possible loss of data
                             compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\pyexpat.c(1362): warning C4152: nonstandard extension, function/data pointer conversion in expression
                             compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Modules\pyexpat.c(1917): warning C4054: 'type cast': from function pointer 'void (__cdecl *)(void *,const XML_Char *,const XML_Char **)' to data pointer 'xmlhandler'
                             compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\expat\xmlparse.c(1731): warning C4456: declaration of 'keep' hides previous local declaration
                             compiler.DisableWarnings.AddUnique("4127"); // python-3.5.1\modules\expat\xmltok_impl.c(310): warning C4127: conditional expression is constant
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
        DynamicExtensionModule
    {
        public _multibytecodec()
            :
            base("_multibytecodec", "Modules/cjkcodecs/multibytecodec")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_cn :
        DynamicExtensionModule
    {
        public _codecs_cn()
            :
            base("_codecs_cn", "Modules/cjkcodecs/_codecs_cn")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_hk :
        DynamicExtensionModule
    {
        public _codecs_hk()
            :
            base("_codecs_hk", "Modules/cjkcodecs/_codecs_hk")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_iso2022 :
        DynamicExtensionModule
    {
        public _codecs_iso2022()
            :
            base("_codecs_iso2022", "Modules/cjkcodecs/_codecs_iso2022")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_jp :
        DynamicExtensionModule
    {
        public _codecs_jp()
            :
            base("_codecs_jp", "Modules/cjkcodecs/_codecs_jp")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_kr :
        DynamicExtensionModule
    {
        public _codecs_kr()
            :
            base("_codecs_kr", "Modules/cjkcodecs/_codecs_kr")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_tw :
        DynamicExtensionModule
    {
        public _codecs_tw()
            :
            base("_codecs_tw", "Modules/cjkcodecs/_codecs_tw")
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class xxsubtype :
        DynamicExtensionModule
    {
        public xxsubtype()
            :
            base("xxsubtype")
        { }
    }

    class _ctypes :
        DynamicExtensionModule
    {
        public _ctypes()
            :
            base(
            "_ctypes",
            new Bam.Core.StringArray(
                "Modules/_ctypes/_ctypes",
                "Modules/_ctypes/callbacks",
                "Modules/_ctypes/callproc",
                "Modules/_ctypes/cfield",
                "Modules/_ctypes/malloc_closure",
                "Modules/_ctypes/stgdict"
#if BAM_HOST_WIN64
                ,"Modules/_ctypes/libffi_msvc/ffi"
                ,"Modules/_ctypes/libffi_msvc/prep_cif"
#elif BAM_HOST_OSX64
                ,"Modules/_ctypes/libffi_osx/ffi"
                ,"Modules/_ctypes/libffi_osx/x86/x86-ffi64"
                ,"Modules/_ctypes/libffi_osx/x86/x86-ffi_darwin"
#endif
                ),
            null,
            settings =>
                {
                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_msvc"));
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_ctypes\_ctypes.c(155): warning C4100: 'kw': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4054"); // Python-3.5.1\Modules\_ctypes\_ctypes.c(603): warning C4054: 'type cast': from function pointer 'FARPROC' to data pointer 'void *'
                        compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\_ctypes\_ctypes.c(3324): warning C4152: nonstandard extension, function/data pointer conversion in expression
                        compiler.DisableWarnings.AddUnique("4457"); // Python-3.5.1\Modules\_ctypes\_ctypes.c(4437): warning C4457: declaration of 'item' hides function parameter
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_ctypes\_ctypes.c(3505) : warning C4706: assignment within conditional expression
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_ctypes\callbacks.c(239) : warning C4701: potentially uninitialized local variable 'space' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_ctypes\callbacks.c(239) : warning C4703: potentially uninitialized local pointer variable 'space' used
                        compiler.DisableWarnings.AddUnique("4055"); // Python-3.5.1\Modules\_ctypes\callproc.c(1395): warning C4055: 'type cast': from data pointer 'void *' to function pointer 'PPROC'
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_ctypes\cfield.c(1009): warning C4244: 'function': conversion from 'long double' to 'double', possible loss of data
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_ctypes\cfield.c(1599): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4456"); // Python-3.5.1\Modules\_ctypes\stgdict.c(492): warning C4456: declaration of 'len' hides previous local declaration
                        compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\Modules\_ctypes\libffi_msvc\prep_cif.c(170): warning C4267: '+=': conversion from 'size_t' to 'unsigned int', possible loss of data
                    }
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_osx/include"));
                        compiler.PreprocessorDefines.Add("MACOSX");
                        var cOnly = settings as C.ICOnlyCompilerSettings;
                        cOnly.LanguageStandard = C.ELanguageStandard.C99; // for 'inline'
                    }
                    var clangAssembler = settings as ClangCommon.ICommonAssemblerSettings;
                    if (null != clangAssembler)
                    {
                        var assembler = settings as C.ICommonAssemblerSettings;
                        assembler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_osx/include"));
                        assembler.PreprocessorDefines.Add("MACOSX");
                    }
                },
            settings =>
                {
                    var vcLinker = settings as VisualCCommon.ICommonLinkerSettings;
                    if (null != vcLinker)
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("Ole32.lib");
                        linker.Libraries.AddUnique("OleAut32.lib");
                    }
                },
#if BAM_HOST_WIN64
            new Bam.Core.StringArray("Modules/_ctypes/libffi_msvc/win64.asm"),
            null
#elif BAM_HOST_OSX64
            new Bam.Core.StringArray(
                "Modules/_ctypes/libffi_osx/x86/darwin64.S",
                "Modules/_ctypes/libffi_osx/x86/x86-darwin.S"),
            settings =>
            {
                var clangAssembler = settings as ClangCommon.ICommonAssemblerSettings;
                if (null != clangAssembler)
                {
                    var assembler = settings as C.ICommonAssemblerSettings;
                    assembler.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_osx/include"));
                    assembler.PreprocessorDefines.Add("MACOSX");
                }
            }
#else
            null,
            null
#endif
            )
        { }

#if BAM_HOST_LINUX64
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init (parent);

            this.CompileAndLinkAgainst<ffi>(this.moduleSourceModules);
        }
#endif
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)]
    class _scproxy :
        DynamicExtensionModule
    {
        public _scproxy()
            :
            base(
            "_scproxy",
            new Bam.Core.StringArray("Modules/_scproxy"),
            settings =>
                {
                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("tautological-pointer-compare"); // Python-3.5.1/Modules/_scproxy.c:74:10: error: comparison of address of 'kSCPropNetProxiesExcludeSimpleHostnames' not equal to a null pointer is always true [-Werror,-Wtautological-pointer-compare]
                    }
                },
            settings =>
                {
                    var osxLinker = settings as C.ICommonLinkerSettingsOSX;
                    if (null != osxLinker)
                    {
                        osxLinker.Frameworks.AddUnique("SystemConfiguration");
                        osxLinker.Frameworks.AddUnique("CoreFoundation");
                    }
                })
        {}
    }
}
