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
namespace ExtensionModuleTest2
{
    [Bam.Core.ModuleGroup("ExtensionModuleTest2")]
    class CustomModule :
        Python.DynamicExtensionModule
    {
        public CustomModule()
            :
            base("custommodule", "source/custommodule")
        { }
    }

    sealed class CustomModuleRuntime :
        Publisher.Collation
    {
        public Publisher.CollatedFile
        PyInterpreter
        {
            get;
            private set;
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<Python.PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            var platformIndependentModulesPublish = Python.StandardDistribution.Publish(this, app);

            var custommodule = this.Include<CustomModule>(C.Plugin.Key, Python.StandardDistribution.ModuleDirectory, app);
            custommodule.Requires(platformIndependentModulesPublish); // publish after everything else

            this.PyInterpreter = app;
        }
    }

    class CustomModuleAPIDocs :
        Python.PyDocGeneratedHtml
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // must execute after publishing
            var publishing = Bam.Core.Graph.Instance.FindReferencedModule<CustomModuleRuntime>() as CustomModuleRuntime;
            this.Requires(publishing);

            // set the interpreter to run the Python command
            this.Interpreter<Python.PythonShell>(C.ConsoleApplication.Key, publishing.PyInterpreter.GeneratedPaths[Publisher.CollatedFile.Key]);
            // specify which module to document, and where the HTML should be generated at
            this.ModuleToDocument("custommodule", this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/docs"));
        }
    }

    //[Bam.Core.ConfigurationFilter(EConfiguration.Profile)]
    sealed class CustomModuleDebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateSymbolsFrom<CustomModuleRuntime>();
        }
    }

    //[Bam.Core.ConfigurationFilter(EConfiguration.Profile)]
    sealed class CustomModuleStripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.StripBinariesFrom<CustomModuleRuntime, CustomModuleDebugSymbols>();

            var runtime = Bam.Core.Graph.Instance.FindReferencedModule<CustomModuleRuntime>();
            this.Include<CustomModuleAPIDocs>(Python.PyDocGeneratedHtml.Key, "pyapidocs", runtime.InitialReference);
        }
    }
}
