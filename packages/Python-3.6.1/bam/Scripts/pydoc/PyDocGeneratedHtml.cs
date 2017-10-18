#region License
// Copyright (c) 2010-2017, Mark Final
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
    public class PyDocGeneratedHtml :
        Bam.Core.Module
    {
        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("PyDoc.Html");

        private IPyDocGenerationPolicy Policy = null;
        private Bam.Core.TokenizedString interpreterPath = null;
        private string moduleToDocument;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
        }

        public void
        Interpreter<DependentModule>(
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishedPath) where DependentModule : Bam.Core.Module, new()
        {
            var module = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.Requires(module);
            this.Tool = module;
            this.interpreterPath = publishedPath;
        }

        public void
        ModuleToDocument(
            string nameOfModule,
            Bam.Core.TokenizedString outputDirectory)
        {
            this.moduleToDocument = nameOfModule;
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(0)/$(1).html", outputDirectory, Bam.Core.TokenizedString.CreateVerbatim(nameOfModule)));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[Key].ToString();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.html(this, context, this.Tool as Bam.Core.ICommandLineTool, this.interpreterPath, this.GeneratedPaths[Key], this.moduleToDocument);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
            case "Native":
                var className = "Python." + mode + "PyDocToHtml";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IPyDocGenerationPolicy>.Create(className);
                break;
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
    }
}
