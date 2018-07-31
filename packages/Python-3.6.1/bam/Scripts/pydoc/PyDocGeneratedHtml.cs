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
using System.Linq;
namespace Python
{
    public class PyDocGeneratedHtml :
        Bam.Core.Module
    {
        public const string PyDocHtmlKey = "PyDoc.Html";

        private string moduleToDocument;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.PrivatePatch(settings =>
                {
                    var pyDocSettings = settings as IPyDocSettings;
                    pyDocSettings.ModuleToDocument = this.moduleToDocument;
                });
        }

        public void
        ModuleToDocument(
            string nameOfModule,
            Bam.Core.TokenizedString outputDirectory)
        {
            this.moduleToDocument = nameOfModule;
            this.RegisterGeneratedFile(
                PyDocHtmlKey,
                this.CreateTokenizedString(
                    "$(0)/$(1).html",
                    outputDirectory,
                    Bam.Core.TokenizedString.CreateVerbatim(nameOfModule)
                )
            );
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[PyDocHtmlKey].ToString();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(
                    this.GeneratedPaths[PyDocHtmlKey]
                );
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.GenerateHtml(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.GenerateHtml(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.GenerateHtml(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    //XcodeSupport.GenerateHtml(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        private Bam.Core.PreBuiltTool Compiler
        {
            get
            {
                return this.Tool as Bam.Core.PreBuiltTool;
            }

            set
            {
                this.Tool = value;
            }
        }

        public override Bam.Core.TokenizedString WorkingDirectory
        {
            get
            {
                return this.OutputDirectories.First();
            }
        }
    }
}
