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
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(54): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(175): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4189"); // Python-3.5.1\Modules\_multiprocessing\multiprocessing.c(158): warning C4189: 'value': local variable is initialized but not referenced
                        compiler.DisableWarnings.AddUnique("4057"); // Python-3.5.1\Modules\_multiprocessing\semaphore.c(528): warning C4057: 'function': 'long *' differs in indirection to slightly different base types from 'int *'
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\_multiprocessing\semaphore.c(120) : warning C4701: potentially uninitialized local variable 'sigint_event' used
                        compiler.DisableWarnings.AddUnique("4703"); // python-3.5.1\modules\_multiprocessing\semaphore.c(120) : warning C4703: potentially uninitialized local pointer variable 'sigint_event' used
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("HAVE_SEM_OPEN");
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_multiprocessing/semaphore.c:335:48: error: unused parameter 'args' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_multiprocessing/semaphore.c:653:1: error: missing initializer for field 'tp_free' of 'PyTypeObject' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("POSIX_SEMAPHORES_NOT_ENABLED"); // macOS does not support semaphores
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_multiprocessing/multiprocessing.c:158:31: error: unused variable 'value' [-Werror,-Wunused-variable]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_multiprocessing/multiprocessing.c:134:10: error: missing field 'ml_meth' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-variable"); // Python-3.5.1/Modules/_multiprocessing/multiprocessing.c:158:31: error: unused variable 'value' [-Werror,-Wunused-variable]
                    }
                },
            settings =>
                {
                    if (settings is VisualCCommon.ICommonLinkerSettings)
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
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("MODULE_NAME", "\"sqlite3\"");
                        var compiler = settings as C.ICommonCompilerSettings;
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_sqlite\cache.c(57): warning C4100: 'kwargs': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_sqlite\module.c(358) : warning C4706: assignment within conditional expression
                            compiler.DisableWarnings.AddUnique("4702"); // python-3.5.1\modules\_sqlite\statement.c(475) : warning C4702: unreachable code
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_sqlite\util.c(161): warning C4127: conditional expression is constant
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_sqlite/cache.c:256:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_sqlite/cache.c:57:73: error: unused parameter 'kwargs' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/_sqlite/statement.c:334:19: error: implicit declaration of function 'sqlite3_transfer_bindings' [-Werror,-Wimplicit-function-declaration]
                        }
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_sqlite/cache.c:256:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_sqlite/cache.c:57:73: error: unused parameter 'kwargs' [-Werror=unused-parameter]
                        }
                    },
                 settings =>
                    {
                        if (settings is GccCommon.ICommonLinkerSettings gccLinker)
                        {
                            gccLinker.CanUseOrigin = true;
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        }
                    })
        { }

