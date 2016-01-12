using Bam.Core;
namespace Python
{
    class ModuleConfigSourceFile :
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

            // TODO: should be builtin?
            //declarations.AppendLine("extern PyObject* PyInit_pwd(void);");
            //inittab.AppendLine("\t{\"pwd\", PyInit_pwd},");

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

            // TODO: should be builtin?
            //declarations.AppendLine("extern PyObject* PyInit_atexit(void);");
            //inittab.AppendLine("\t{\"atexit\", PyInit_atexit},");

            declarations.AppendLine("extern PyObject* PyInit__stat(void);");
            inittab.AppendLine("\t{\"_stat\", PyInit__stat},");

            // TODO: should be builtin?
            //declarations.AppendLine("extern PyObject* PyInit_time(void);");
            //inittab.AppendLine("\t{\"time\", PyInit_time},");

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
            Bam.Core.Log.MessageAll("Written '{0}'", destPath);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }
}
