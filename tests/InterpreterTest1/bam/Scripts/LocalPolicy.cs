using Bam.Core;
namespace InterpreterTest1
{
    public sealed class LocalPolicy :
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
}
