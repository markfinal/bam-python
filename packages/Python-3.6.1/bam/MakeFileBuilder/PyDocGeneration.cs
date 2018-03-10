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
    public sealed class MakeFilePyDocToHtml :
        IPyDocGenerationPolicy
    {
        void
        IPyDocGenerationPolicy.html(
            PyDocGeneratedHtml sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool intepreter,
            Bam.Core.TokenizedString interpreterPath,
            Bam.Core.TokenizedString generatedHtml,
            string moduleToDocument)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(generatedHtml);
            rule.AddPrerequisite(interpreterPath);

            var htmlOutputPath = generatedHtml.ToString();
            var htmlOutputDir = System.IO.Path.GetDirectoryName(htmlOutputPath);

            var args = new Bam.Core.StringArray();
            args.Add(System.String.Format("cd {0}; ", htmlOutputDir));
            args.Add(interpreterPath.ToString());
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add("-m");
            args.Add("pydoc");
            args.Add("-w");
            args.Add(moduleToDocument);
            rule.AddShellCommand(args.ToString(' '));

            meta.CommonMetaData.AddDirectory(htmlOutputDir);
        }
    }
}