using Bam.Core;
namespace Python
{
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
}
