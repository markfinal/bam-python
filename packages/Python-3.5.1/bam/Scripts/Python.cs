using Bam.Core;
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
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }
                });

            var pythonSource = this.CreateCSourceContainer("$(packagedir)/Python/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*dynload_)(?!.*dup2)(?!.*strdup).*)$"));
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                pythonSource.AddFiles("$(packagedir)/Python/dynload_win.c");
            }
            pythonSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }
                });

            var moduleSource = this.CreateCSourceContainer("$(packagedir)/Modules/main.c");
            //moduleSource.AddFiles("$(packagedir)/Modules/config.c");
            //moduleSource.AddFiles("$(packagedir)/Modules/getpath.c");
            moduleSource.AddFiles("$(packagedir)/Modules/gcmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/faulthandler.c");
            moduleSource.AddFiles("$(packagedir)/Modules/hashtable.c");
            moduleSource.AddFiles("$(packagedir)/Modules/signalmodule.c");
            moduleSource.AddFiles("$(packagedir)/Modules/_tracemalloc.c");
            moduleSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }
                });

            var signalSource = this.CreateCSourceContainer("$(packagedir)/Python/sigcheck.c");
            signalSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                    compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/PC"));
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var pcSource = this.CreateCSourceContainer("$(packagedir)/PC/dl_nt.c");
                pcSource.AddFiles("$(packagedir)/PC/config.c");
                pcSource.AddFiles("$(packagedir)/PC/frozen_dllmain.c");
                pcSource.AddFiles("$(packagedir)/PC/getpathp.c");
                pcSource.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("Py_BUILD_CORE");
                        compiler.PreprocessorDefines.Add("Py_ENABLE_SHARED");
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/Include"));
                    });
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(parserSource);
            }
        }
    }
}
