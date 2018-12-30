#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class ModuleConfigSourceFile :
        C.SourceFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(
                SourceFileKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(config)/config.c")
            );
            this.Macros.Add(
                "templateConfig",
                this.CreateTokenizedString("$(packagedir)/Modules/config.c.in")
            );

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                    }
                });
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[SourceFileKey].ToString();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(
                    this.GeneratedPaths[SourceFileKey]
                );
                return;
            }
        }

        private void
        InsertBuiltinModules(
            ref string configText)
        {
            var declarations = new System.Text.StringBuilder();
            var inittab = new System.Text.StringBuilder();

            declarations.AppendLine("extern PyObject* PyInit__signal(void);"); // required by library calls
            inittab.AppendLine("\t{\"_signal\", PyInit__signal},");

            declarations.AppendLine("extern PyObject* PyInit_posix(void);");
            inittab.AppendLine("\t{\"posix\", PyInit_posix},");

            declarations.AppendLine("extern PyObject* PyInit_errno(void);");
            inittab.AppendLine("\t{\"errno\", PyInit_errno},");

            declarations.AppendLine("extern PyObject* PyInit_pwd(void);");
            inittab.AppendLine("\t{\"pwd\", PyInit_pwd},");

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

            declarations.AppendLine("extern PyObject* PyInit_atexit(void);");
            inittab.AppendLine("\t{\"atexit\", PyInit_atexit},");

            declarations.AppendLine("extern PyObject* PyInit__stat(void);");
            inittab.AppendLine("\t{\"_stat\", PyInit__stat},");

            declarations.AppendLine("extern PyObject* PyInit_time(void);");
            inittab.AppendLine("\t{\"time\", PyInit_time},");

            declarations.AppendLine("extern PyObject* PyInit__locale(void);");
            inittab.AppendLine("\t{\"_locale\", PyInit__locale},");

            declarations.AppendLine("extern PyObject* PyInit__io(void);");
            inittab.AppendLine("\t{\"_io\", PyInit__io},");

            declarations.AppendLine("extern PyObject* PyInit_zipimport(void);");
            inittab.AppendLine("\t{\"zipimport\", PyInit_zipimport},");

            declarations.AppendLine("extern PyObject* PyInit_faulthandler(void);");
            inittab.AppendLine("\t{\"faulthandler\", PyInit_faulthandler},");

            declarations.AppendLine("extern PyObject* PyInit__tracemalloc(void);");
            inittab.AppendLine("\t{\"_tracemalloc\", PyInit__tracemalloc},");

            declarations.AppendLine("extern PyObject* PyInit__symtable(void);");
            inittab.AppendLine("\t{\"_symtable\", PyInit__symtable},");

            configText = configText.Replace("/* -- ADDMODULE MARKER 1 -- */",
                declarations.ToString());
            configText = configText.Replace("/* -- ADDMODULE MARKER 2 -- */",
                inittab.ToString());
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[SourceFileKey].ToString();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            var stubPath = this.Macros["templateConfig"].ToString();
            var stubText = System.IO.File.ReadAllText(stubPath);
            // TODO: this should be following the rules in Modules/makesetup and Modules/Setup.dist
            // for which modules are static (and thus part of the Python library) and which are shared
            // and separate in the distribution
            // note that you need to read Setup.dist backward, as some modules are mentioned twice
            // and it is the 'topmost' that overrules
            InsertBuiltinModules(ref stubText);
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.NewLine = "\n";
                writeFile.Write(stubText);
            }
            Bam.Core.Log.MessageAll($"Written '{destPath}'");
        }
    }
}
