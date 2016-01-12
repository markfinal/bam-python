using Bam.Core;
namespace InterpreterTest1
{
    sealed class CTest :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");
            source.PrivatePatch(settings =>
                {
                    var winCompiler = settings as C.ICommonCompilerSettingsWin;
                    if (null != winCompiler)
                    {
                        winCompiler.CharacterSet = C.ECharacterSet.Unicode;
                    }
                });

            this.CompileAndLinkAgainst<Python.PythonLibrary>(source);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }

            this.PrivatePatch(settings =>
                {
                    var gccCommon = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccCommon)
                    {
                        gccCommon.CanUseOrigin = true;
                        gccCommon.RPath.AddUnique("$ORIGIN");
                    }
                });
        }
    }

    sealed class CTestRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<CTest>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            var pyLibCopy = this.Include<Python.PythonLibrary>(C.DynamicLibrary.Key, ".", app);
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, ".", app);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, "lib", app);
                platIndependentModules.CopiedFilename = "python3.5";
                this.Include<Python.SysConfigDataPythonFile>(Python.SysConfigDataPythonFile.Key, "lib/python3.5", app);

                var timeModule = this.Include<Python.TimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                timeModule.DependsOn(platIndependentModules);
            }
        }
    }
}
