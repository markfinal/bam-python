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
                    }
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }

            this.LinkAgainst<PythonLibrary>();
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
                    }
                });

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
                    }
                });

            var pythonSource = this.CreateCSourceContainer("$(packagedir)/Python/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup)(?!.*frozenmain)(?!.*sigcheck).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
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
                    }
                });

            var moduleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            moduleSource.AddFiles("$(packagedir)/Modules/getbuildinfo.c");
            //moduleSource.AddFiles("$(packagedir)/Modules/config.c");
            //moduleSource.AddFiles("$(packagedir)/Modules/getpath.c");

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
                moduleSource.AddFiles("$(packagedir)/Modules/_winapi.c");
            }

            moduleSource.PrivatePatch(settings =>
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
                    }
                });

            moduleSource.Children.Where(item => item.InputPath.Parse().Contains("zlibmodule.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/Modules/zlib")); // for zlib.h
                    }));

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
            }
        }
    }
}
