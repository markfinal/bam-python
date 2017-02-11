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
using Bam.Core;
namespace ShellTest1
{
    sealed class Test :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RequiredToExist<Python.PythonShell>();
        }
    }

    sealed class TestRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var pyShellCopy = this.Include<Python.PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            var pyLibCopy = this.Include<Python.PythonLibrary>(C.DynamicLibrary.Key, ".", pyShellCopy);
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;
            this.IncludeFile("$(packagedir)/data/helloworld.py", ".", pyShellCopy);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, ".", pyShellCopy);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, "lib", pyShellCopy);
                platIndependentModules.CopiedFilename = "python3.5";
                this.Include<Python.SysConfigDataPythonFile>(Python.SysConfigDataPythonFile.Key, "lib/python3.5", pyShellCopy);

                var timeModule = this.Include<Python.TimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", pyShellCopy);
                timeModule.DependsOn(platIndependentModules);
            }
        }
    }
}