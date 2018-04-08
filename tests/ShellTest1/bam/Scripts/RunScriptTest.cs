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
using Bam.Core;
using System.Linq;
using Python.StandardDistribution;
namespace ShellTest1
{
    sealed class TestRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.RegisterPythonModuleTypesToCollate();

            this.Mapping.Register(
                typeof(Python.PythonZip),
                Publisher.ZipModule.Key,
                this.CreateTokenizedString("$(0)", new[] { this.ExecutableDir }),
                true
            );

            var appAnchor = this.Include<Python.PythonShell>(C.ConsoleApplication.Key);
            this.IncludePythonStandardDistribution(appAnchor, this.Find<Python.PythonLibrary>().First());

            this.IncludeFiles(this.CreateTokenizedString("$(packagedir)/data/helloworld.py"), this.ExecutableDir, appAnchor);
            this.Include<Python.PythonZip>(
                Publisher.ZipModule.Key
            );
#else
            var pyShellCopy = this.Include<Python.PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            Python.StandardDistribution.Publish(this, pyShellCopy);
            this.IncludeFile("$(packagedir)/data/helloworld.py", ".", pyShellCopy);
#endif
        }
    }
}
