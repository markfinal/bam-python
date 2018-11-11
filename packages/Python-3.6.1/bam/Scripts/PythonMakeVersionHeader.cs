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
    interface IConfigurePythonResourceHeader :
        Bam.Core.IModuleConfiguration
    {
        bool DebugCRT { get; }
    }

    sealed class ConfigurePythonResourceHeader :
        IConfigurePythonResourceHeader
    {
        public ConfigurePythonResourceHeader(
            Bam.Core.Environment buildEnvironment) => this.DebugCRT = false;

        public bool DebugCRT { get; set; }
    }

    // in order to run a private patch
    class EmptySettings :
        Bam.Core.Settings
    {}

    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class PythonMakeVersionHeader :
        C.ProceduralHeaderFile,
        Bam.Core.IHasModuleConfiguration
    {
        System.Type Bam.Core.IHasModuleConfiguration.ReadOnlyInterfaceType => typeof(IConfigurePythonResourceHeader);
        System.Type Bam.Core.IHasModuleConfiguration.WriteableClassType => typeof(ConfigurePythonResourceHeader);

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Settings = new EmptySettings(); // in order to run a private patch
        }

        protected override Bam.Core.TokenizedString OutputPath => this.CreateTokenizedString("$(packagebuilddir)/$(config)/pythonnt_rc.h");

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine($"#define FIELD3 {Version.Field3}");
                contents.AppendLine($"#define MS_DLL_ID \"{Version.MajorDotMinor}\"");

                var useDebugCRT = (this.Configuration as IConfigurePythonResourceHeader).DebugCRT;
                if (useDebugCRT)
                {
                    contents.AppendLine($"#define PYTHON_DLL_NAME \"{Version.WindowsDebugOutputName}.dll\"");
                }
                else
                {
                    contents.AppendLine($"#define PYTHON_DLL_NAME \"{Version.WindowsOutputName}.dll\"");
                }
                return contents.ToString();
            }
        }
    }
}
