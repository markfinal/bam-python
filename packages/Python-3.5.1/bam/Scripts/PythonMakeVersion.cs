#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace Python
{
    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    [C.Thirdparty]
    sealed class PythonMakeVersion :
        C.ConsoleApplication,
        Bam.Core.ICommandLineTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/PC/make_versioninfo.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
                        compiler.DisableWarnings.AddUnique("4100"); // Python-3.5.1\PC\make_versioninfo.c(23) : warning C4100: 'argv' : unreferenced formal parameter
                    }
                });

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }

        System.Collections.Generic.Dictionary<string, TokenizedStringArray> ICommandLineTool.EnvironmentVariables
        {
            get
            {
                return null;
            }
        }

        TokenizedString ICommandLineTool.Executable
        {
            get
            {
                return this.GeneratedPaths[Key];
            }
        }

        StringArray ICommandLineTool.InheritedEnvironmentVariables
        {
            get
            {
                return null;
            }
        }

        TokenizedStringArray ICommandLineTool.InitialArguments
        {
            get
            {
                return null;
            }
        }

        TokenizedStringArray ICommandLineTool.TerminatingArguments
        {
            get
            {
                return null;
            }
        }

        string ICommandLineTool.UseResponseFileOption
        {
            get
            {
                return null;
            }
        }

        Settings ITool.CreateDefaultSettings<T>(T module)
        {
            return null;
        }
    }
}
