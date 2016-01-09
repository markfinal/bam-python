using Bam.Core;
using System.Linq;
namespace Python
{
    sealed class PythonInterpreter :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");

            var source = this.CreateCSourceContainer("$(packagedir)/Programs/python.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
            else
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                source.DependsOn(pyConfigHeader);
                source.UsePublicPatches(pyConfigHeader);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.PrivatePatch(settings =>
                        {
                            var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                            gccLinker.CanUseOrigin = true;
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        });
                }
            }

            this.LinkAgainst<PythonLibrary>();
        }
    }

    class PyConfigHeader :
        C.CModule
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("PyConfig header");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(config)/pyconfig.h"));

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                    }
                });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.WriteLine("#ifndef PYCONFIG_H");
                writeFile.WriteLine("#define PYCONFIG_H");
                writeFile.WriteLine("#define _BSD_SOURCE 1");
                writeFile.WriteLine("#include <limits.h>"); // so that __USE_POSIX is not undeffed
                writeFile.WriteLine("#define __USE_POSIX 1");
                writeFile.WriteLine("#define __USE_POSIX199309 1");
                writeFile.WriteLine("#define HAVE_STDINT_H");
                writeFile.WriteLine("#define HAVE_SYS_TIME_H");
                writeFile.WriteLine("#define HAVE_SYS_STAT_H");
                writeFile.WriteLine("#define HAVE_LONG_LONG 1"); // required to have a value in Modules/arraymodule.c
                writeFile.WriteLine("#define HAVE_STRING_H");
                writeFile.WriteLine("#define HAVE_ERRNO_H");
                writeFile.WriteLine("#define HAVE_LSTAT");
                writeFile.WriteLine("#define PY_INT64_T PY_LONG_LONG");
                writeFile.WriteLine("#define PY_FORMAT_LONG_LONG \"ll\"");
                writeFile.WriteLine("#define PY_FORMAT_SIZE_T \"z\"");
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    writeFile.WriteLine("#define SIZEOF_WCHAR_T 4");
                }
                else
                {
                    writeFile.WriteLine("#define SIZEOF_WCHAR_T 2");
                }
                writeFile.WriteLine("#define SIZEOF_LONG 8");
                writeFile.WriteLine("#define SIZEOF_LONG_LONG 8");
                writeFile.WriteLine("#define SIZEOF_INT 4");
                writeFile.WriteLine("#define SIZEOF_SHORT 2");
                writeFile.WriteLine("#define SIZEOF_VOID_P 8");
                writeFile.WriteLine("#define SIZEOF_SIZE_T 8");
                writeFile.WriteLine("#define SIZEOF_OFF_T 8");
                writeFile.WriteLine("#define VA_LIST_IS_ARRAY 1");
                writeFile.WriteLine("#define HAVE_STDARG_PROTOTYPES");
                writeFile.WriteLine("#define HAVE_UINTPTR_T");
                writeFile.WriteLine("#define HAVE_WCHAR_H");
                writeFile.WriteLine("#define HAVE_UINT32_T");
                writeFile.WriteLine("#define HAVE_INT32_T");
                writeFile.WriteLine("#define HAVE_FCNTL_H");
                writeFile.WriteLine("#define HAVE_UNISTD_H");
                writeFile.WriteLine("#define HAVE_SIGNAL_H");
                writeFile.WriteLine("#define TIME_WITH_SYS_TIME");
                writeFile.WriteLine("#define HAVE_DIRENT_H");
                writeFile.WriteLine("#define HAVE_CLOCK");
                writeFile.WriteLine("#define HAVE_GETTIMEOFDAY");
                writeFile.WriteLine("#define WITH_THREAD");
                writeFile.WriteLine("#define WITH_PYMALLOC");
                writeFile.WriteLine("#define HAVE_SYSCONF"); // or my_getallocationgranularity is undefined
                writeFile.WriteLine("#define PyAPI_FUNC(RTYPE) __attribute__ ((visibility(\"default\"))) RTYPE");
                writeFile.WriteLine("#define PyAPI_DATA(RTYPE) extern __attribute__ ((visibility(\"default\"))) RTYPE");
                writeFile.WriteLine("#ifdef Py_BUILD_CORE");
                writeFile.WriteLine("#define PyMODINIT_FUNC PyObject*");
                writeFile.WriteLine("#else");
                writeFile.WriteLine("#define PyMODINIT_FUNC extern __attribute__ ((visibility(\"default\"))) PyObject*");
                writeFile.WriteLine("#endif");
                writeFile.WriteLine("#define HAVE_DYNAMIC_LOADING");
                writeFile.WriteLine("#define SOABI \"cpython-35\"");
                writeFile.WriteLine("#define HAVE_DLFCN_H");
                writeFile.WriteLine("#define HAVE_DLOPEN");
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    writeFile.WriteLine("#define HAVE_FSTATVFS");
                    writeFile.WriteLine("#define HAVE_SYS_STATVFS_H");
                }
                else
                {
                    writeFile.WriteLine("#define HAVE_CLOCK_GETTIME");
                    writeFile.WriteLine("#define daylight __daylight");
                    writeFile.WriteLine("#define HAVE_LANGINFO_H"); // defines CODESET
                }
                writeFile.WriteLine("#endif");
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }

    class ConfigSource :
        C.SourceFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths[Key] = this.CreateTokenizedString("$(packagebuilddir)/$(config)/config.c");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                    }
                });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }

        private void
        insertBuiltinModules(
            ref string configText)
        {
            var declarations = new System.Text.StringBuilder();
            var inittab = new System.Text.StringBuilder();

            declarations.AppendLine("extern PyObject* PyInit_posix(void);");
            inittab.AppendLine("\t{\"posix\", PyInit_posix},");

            declarations.AppendLine("extern PyObject* PyInit_errno(void);");
            inittab.AppendLine("\t{\"errno\", PyInit_errno},");

            declarations.AppendLine("extern PyObject* PyInit_pwd(void);");
            inittab.AppendLine("\t{\"pwd\", PyInit_pwd},");

            declarations.AppendLine("extern PyObject* PyInit__sre(void);");
            inittab.AppendLine("\t{\"_sre\", PyInit__sre},");

            declarations.AppendLine("extern PyObject* PyInit__codecs(void);");
            inittab.AppendLine("\t{\"_codecs\", PyInit__codecs},");

            declarations.AppendLine("extern PyObject* PyInit__weakref(void);");
            inittab.AppendLine("\t{\"_weakref\", PyInit__weakref},");

            declarations.AppendLine("extern PyObject* PyInit__functools(void);");
            inittab.AppendLine("\t{\"_functools\", PyInit__functools},");

            declarations.AppendLine("extern PyObject* PyInit__operator(void);");
            inittab.AppendLine("\t{\"_operator\", PyInit__operator},");

            declarations.AppendLine("extern PyObject* PyInit__collections(void);");
            inittab.AppendLine("\t{\"_collections\", PyInit__collections},");

            declarations.AppendLine("extern PyObject* PyInit_itertools(void);");
            inittab.AppendLine("\t{\"itertools\", PyInit_itertools},");

            declarations.AppendLine("extern PyObject* PyInit_atexit(void);");
            inittab.AppendLine("\t{\"atexit\", PyInit_atexit},");

            declarations.AppendLine("extern PyObject* PyInit__stat(void);");
            inittab.AppendLine("\t{\"_stat\", PyInit__stat},");

            declarations.AppendLine("extern PyObject* PyInit_time(void);");
            inittab.AppendLine("\t{\"time\", PyInit_time},");

            declarations.AppendLine("extern PyObject* PyInit__locale(void);");
            inittab.AppendLine("\t{\"_locale\", PyInit__locale},");

            declarations.AppendLine("extern PyObject* PyInit__io(void);");
            inittab.AppendLine("\t{\"_io\", PyInit__io},");

            declarations.AppendLine("extern PyObject* PyInit_zipimport(void);");
            inittab.AppendLine("\t{\"zipimport\", PyInit_zipimport},");

            declarations.AppendLine("extern PyObject* PyInit_faulthandler(void);");
            inittab.AppendLine("\t{\"defaulthandler\", PyInit_faulthandler},");

            declarations.AppendLine("extern PyObject* PyInit__tracemalloc(void);");
            inittab.AppendLine("\t{\"_tracemalloc\", PyInit__tracemalloc},");

            declarations.AppendLine("extern PyObject* PyInit__symtable(void);");
            inittab.AppendLine("\t{\"_symtable\", PyInit__symtable},");

            declarations.AppendLine("extern PyObject* PyInit__signal(void);");
            inittab.AppendLine("\t{\"_signal\", PyInit__signal},");

            configText = configText.Replace("/* -- ADDMODULE MARKER 1 -- */",
                declarations.ToString());
            configText = configText.Replace("/* -- ADDMODULE MARKER 2 -- */",
                inittab.ToString());
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            var stubPath = this.CreateTokenizedString("$(packagedir)/Modules/config.c.in").Parse();
            var stubText = System.IO.File.ReadAllText(stubPath);
            insertBuiltinModules(ref stubText);
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.Write(stubText);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }

    class SysConfigDataPythonFile :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("_sysconfigdata Python file");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(config)/_sysconfigdata.py"));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.WriteLine("build_time_vars = {}");
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }

    sealed class PythonLibrary :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("3");
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("5");
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim("1");

            var headers = this.CreateHeaderContainer("$(packagedir)/Include/*.h");

            var parserSource = this.CreateCSourceContainer("$(packagedir)/Parser/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$"));
            parserSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });
            headers.AddFiles("$(packagedir)/Parser/*.h");

            var objectSource = this.CreateCSourceContainer("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });

            objectSource.Children.Where(item => item.InputPath.Parse().Contains("bytesobject.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));
            objectSource.Children.Where(item => item.InputPath.Parse().Contains("odictobject.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));
            headers.AddFiles("$(packagedir)/Objects/*.h");

            var pythonSource = this.CreateCSourceContainer("$(packagedir)/Python/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup)(?!.*frozenmain)(?!.*sigcheck).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
            }
            else
            {
                // don't use dynload_next, as it's for older OSX (10.2 or below)
                pythonSource.AddFiles("$(packagedir)/Python/dynload_shlib.c");
            }
            pythonSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });

            pythonSource.Children.Where(item => item.InputPath.Parse().Contains("_warnings.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICOnlyCompilerSettings;
                        compiler.LanguageStandard = C.ELanguageStandard.C99; // because of C++ style comments
                    }));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                pythonSource.Children.Where(item => item.InputPath.Parse().Contains("dtoa.c")).ToList().ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("parentheses"); // if (y = value) type expression
                    }));
                pythonSource.Children.Where(item => item.InputPath.Parse().Contains("pytime.c")).ToList().ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("tautological-constant-out-of-range-compare"); // numbers out of range of comparison
                    }));
            }
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.NotWindows))
            {
                // TODO: I cannot see how else some symbols are exported with preprocessor settings
                pythonSource.Children.Where(item => item.InputPath.Parse().Contains("getargs.c")).ToList().ForEach(item =>
                    item.PrivatePatch(settings =>
                    {
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Visibility = GccCommon.EVisibility.Default;
                        }

                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            clangCompiler.Visibility = ClangCommon.EVisibility.Default;
                        }
                    }));
            }
            headers.AddFiles("$(packagedir)/Python/*.h");

            var builtinModuleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                // Windows builds includes many more modules in the core library
                builtinModuleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_struct.c");
            }

            builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/audioop.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/binascii.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/md5module.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));

            builtinModuleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_csv.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_io/*.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_sre.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_stat.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_weakref.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
                builtinModuleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
            }
            else
            {
                builtinModuleSource.AddFiles("$(packagedir)/Modules/getpath.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/pwdmodule.c");

                var configSource = Bam.Core.Graph.Instance.FindReferencedModule<ConfigSource>();
                builtinModuleSource.AddFile(configSource);

                builtinModuleSource.Children.Where(item => item.InputPath.Parse().Contains("getpath.c")).ToList().ForEach(item =>
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.PreprocessorDefines.Add("VERSION", "\"3.5\"");
                            compiler.PreprocessorDefines.Add("PYTHONPATH", "\".\"");
                        }));
            }

            builtinModuleSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });

            builtinModuleSource.Children.Where(item => item.InputPath.Parse().Contains("zlibmodule.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
                    }));
            headers.AddFiles("$(packagedir)/Modules/*.h");
            headers.AddFiles("$(packagedir)/Modules/cjkcodecs/*.h");
            headers.AddFiles("$(packagedir)/Modules/zlib/*.h");
            headers.AddFiles("$(packagedir)/Modules/_io/*.h");

#if false
            // sigcheck has a simplified error check compared to signalmodule
            var signalSource = this.CreateCSourceContainer("$(packagedir)/Python/sigcheck.c");
            signalSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                    {
                        compiler.PreprocessorDefines.Add("Py_DEBUG");
                    }
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        compiler.PreprocessorDefines.Add("WIN32");
                    }
                });
#endif

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceContainer("$(packagedir)/PC/dl_nt.c");
                pcSource.AddFiles("$(packagedir)/PC/config.c");
                //pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                pcSource.AddFiles("$(packagedir)/PC/winreg.c");
                pcSource.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                        {
                            compiler.PreprocessorDefines.Add("Py_DEBUG");
                        }
                        compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                        compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                        compiler.PreprocessorDefines.Add("WIN32");
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    });
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(parserSource);
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("Advapi32.lib");
                        linker.Libraries.Add("Ws2_32.lib");
                        linker.Libraries.Add("User32.lib");
                    });
                headers.AddFiles("$(packagedir)/PC/*.h");
            }
            else
            {
                // TODO: is there a call for a CompileWith function?
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                this.DependsOn(pyConfigHeader);
                this.UsePublicPatches(pyConfigHeader);
                // TODO: end of function

                var sysConfigDataPy = Bam.Core.Graph.Instance.FindReferencedModule<SysConfigDataPythonFile>();
                this.Requires(sysConfigDataPy);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-lpthread");
                        linker.Libraries.Add("-lm");
                        linker.Libraries.Add("-ldl");
                    });

                // TODO: would like to do this, but can't, see bug#101
                //headers.AddFile(pyConfigHeader);
                headers.AddFile(pyConfigHeader.GeneratedPaths[PyConfigHeader.Key].Parse());
            }
        }
    }

    class PythonExtensionModule :
        C.Plugin
    {
        protected string ModuleName;
        protected Bam.Core.StringArray SourceFiles;
        protected Bam.Core.StringArray Libraries;

        protected PythonExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles,
            Bam.Core.StringArray libraries)
        {
            this.ModuleName = moduleName;
            this.SourceFiles = sourceFiles;
            this.Libraries = libraries;
        }

        protected PythonExtensionModule(
            string moduleName,
            Bam.Core.StringArray sourceFiles)
            :
            this(moduleName, sourceFiles, null)
        {}

        protected PythonExtensionModule(
            string moduleName,
            string sourceFile)
            :
            this(moduleName, new Bam.Core.StringArray(sourceFile), null)
        {}

        protected PythonExtensionModule(
            string moduleName)
            :
            this(moduleName, new Bam.Core.StringArray(moduleName), null)
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
                if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                {
                    compiler.PreprocessorDefines.Add("Py_DEBUG");
                }
                compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    compiler.PreprocessorDefines.Add("WIN32");
                }
            });

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

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)]
    sealed class StructModule :
        PythonExtensionModule
    {
        public StructModule()
            :
            base("_struct")
        {}
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)]
    sealed class ArrayModule :
        PythonExtensionModule
    {
        public ArrayModule()
            :
            base("array", "arraymodule")
        {}
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)]
    sealed class CMathModule :
        PythonExtensionModule
    {
        public CMathModule()
            :
            base("cmath", new Bam.Core.StringArray("cmathmodule", "_math"))
        {}
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.NotWindows)]
    sealed class MathModule :
        PythonExtensionModule
    {
        public MathModule()
            :
            base("math", new Bam.Core.StringArray("mathmodule", "_math"))
        {}
    }

    sealed class PythonRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<PythonInterpreter>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<PythonLibrary>(C.DynamicLibrary.Key, ".", app);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var platIndependentModules = this.IncludeDirectory(this.CreateTokenizedString("$(packagedir)/Lib"), ".", app);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                var platIndependentModules = this.IncludeDirectory(this.CreateTokenizedString("$(packagedir)/Lib"), "lib", app);
                platIndependentModules.CopiedFilename = "python3.5";

                this.Include<SysConfigDataPythonFile>(SysConfigDataPythonFile.Key, "lib/python3.5", app);

                // extension modules
                var structModule = this.Include<StructModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                structModule.DependsOn(platIndependentModules);

                var arrayModule = this.Include<ArrayModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                arrayModule.DependsOn(platIndependentModules);

                var cmathModule = this.Include<CMathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                cmathModule.DependsOn(platIndependentModules);

                var mathModule = this.Include<MathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                mathModule.DependsOn(platIndependentModules);
            }
        }
    }
}
