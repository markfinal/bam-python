using Bam.Core;
using System.Linq;
namespace Python
{
    sealed class PythonLibrary :
        C.DynamicLibrary
    {
        public Bam.Core.TokenizedString
        LibraryDirectory
        {
            get
            {
                return this.Macros["PythonLibDirectory"];
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python35");
            }
            else
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");
            }
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("3");
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("5");
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim("1");

            this.Macros["PythonLibDirectory"] = this.CreateTokenizedString("$(packagedir)/Lib");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        if (Bam.Core.EConfiguration.Debug == this.BuildEnvironment.Configuration)
                        {
                            compiler.PreprocessorDefines.Add("Py_DEBUG");
                        }
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                        }
                    }
                });

            var headers = this.CreateHeaderContainer("$(packagedir)/Include/*.h");

            var parserSource = this.CreateCSourceContainer("$(packagedir)/Parser/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*pgen).*)$"));
            parserSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                    }
                });
            headers.AddFiles("$(packagedir)/Parser/*.h");

            var objectSource = this.CreateCSourceContainer("$(packagedir)/Objects/*.c");
            objectSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
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
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
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
                builtinModuleSource.AddFiles("$(packagedir)/Modules/atexitmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/audioop.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/cmathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/mathmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/md5module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/mmapmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/parsermodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/rotatingtree.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha1module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha256module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/sha512module.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/socketmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/timemodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/unicodedata.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_bisectmodule.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_cryptmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_csv.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_datetimemodule.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_hashopenssl.c"); // needs OpenSSL
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_heapqmodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_json.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_lsprof.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_math.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_opcode.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_pickle.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_randommodule.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_ssl.c"); // needs OpenSSL
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_struct.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testbuffer.c");
                //builtinModuleSource.AddFiles("$(packagedir)/Modules/_testcapimodule.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testimportmultiple.c");
                builtinModuleSource.AddFiles("$(packagedir)/Modules/_testmultiphase.c");
            }

            builtinModuleSource.AddFiles("$(packagedir)/Modules/binascii.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/cjkcodecs/*.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/errnomodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/hashtable.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/itertoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/posixmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/symtablemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/xxsubtype.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zipimport.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zlibmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/zlib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*example)(?!.*minigzip).*)$"));

            builtinModuleSource.AddFiles("$(packagedir)/Modules/_codecsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_collectionsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_functoolsmodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_io/*.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_localemodule.c");
            builtinModuleSource.AddFiles("$(packagedir)/Modules/_operator.c");
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

                var ModuleConfigSourceFile = Bam.Core.Graph.Instance.FindReferencedModule<ModuleConfigSourceFile>();
                builtinModuleSource.AddFile(ModuleConfigSourceFile);

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
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
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
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
                    }
                });
#endif

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceContainer("$(packagedir)/PC/dl_nt.c");
                var pcConfig = pcSource.AddFiles("$(packagedir)/PC/config.c");
                pcConfig[0].PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("WIN32"); // required to register two extension modules
                    });
                //pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                pcSource.AddFiles("$(packagedir)/PC/winreg.c");
                pcSource.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                        compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                        var winCompiler = settings as C.ICommonCompilerSettingsWin;
                        winCompiler.CharacterSet = C.ECharacterSet.NotSet;
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
                this.UsePublicPatches(pyConfigHeader);
                parserSource.DependsOn(pyConfigHeader);
                objectSource.DependsOn(pyConfigHeader);
                pythonSource.DependsOn(pyConfigHeader);
                builtinModuleSource.DependsOn(pyConfigHeader);
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
}