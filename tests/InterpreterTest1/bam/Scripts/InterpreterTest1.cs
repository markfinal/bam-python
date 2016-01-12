using Bam.Core;
namespace InterpreterTest1
{
    public sealed class PatchworksPolicy :
        Bam.Core.ISitePolicy
    {
        void
        Bam.Core.ISitePolicy.DefineLocalSettings(
            Bam.Core.Settings settings,
            Bam.Core.Module module)
        {
            var winCompiler = settings as C.ICommonCompilerSettingsWin;
            if (null != winCompiler)
            {
                winCompiler.CharacterSet = C.ECharacterSet.Unicode;
            }
        }
    }

    sealed class CTest :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");

            this.CompileAndLinkAgainst<Python.PythonLibrary>(source);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
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
            }
        }
    }
}
