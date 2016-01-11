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
        }
    }
}