#if PYTHON_WITH_SQLITE
        protected override void
        Init()
        {
            base.Init();

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
                settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_hashopenssl.c(122): warning C4100: 'unused': unreferenced formal parameter
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_hashopenssl.c:215:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_hashopenssl.c:122:37: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_hashopenssl.c:215:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_hashopenssl.c:122:37: error: unused parameter 'unused' [-Werror=unused-parameter]
                    }
                },
                settings =>
                {
                    if (settings is VisualCCommon.ICommonLinkerSettings)
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
        Init()
        {
            base.Init();

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
            base(
            "_opcode",
            new Bam.Core.StringArray("Modules/_opcode"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_opcode.c:23:40: error: unused parameter 'module' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_opcode.c:23:40: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _lsprof :
        DynamicExtensionModule
    {
        public _lsprof()
            :
            base("_lsprof",
            new Bam.Core.StringArray("Modules/_lsprof", "Modules/rotatingtree"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_lsprof.c:264:29: error: unused parameter 'pObj' [-Werror=unused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_lsprof.c:608:1: error: string length '772' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_lsprof.c:797:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_lsprof.c:264:29: error: unused parameter 'pObj' [-Werror,-Wunused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_lsprof.c:608:28: error: string literal of length 772 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_lsprof.c:510:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                }
            })
        { }
    }

    class _testmultiphase :
        DynamicExtensionModule
    {
        public _testmultiphase()
            :
            base(
                "_testmultiphase",
                new Bam.Core.StringArray("Modules/_testmultiphase"),
                settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testmultiphase.c(30): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\_testmultiphase.c(84): warning C4152: nonstandard extension, function/data pointer conversion in expression
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
                    }
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testmultiphase.c:30:29: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testmultiphase.c:47:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/_testmultiphase.c:84:5: error: ISO C forbids initialization between function pointer and 'void *' [-Werror=pedantic]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testmultiphase.c:30:29: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testmultiphase.c:47:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        clangCompiler.Pedantic = false; // Python-3.5.1/Modules/_testmultiphase.c:84:22: error: initializing 'void *' with an expression of type 'int (ExampleObject *)' converts between void pointer and function pointer [-Werror,-Wpedantic]
                    }
                })
        { }
    }

    class _testimportmultiple :
        DynamicExtensionModule
    {
        public _testimportmultiple()
            :
            base("_testimportmultiple")
        {}
    }

    class _testbuffer :
        DynamicExtensionModule
    {
        public _testbuffer()
            :
            base(
                "_testbuffer",
                new Bam.Core.StringArray("Modules/_testbuffer"),
                settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testbuffer.c(208): warning C4100: 'kwds': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_testbuffer.c(2643): warning C4232: nonstandard extension used: 'tp_getattro': address of dllimport 'PyObject_GenericGetAttr' is not static, identity not guaranteed
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
                        if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2013))
                        {
                            compiler.DisableWarnings.AddUnique("4306"); // Python-3.5.1\Modules\_testbuffer.c(1450) : warning C4306: 'type cast' : conversion from 'int' to 'PyObject *' of greater size
                        }
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testbuffer.c:208:27: error: unused parameter 'type' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testbuffer.c:1971:1: error: missing initializer for field 'was_sq_slice' of 'PySequenceMethods' [-Werror=missing-field-initializers]
                        if (settings.Module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug)
                        {
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_testbuffer.c:1564:9: error: string length '2295' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testbuffer.c:208:27: error: unused parameter 'type' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testbuffer.c:1971:1: error: missing field 'was_sq_slice' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _json :
        DynamicExtensionModule
    {
        public _json()
            :
            base(
            "_json",
            new Bam.Core.StringArray("Modules/_json"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_json.c:33:5: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_json.c:1203:43: error: unused parameter 'args' [-Werror=unused-parameter]

                    var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                        (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                        (settings.Module as C.ObjectFile).Compiler;

                    if (compilerUsed.Version.AtLeast(GccCommon.ToolchainVersion.GCC_5_4))
                    {
                        if (0 != (settings.Module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug))
                        {
                            compiler.DisableWarnings.AddUnique("strict-overflow"); // Python-3.6.1/Modules/_json.c:398:1: error: assuming signed overflow does not occur when assuming that (X + c) >= X is always true [-Werror=strict-overflow]
                        }
                    }
                }

                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_json.c:33:91: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_json.c:1203:43: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _thread :
        DynamicExtensionModule
    {
        public _thread()
            :
            base(
            "_thread",
            new Bam.Core.StringArray("Modules/_threadmodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_threadmodule.c:231:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_threadmodule.c:327:1: error: string length '666' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_threadmodule.c:445:41: error: unused parameter 'args' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_threadmodule.c:231:26: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_threadmodule.c:328:1: error: string literal of length 666 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_threadmodule.c:445:41: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class array :
        DynamicExtensionModule
    {
        public array()
            :
            base(
            "array",
            new Bam.Core.StringArray("Modules/arraymodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/arraymodule.c.h:276:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/arraymodule.c:2742:1: error: string length '2357' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/arraymodule.c:777:55: error: unused parameter 'unused' [-Werror=unused-parameter]
                    gccCompiler.Pedantic = false; //Python-3.5.1/Modules/arraymodule.c:3021:5: error: ISO C forbids initialization between function pointer and 'void *' [-Werror=pedantic]
                }
                if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/arraymodule.c.h:276:35: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/arraymodule.c:2743:1: error: string literal of length 2357 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/arraymodule.c:777:55: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                    clangCompiler.Pedantic = false; // Python-3.5.1/Modules/arraymodule.c:3021:19: error: initializing 'void *' with an expression of type 'int (PyObject *)' (aka 'int (struct _object *)') converts between void pointer and function pointer [-Werror,-Wpedantic]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class cmath :
        DynamicExtensionModule
    {
        public cmath()
            :
            base("cmath",
            new Bam.Core.StringArray("Modules/cmathmodule", "Modules/_math"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cmathmodule.c:1214:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/cmathmodule.c.h:810:1: error: string length '688' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cmathmodule.c:436:30: error: unused parameter 'module' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cmathmodule.c:1214:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/cmathmodule.c.h:811:1: error: string literal of length 688 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cmathmodule.c:436:30: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class math :
        DynamicExtensionModule
    {
        public math()
            :
            base("math", new Bam.Core.StringArray("Modules/mathmodule", "Modules/_math"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/mathmodule.c:77:5: error: implicit declaration of function 'round' [-Werror=implicit-function-declaration]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/mathmodule.c:2049:1: error: string length '706' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // /Python-3.5.1/Modules/mathmodule.c:689:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/mathmodule.c:2116:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/mathmodule.c:689:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/mathmodule.c:2050:1: error: string literal of length 706 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/mathmodule.c:2116:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _struct :
        DynamicExtensionModule
    {
        public _struct()
            :
            base(
            "_struct",
            new Bam.Core.StringArray("Modules/_struct"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_struct.c:346:41: error: unused parameter 'f' [-Werror=unused-parameter]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_struct.c:727:5: error: missing initializer for field 'pack' of 'formatdef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_struct.c:2225:1: error: string length '1284' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_struct.c:346:41: error: unused parameter 'f' [-Werror,-Wunused-parameter]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_struct.c:727:53: error: missing field 'pack' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_struct.c:2226:1: error: string literal of length 1284 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                }
            })
        { }
    }

    class _testcapi :
        DynamicExtensionModule
    {
        public _testcapi()
            :
            base(
                "_testcapi",
                new Bam.Core.StringArray("Modules/_testcapimodule"),
                settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\_testcapimodule.c(58): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\_testcapimodule.c(52): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\_testcapimodule.c(255): warning C4232: nonstandard extension used: 'tp_dealloc': address of dllimport'PyObject_Free' is not static, identity not guaranteed
                        compiler.DisableWarnings.AddUnique("4221"); // Python-3.5.1\Modules\_testcapimodule.c(2504): warning C4221: nonstandard extension used: 'buf': cannot be initialized using address of automatic variable 'data'
                        compiler.DisableWarnings.AddUnique("4204"); // Python-3.5.1\Modules\_testcapimodule.c(2504): warning C4204: nonstandard extension used: non-constant aggregate initializer
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.6.1\modules\_testcapimodule.c(1909) : warning C4706: assignment within conditional expression
                    }
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testcapimodule.c:52:23: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testcapimodule.c:294:1: error: missing initializer for field 'tp_free' of 'PyTypeObject' [-Werror=missing-field-initializers]
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/_testcapimodule.c:2505:9: error: initializer element is not computable at load time [-Werror]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_testcapimodule.c:52:23: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_testcapimodule.c:294:1: error: missing field 'tp_free' initializer [-Werror,-Wmissing-field-initializers]
                        if ((settings.Module.Tool as C.CompilerTool).Version.AtMost(ClangCommon.ToolchainVersion.Xcode_9_4_1))
                        {
                            compiler.DisableWarnings.AddUnique("extended-offsetof"); // Python-3.5.1/Modules/_testcapimodule.c:3738:24: error: using extended field designator is an extension [-Werror,-Wextended-offsetof]
                        }
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _random :
        DynamicExtensionModule
    {
        public _random()
            :
            base(
            "_random",
            new Bam.Core.StringArray("Modules/_randommodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_randommodule.c:428:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_randommodule.c:428:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                }
            })
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
                         var preprocessor = settings as C.ICommonPreprocessorSettings;
                         preprocessor.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         preprocessor.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         preprocessor.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");

                         var compiler = settings as C.ICommonCompilerSettings;
                         if (settings is VisualCCommon.ICommonCompilerSettings)
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
                             if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                             {
                                 compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                             }
                         }

                         if (settings is GccCommon.ICommonCompilerSettings)
                         {
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_elementtree.c:315:43: error: unused parameter 'args' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_elementtree.c:1972:1: error: missing initializer for field 'sq_contains' of 'PySequenceMethods' [-Werror=missing-field-initializers]
                         }

                         if (settings is ClangCommon.ICommonCompilerSettings)
                         {
                             compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_elementtree.c:315:43: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                             compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_elementtree.c:1972:1: error: missing field 'sq_contains' initializer [-Werror,-Wmissing-field-initializers]
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
            base(
            "_pickle",
            new Bam.Core.StringArray("Modules/_pickle"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_pickle.c:391:1: error: missing initializer for field 'tp_print' of 'PyTypeObject' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_pickle.c:875:42: error: unused parameter 'self' [-Werror=unused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/_pickle.c.h:64:1: error: string length '928' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("overflow"); // Python-3.5.1/Modules/_pickle.c:977:13: error: overflow in implicit constant conversion [-Werror=overflow]
                    compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Modules/_pickle.c:1464:23: error: ISO C90 does not support the 'z' gnu_printf length modifier [-Werror=format=]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_pickle.c:391:1: error: missing field 'tp_print' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/_pickle.c.h:65:1: error: string literal of length 928 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_pickle.c:875:42: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _datetime :
        DynamicExtensionModule
    {
        public _datetime()
            :
            base("_datetime",
            new Bam.Core.StringArray("Modules/_datetimemodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_datetimemodule.c:995:5: error: string length '7029' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/_datetimemodule.c:2151:9: error: implicit declaration of function 'round' [-Werror=implicit-function-declaration]
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_datetimemodule.c:2307:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_datetimemodule.c:2398:40: error: unused parameter 'unused' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_datetimemodule.c:2297:10: error: missing field 'type' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_datetimemodule.c:2398:40: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _bisect :
        DynamicExtensionModule
    {
        public _bisect()
            :
            base(
            "_bisect",
            new Bam.Core.StringArray("Modules/_bisectmodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_bisectmodule.c:233:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_bisectmodule.c:47:24: error: unused parameter 'self' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_bisectmodule.c:233:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_bisectmodule.c:47:24: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                }
            })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _heapq :
        DynamicExtensionModule
    {
        public _heapq()
            :
            base(
            "_heapq",
            new Bam.Core.StringArray("Modules/_heapqmodule"),
            settings =>
            {
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_heapqmodule.c:494:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_heapqmodule.c:100:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_heapqmodule.c:497:1: error: string length '1263' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_heapqmodule.c:494:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_heapqmodule.c:100:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_heapqmodule.c:498:1: error: string literal of length 1263 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                }
            })
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
                if (settings is VisualCCommon.ICommonCompilerSettings)
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
                    if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                    {
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                    }
                }
                if (settings is GccCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/unicodedata.c:81:9: error: missing initializer for field 'doc' of 'PyMemberDef' [-Werror=missing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/unicodedata.c:175:38: error: unused parameter 'self' [-Werror=unused-parameter]
                }
                if (settings is ClangCommon.ICommonCompilerSettings)
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/unicodedata.c:81:82: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                    compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/unicodedata.c:175:38: error: unused parameter 'self' [-Werror,-Wunused-parameter]
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
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fcntlmodule.c:428:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fcntlmodule.c:59:31: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/fcntlmodule.c.h:5:1: error: string length '724' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }

                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fcntlmodule.c:428:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fcntlmodule.c:59:31: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                    }
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
            base("grp",
            new Bam.Core.StringArray("Modules/grpmodule"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/grpmodule.c:193:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/grpmodule.c:96:32: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/grpmodule.c:196:1: error: string length '536' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/grpmodule.c:20:6: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/grpmodule.c:96:32: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/grpmodule.c:197:1: error: string literal of length 536 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                })
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
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\selectmodule.c(179): warning C4100: 'self': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4701"); // python-3.5.1\modules\selectmodule.c(260) : warning C4701: potentially uninitialized local variable 'timeout' used
                        compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\selectmodule.c(98) : warning C4706: assignment within conditional expression
                        // VisualC 2015 onwards does not issue C4127 for idiomatic cases such as 1 or true
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                        {
                            compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Parser\myreadline.c(39) : warning C4127: conditional expression is constant
                        }
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/selectmodule.c:179:25: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/selectmodule.c:2345:1: error: string length '991' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/selectmodule.c:2377:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/selectmodule.c:179:25: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/selectmodule.c:2346:1: error: string literal of length 991 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/selectmodule.c:2377:18: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                },
            settings =>
                {
                    if (settings is VisualCCommon.ICommonLinkerSettings)
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
            base("mmap",
            new Bam.Core.StringArray("Modules/mmapmodule"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/mmapmodule.c:730:5: error: missing initializer for field 'ml_doc' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/mmapmodule.c:145:48: error: unused parameter 'unused' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/mmapmodule.c:1009:1: error: string length '1101' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/mmapmodule.c:730:76: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/mmapmodule.c:145:48: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/mmapmodule.c:1010:1: error: string literal of length 1101 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _csv :
        DynamicExtensionModule
    {
        public _csv()
            :
            base("_csv",
            new Bam.Core.StringArray("Modules/_csv"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_csv.c:303:5: error: missing initializer for field 'doc' of 'struct PyMemberDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_csv.c:193:23: error: unused parameter 'name' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_csv.c:1494:1: error: string length '2579' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_csv.c:71:9: error: missing field 'name' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_csv.c:193:23: error: unused parameter 'name' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_csv.c:1495:1: error: string literal of length 2579 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                })
        { }
    }

    class _socket :
        DynamicExtensionModule
    {
        public _socket()
            :
            base("_socket", "Modules/socketmodule", settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("_WINSOCK_DEPRECATED_NO_WARNINGS");
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\socketmodule.c(1134): warning C4100: 'proto': unreferenced formal parameter
                        compiler.DisableWarnings.AddUnique("4245"); // Python-3.5.1\Modules\socketmodule.c(1388): warning C4245: '=': conversion from 'int' to 'std::size_t', signed/unsigned mismatch
                        compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\socketmodule.c(1597): warning C4244: '=': conversion from 'int' to 'ADDRESS_FAMILY', possible loss of data
                        compiler.DisableWarnings.AddUnique("4127"); // Python-3.5.1\Modules\socketmodule.c(2241): warning C4127: conditional expression is constant
                        compiler.DisableWarnings.AddUnique("4232"); // Python-3.5.1\Modules\socketmodule.c(4356): warning C4232: nonstandard extension used: 'tp_getattro': address of dllimport 'PyObject_GenericGetAttr' is not static, identity not guaranteed
                        var compilerUsed = (settings.Module is Bam.Core.IModuleGroup) ?
                            (settings.Module as C.CCompilableModuleContainer<C.ObjectFile>).Compiler :
                            (settings.Module as C.ObjectFile).Compiler;
                        if (compilerUsed.Version.AtMost(VisualCCommon.ToolchainVersion.VC2015))
                        {
                            compiler.DisableWarnings.AddUnique("4996"); // Python-3.5.1\Modules\socketmodule.c(6081) : warning C4996: 'GetVersion': was declared deprecated
                        }
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/socketmodule.c:99:1: error: string length '2087' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/getnameinfo.c:67:5: error: missing initializer for field 'a_off' of 'struct gni_afd' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-but-set-variable"); // Python-3.5.1/Modules/getnameinfo.c:108:9: error: variable 'h_error' set but not used [-Werror=unused-but-set-variable]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/socketmodule.c:1134:74: error: unused parameter 'proto' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Modules/socketmodule.c:882:1: error: 'new_sockobject' defined but not used [-Werror=unused-function]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/socketmodule.c:100:1: error: string literal of length 2087 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/getnameinfo.c:67:13: error: missing field 'a_off' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/socketmodule.c:1134:23: error: unused parameter 'sockfd' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.5.1/Modules/socketmodule.c:882:1: error: 'new_sockobject' defined but not used [-Werror=unused-function]
                    }
                },
                settings =>
                {
                    if (settings is VisualCCommon.ICommonLinkerSettings)
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
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // python-3.5.1\modules\clinic/_ssl.c.h(17): warning C4100: '_unused_ignored': unreferenced formal parameter
                            compiler.DisableWarnings.AddUnique("4152"); // Python-3.5.1\Modules\_ssl.c(298): warning C4152: nonstandard extension, function/data pointer conversion in expression
                            compiler.DisableWarnings.AddUnique("4244"); // Python-3.5.1\Modules\_ssl.c(2496): warning C4244: '=': conversion from 'Py_ssize_t' to 'int', possible loss of data
                            compiler.DisableWarnings.AddUnique("4267"); // Python-3.5.1\Modules\_ssl.c(3630): warning C4267: 'function': conversion from 'size_t' to 'long', possible loss of data
                            compiler.DisableWarnings.AddUnique("4706"); // python-3.5.1\modules\_ssl.c(4192) : warning C4706: assignment within conditional expression
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_ssl_data.h:8:12: error: missing field 'code' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_ssl.c:381:49: error: unused parameter 'filename' [-Werror,-Wunused-parameter]
                            clangCompiler.Pedantic = false; // Python-3.5.1/Modules/_ssl.c:298:17: error: initializing 'void *' with an expression of type 'PyObject *(PyOSErrorObject *)' (aka 'struct _object *(PyOSErrorObject *)') converts between void pointer and function pointer [-Werror,-Wpedantic]
                        }
                        if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/_ssl.c.h:210:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_ssl.c:381:49: error: unused parameter 'filename' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("type-limits"); // Python-3.5.1/Include/pymem.h:93:6: error: comparison is always false due to limited range of data type [-Werror=type-limits]
                            gccCompiler.Pedantic = false; // Python-3.5.1/Modules/_ssl.c:298:5: error: ISO C forbids initialization between function pointer and 'void *' [-Werror=pedantic]
                        }
                    },
                settings =>
                    {
                        if (settings is VisualCCommon.ICommonLinkerSettings vcLinker)
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
        Init()
        {
            base.Init();

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
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4013"); // Python-3.5.1\Modules\_cryptmodule.c(39) : warning C4013: 'crypt' undefined; assuming extern returning int
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/_cryptmodule.c:39:5: error: implicit declaration of function 'crypt' [-Werror=implicit-function-declaration]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_cryptmodule.c:34:31: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_cryptmodule.c:45:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_cryptmodule.c:34:31: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_cryptmodule.c:45:29: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
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
            base("nis",
            new Bam.Core.StringArray("Modules/nismodule"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/nismodule.c:144:35: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/nismodule.c:441:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/nismodule.c:144:35: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/nismodule.c:441:37: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class termios :
        DynamicExtensionModule
    {
        public termios()
            :
            base("termios",
            new Bam.Core.StringArray("Modules/termios"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/termios.c:62:29: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/termios.c:303:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/termios.c:62:29: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/termios.c:303:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class resource :
        DynamicExtensionModule
    {
        public resource()
            :
            base("resource",
            new Bam.Core.StringArray("Modules/resource"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/resource.c:59:30: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/resource.c:287:5: error: missing initializer for field 'ml_doc' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/resource.c:59:30: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/resource.c:45:7: error: missing field 'doc' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // not buildable on Windows
    class _posixsubprocess :
        DynamicExtensionModule
    {
        public _posixsubprocess()
            :
            base("_posixsubprocess",
            new Bam.Core.StringArray("Modules/_posixsubprocess"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_posixsubprocess.c:391:16: error: unused parameter 'call_setsid' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_posixsubprocess.c:782:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_posixsubprocess.c:752:1: error: string length '788' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_posixsubprocess.c:391:16: error: unused parameter 'call_setsid' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_posixsubprocess.c:782:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/_posixsubprocess.c:753:1: error: string literal of length 788 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class audioop :
        DynamicExtensionModule
    {
        public audioop()
            :
            base("audioop",
            new Bam.Core.StringArray("Modules/audioop"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/audioop.c:410:37: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/audioop.c.h:22:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/audioop.c:410:37: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/audioop.c.h:22:37: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _md5 :
        DynamicExtensionModule
    {
        public _md5()
            :
            base("_md5",
            new Bam.Core.StringArray("Modules/md5module"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/md5module.c:432:30: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/md5module.c:428:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/md5module.c:432:30: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/md5module.c:428:23: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha1 :
        DynamicExtensionModule
    {
        public _sha1()
            :
            base("_sha1",
            new Bam.Core.StringArray("Modules/sha1module"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha1module.c:409:31: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha1module.c:405:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha1module.c:409:31: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha1module.c:405:23: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha256 :
        DynamicExtensionModule
    {
        public _sha256()
            :
            base("_sha256",
            new Bam.Core.StringArray("Modules/sha256module"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha256module.c:499:33: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha256module.c:495:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha256module.c:499:33: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha256module.c:495:23: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha512 :
        DynamicExtensionModule
    {
        public _sha512()
            :
            base("_sha512",
            new Bam.Core.StringArray("Modules/sha512module"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha512module.c:570:33: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha512module.c:566:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/sha512module.c:570:33: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/sha512module.c:566:23: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
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
            base("syslog",
            new Bam.Core.StringArray("Modules/syslogmodule"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/syslogmodule.c:113:27: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/syslogmodule.c:243:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/syslogmodule.c:113:27: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/syslogmodule.c:243:93: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses :
        DynamicExtensionModule
    {
        public _curses()
            :
            base("_curses",
                 new Bam.Core.StringArray("Modules/_cursesmodule"),
                 new Bam.Core.StringArray("-lncurses"),
                 settings =>
                 {
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_cursesmodule.c:460:1: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_cursesmodule.c:1973:74: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                    }
                 },
                 null, null, null)
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)] // not buildable on Windows or Linux (by default)
    class _curses_panel :
        DynamicExtensionModule
    {
        public _curses_panel()
            :
            base("_curses_panel",
                 new Bam.Core.StringArray("Modules/_curses_panel"),
                 new Bam.Core.StringArray("-lncurses", "-lpanel"),
                 settings =>
                 {
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_curses_panel.c:394:33: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_curses_panel.c:358:70: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                        clangCompiler.Pedantic = false; // Python-3.5.1/Modules/_curses_panel.c:376:21: error: initializing 'void *' with an expression of type 'void (PyCursesPanelObject *)' converts between void pointer and function pointer [-Werror,-Wpedantic]
                    }
                 },
                 null, null, null)
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
            base("binascii",
            new Bam.Core.StringArray("Modules/binascii"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/binascii.c:256:35: error: unused parameter 'module' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/binascii.c.h:21:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/binascii.c:222:9: error: string length '538' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/binascii.c:256:35: error: unused parameter 'module' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/binascii.c.h:21:33: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class parser :
        DynamicExtensionModule
    {
        public parser()
            :
            base("parser",
            new Bam.Core.StringArray("Modules/parsermodule"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/parsermodule.c:396:38: error: unused parameter 'unused' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/parsermodule.c:260:1: error: missing initializer for field 'tp_members' of 'PyTypeObject' [-Werror=missing-field-initializers]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/parsermodule.c:396:38: error: unused parameter 'unused' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/parsermodule.c:260:1: error: missing field 'tp_members' initializer [-Werror,-Wmissing-field-initializers]
                    }
                })
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
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\fpectlmodule.c(100): warning C4100: 'args': unreferenced formal parameter
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fpectlmodule.c:95:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fpectlmodule.c:100:42: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fpectlmodule.c:95:86: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fpectlmodule.c:100:42: error: unused parameter 'self' [-Werror,-Wunused-parameter]
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
                        if (settings is VisualCCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\Modules\fpetestmodule.c(62): warning C4100: 'args': unreferenced formal parameter
                            if (settings.Module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug)
                            {
                                compiler.DisableWarnings.AddUnique("4723"); // python-3.5.1\modules\fpetestmodule.c(162) : warning C4723: potential divide by 0
                            }
                        }
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fpetestmodule.c:58:5: error: missing initializer for field 'ml_doc' of 'PyMethodDef' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fpetestmodule.c:62:33: error: unused parameter 'self' [-Werror=unused-parameter]
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/fpetestmodule.c:58:70: error: missing field 'ml_doc' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/fpetestmodule.c:62:33: error: unused parameter 'self' [-Werror,-Wunused-parameter]
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
        Init()
        {
            base.Init();

            this.moduleSourceModules.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;

                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        if (this.moduleSourceModules.Compiler.Version.AtLeast(ClangCommon.ToolchainVersion.Xcode_7))
                        {
                            compiler.DisableWarnings.AddUnique("shift-negative-value"); // Python-3.5.1/Modules/zlib/inflate.c:1507:61: error: shifting a negative signed value is undefined [-Werror,-Wshift-negative-value]
                        }
                    }
                });

            this.moduleSourceModules["zlibmodule"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Modules/zlib"));

                        var compiler = settings as C.ICommonCompilerSettings;
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/zlibmodule.c.h:26:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/zlibmodule.c:122:22: error: unused parameter 'ctx' [-Werror=unused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/zlibmodule.c.h:83:1: error: string length '986' is greater than the length '509' ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/clinic/zlibmodule.c.h:26:34: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                            compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/zlibmodule.c:122:22: error: unused parameter 'ctx' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("overlength-strings"); // Python-3.5.1/Modules/clinic/zlibmodule.c.h:84:1: error: string literal of length 986 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                    }));

            this.moduleSourceModules["gzlib"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzlib.c:214:9: error: implicit declaration of function 'snprintf' [-Werror=implicit-function-declaration]
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzlib.c:256:24: error: implicit declaration of function 'lseek' [-Werror,-Wimplicit-function-declaration]
                        }
                    }));

            this.moduleSourceModules["gzread"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzread.c:30:9: error: implicit declaration of function 'read' [-Werror=implicit-function-declaration]
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzread.c:30:15: error: implicit declaration of function 'read' [-Werror,-Wimplicit-function-declaration]
                        }
                    }));

            this.moduleSourceModules["gzwrite"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        if (settings is GccCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzwrite.c:84:9: error: implicit declaration of function 'write' [-Werror=implicit-function-declaration]
                        }
                        if (settings is ClangCommon.ICommonCompilerSettings)
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.DisableWarnings.AddUnique("implicit-function-declaration"); // Python-3.5.1/Modules/zlib/gzwrite.c:84:15: error: implicit declaration of function 'write' [-Werror,-Wimplicit-function-declaration]
                        }
                    }));

            this.moduleSourceModules["infback"].ForEach(item =>
                item.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("implicit-fallthrough"); // TODO: GCC 7+
                    }
                }));

            this.moduleSourceModules["inflate"].ForEach(item =>
                item.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("implicit-fallthrough"); // TODO: GCC 7+
                    }
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
                         var preprocessor = settings as C.ICommonPreprocessorSettings;
                         preprocessor.PreprocessorDefines.Add("HAVE_EXPAT_CONFIG_H");
                         preprocessor.PreprocessorDefines.Add("USE_PYEXPAT_CAPI");
                         preprocessor.SystemIncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/expat"));
                         if (settings is VisualCCommon.ICommonCompilerSettings)
                         {
                             preprocessor.PreprocessorDefines.Add("COMPILED_FROM_DSP"); // to indicate a Windows build
                             preprocessor.PreprocessorDefines.Add("XML_STATIC"); // to avoid unwanted declspecs
                             var compiler = settings as C.ICommonCompilerSettings;
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
                             preprocessor.PreprocessorDefines.Add("HAVE_MEMMOVE", "1");
                         }
                         if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                         {
                             var compiler = settings as C.ICommonCompilerSettings;
                             compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/expat/xmlparse.c:4914:28: error: unused parameter 's' [-Werror=unused-parameter]
                             compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/expat/xmltok.c:471:1: error: missing initializer for field 'isName2' of 'const struct normal_encoding' [-Werror=missing-field-initializers]
                             compiler.DisableWarnings.AddUnique("implicit-fallthrough"); // TODO GCC7+
                             gccCompiler.Pedantic = false; // Python-3.5.1/Modules/pyexpat.c:1362:27: error: ISO C forbids assignment between function pointer and 'void *' [-Werror=pedantic]
                         }
                        if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                        {
                             var compiler = settings as C.ICommonCompilerSettings;
                             compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/expat/xmlparse.c:4914:28: error: unused parameter 's' [-Werror,-Wunused-parameter]
                            compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/expat/xmltok.c:471:1: error: missing field 'isName2' initializer [-Werror,-Wmissing-field-initializers]
                            clangCompiler.Pedantic = false; // Python-3.5.1/Modules/pyexpat.c:1362:27: error: assigning to 'xmlhandler' (aka 'void *') from 'void (void *, const XML_Char *, int)' (aka 'void (void *, const char *, int)') converts between void pointer and function pointer [-Werror,-Wpedantic]
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
            base("_multibytecodec",
            new Bam.Core.StringArray("Modules/cjkcodecs/multibytecodec"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/clinic/multibytecodec.c.h:65:5: error: missing initializer for field 'len' of 'Py_buffer' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/multibytecodec.c:131:27: error: unused parameter 'closure' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/clinic/multibytecodec.c.h:65:34: error: missing field 'len' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/multibytecodec.c:131:27: error: unused parameter 'closure' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_cn :
        DynamicExtensionModule
    {
        public _codecs_cn()
            :
            base("_codecs_cn",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_cn"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_hk :
        DynamicExtensionModule
    {
        public _codecs_hk()
            :
            base("_codecs_hk",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_hk"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_iso2022 :
        DynamicExtensionModule
    {
        public _codecs_iso2022()
            :
            base("_codecs_iso2022",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_iso2022"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("implicit-fallthrough"); // TODO GCC 7+
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_jp :
        DynamicExtensionModule
    {
        public _codecs_jp()
            :
            base("_codecs_jp",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_jp"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_kr :
        DynamicExtensionModule
    {
        public _codecs_kr()
            :
            base("_codecs_kr",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_kr"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _codecs_tw :
        DynamicExtensionModule
    {
        public _codecs_tw()
            :
            base("_codecs_tw",
            new Bam.Core.StringArray("Modules/cjkcodecs/_codecs_tw"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror=unused-parameter]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:300:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/cjkcodecs/cjkcodecs.h:259:20: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                    }
                })
        { }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class xxsubtype :
        DynamicExtensionModule
    {
        public xxsubtype()
            :
            base("xxsubtype",
            new Bam.Core.StringArray("Modules/xxsubtype"),
            settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/xxsubtype.c:79:5: error: missing initializer for field 'ml_flags' of 'PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/xxsubtype.c:236:22: error: unused parameter 'self' [-Werror=unused-parameter]
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/xxsubtype.c:293:5: error: ISO C forbids initialization between function pointer and 'void *' [-Werror=pedantic]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/xxsubtype.c:79:21: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/xxsubtype.c:236:22: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        clangCompiler.Pedantic = false; //Python-3.5.1/Modules/xxsubtype.c:293:19: error: initializing 'void *' with an expression of type 'int (PyObject *)' (aka 'int (struct _object *)') converts between void pointer and function pointer [-Werror,-Wpedantic]
                    }
                })
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
                "Modules/_ctypes/stgdict"),
            null,
            settings =>
                {
                    if (settings is VisualCCommon.ICommonCompilerSettings)
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.AddUnique(settings.Module.CreateTokenizedString("$(packagedir)/Modules/_ctypes/libffi_msvc"));
                        var compiler = settings as C.ICommonCompilerSettings;
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
                        var module = settings.Module as C.CModule;
                        if (module.BitDepth == C.EBit.ThirtyTwo)
                        {
                            compiler.DisableWarnings.AddUnique("4389"); // Python-3.5.1\Modules\_ctypes\cfield.c(1447): warning C4389: '!=': signed/unsigned mismatch
                        }
                    }
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_ctypes/_ctypes.c:155:47: error: unused parameter 'args' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_ctypes/_ctypes.c:210:1: error: missing initializer for field 'tp_is_gc' of 'PyTypeObject' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("format"); // Python-3.5.1/Modules/_ctypes/_ctypes.c:321:17: error: ISO C90 does not support the 'z' gnu_printf length modifier [-Werror=format=]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.6.1/Modules/_ctypes/cfield.c:715:1: error: 'bool_set' defined but not used [-Werror=unused-function]
                        compiler.DisableWarnings.AddUnique("implicit-fallthrough"); // TODO: Gcc7+
                        gccCompiler.Pedantic = false; // Python-3.5.1/Modules/_ctypes/_ctypes.c:3298:15: error: ISO C forbids conversion of object pointer to function pointer type [-Werror=pedantic]
                    }
                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.5.1/Modules/_ctypes/_ctypes.c:155:47: error: unused parameter 'args' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.5.1/Modules/_ctypes/_ctypes.c:210:1: error: missing field 'tp_is_gc' initializer [-Werror,-Wmissing-field-initializers]
                        clangCompiler.Pedantic = false; // Python-3.5.1/Modules/_ctypes/_ctypes.c:3324:27: error: assigning to 'void *' from 'int (*)(void)' converts between void pointer and function pointer [-Werror,-Wpedantic]
                    }
                },
            settings =>
                {
                    if (settings is VisualCCommon.ICommonLinkerSettings)
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("Ole32.lib");
                        linker.Libraries.AddUnique("OleAut32.lib");
                    }
                },
            null,
            null
            )
        { }

        protected override void
        Init()
        {
            base.Init();

            this.CompileAndLinkAgainst<ffi>(this.moduleSourceModules);
        }
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
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("tautological-pointer-compare"); // Python-3.5.1/Modules/_scproxy.c:74:10: error: comparison of address of 'kSCPropNetProxiesExcludeSimpleHostnames' not equal to a null pointer is always true [-Werror,-Wtautological-pointer-compare]
                    }
                },
            settings =>
                {
                    if (settings is C.ICommonLinkerSettingsOSX osxLinker)
                    {
                        osxLinker.Frameworks.AddUnique("SystemConfiguration");
                        osxLinker.Frameworks.AddUnique("CoreFoundation");
                    }
                })
        {}
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _blake2 :
        DynamicExtensionModule
    {
        public _blake2() :
            base("_blake2",
                 new Bam.Core.StringArray("Modules/_blake2/blake2module", "Modules/_blake2/blake2b_impl", "Modules/_blake2/blake2s_impl"),
                 settings =>
                 {
                    if (settings is ClangCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.6.1/Modules/_blake2/blake2module.c:25:16: error: missing field 'ml_flags' initializer [-Werror,-Wmissing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/_blake2/blake2b_impl.c:370:36: error: unused parameter 'self' [-Werror,-Wunused-parameter]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.6.1/Modules/_blake2/impl/blake2-impl.h:22:31: error: unused function 'load32' [-Werror,-Wunused-function]
                    }
                    if (settings is GccCommon.ICommonCompilerSettings)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.6.1/Modules/_blake2/blake2module.c:25:5: error: missing initializer for field 'ml_flags' of 'struct PyMethodDef' [-Werror=missing-field-initializers]
                        compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/_blake2/blake2b_impl.c:370:36: error: unused parameter 'self' [-Werror=unused-parameter]
                        compiler.DisableWarnings.AddUnique("unused-function"); // Python-3.6.1/Modules/_blake2/impl/blake2-impl.h:22:31: error: 'load32' defined but not used [-Werror=unused-function]
                    }
                 })
        {}
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)] // Windows builtin
    class _sha3 :
        DynamicExtensionModule
    {
        public _sha3() :
            base("_sha3",
                 new Bam.Core.StringArray("Modules/_sha3/sha3module"),
                 settings =>
                 {
                     if (settings is ClangCommon.ICommonCompilerSettings)
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.6.1/Modules/_sha3/clinic/sha3module.c.h:19:64: error: missing field 'custom_msg' initializer [-Werror,-Wmissing-field-initializers]
                         compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/_sha3/sha3module.c:421:45: error: unused parameter 'closure' [-Werror,-Wunused-parameter]
                     }
                     if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                     {
                         var compiler = settings as C.ICommonCompilerSettings;
                         compiler.DisableWarnings.AddUnique("missing-field-initializers"); // Python-3.6.1/Modules/_sha3/clinic/sha3module.c.h:19:5: error: missing initializer for field 'custom_msg' of '_PyArg_Parser' [-Werror=missing-field-initializers]
                         compiler.DisableWarnings.AddUnique("unused-parameter"); // Python-3.6.1/Modules/_sha3/sha3module.c:421:45: error: unused parameter 'closure' [-Werror=unused-parameter]
                         var module = settings.Module as C.CModule;
                         if (module.BitDepth == C.EBit.ThirtyTwo)
                         {
                             if (module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug)
                             {
                                 gccCompiler.StrictAliasing = false; // Python-3.6.1/Modules/_sha3/kcp/KeccakP-1600-inplace32BI.c:97:5: error: dereferencing type-punned pointer will break strict-aliasing rules [-Werror=strict-aliasing]
                             }
                         }
                     }
                 })
        { }
    }
}
