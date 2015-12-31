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
                    compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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
        private static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("PyConfig header");

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
                writeFile.WriteLine("#define PY_INT64_T PY_LONG_LONG");
                writeFile.WriteLine("#define PY_FORMAT_LONG_LONG \"ll\"");
                writeFile.WriteLine("#define PY_FORMAT_SIZE_T \"z\"");
                writeFile.WriteLine("#define SIZEOF_WCHAR_T 2");
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
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    writeFile.WriteLine("#define HAVE_FSTATVFS");
                    writeFile.WriteLine("#define HAVE_SYS_STATVFS_H");
                }
                else
                {
                    writeFile.WriteLine("#define HAVE_CLOCK_GETTIME");
                    writeFile.WriteLine("#define daylight __daylight");
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
                writeFile.WriteLine("#include \"Python.h\"");
                writeFile.WriteLine("struct _inittab _PyImport_Inittab[] = {");
                writeFile.WriteLine("\t/* Sentinel */");
                writeFile.WriteLine("\t{0, 0}");
                writeFile.WriteLine("};");
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
                    compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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
                    compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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
                //pythonSource.AddFiles("$(packagedir)/Python/strdup.c");
            }
            pythonSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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
            headers.AddFiles("$(packagedir)/Python/*.h");

            var moduleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            moduleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");

            moduleSource.AddFiles("$(packagedir)/Modules/arraymodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/audioop.c");
            moduleSource.AddFiles("$(packagedir)/Modules/binascii.c");
            moduleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c");
            moduleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            moduleSource.AddFiles("$(packagedir)/Modules/hashtable.c");
            moduleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/md5module.c");
            moduleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/posixmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c");
            moduleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
            moduleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
            moduleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
            moduleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");
            moduleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            moduleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));

            moduleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_csv.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_io/*.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_json.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_math.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_operator.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_sre.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_stat.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_struct.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_threadmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_weakref.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                moduleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
                moduleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
                moduleSource.AddFiles("$(packagedir)/PC/msvcrtmodule.c");
            }
            else
            {
                moduleSource.AddFiles("$(packagedir)/Modules/getpath.c");

                var configSource = Bam.Core.Graph.Instance.FindReferencedModule<ConfigSource>();
                moduleSource.AddFile(configSource);
            }

            moduleSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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

            moduleSource.Children.Where(item => item.InputPath.Parse().Contains("zlibmodule.c")).ToList().ForEach(item =>
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
                        compiler.PreprocessorDefines.Add("VERSION", "\\\"3.5\\\"");
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
                parserSource.DependsOn(pyConfigHeader);
                parserSource.UsePublicPatches(pyConfigHeader);
                objectSource.DependsOn(pyConfigHeader);
                objectSource.UsePublicPatches(pyConfigHeader);
                pythonSource.DependsOn(pyConfigHeader);
                pythonSource.UsePublicPatches(pyConfigHeader);
                moduleSource.DependsOn(pyConfigHeader);
                moduleSource.UsePublicPatches(pyConfigHeader);
                // TODO: end of function
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-lpthread");
                        linker.Libraries.Add("-lm");
                    });
            }
        }
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
        }
    }
}
