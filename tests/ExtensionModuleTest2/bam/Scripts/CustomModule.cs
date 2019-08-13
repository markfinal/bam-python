#region License
// Copyright (c) 2010-2019, Mark Final
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
using Python.StandardDistribution;
using System.Linq;
namespace ExtensionModuleTest2
{
    [Bam.Core.ModuleGroup("ExtensionModuleTest2")]
    sealed class CustomModule :
        Python.DynamicExtensionModule
    {
        public CustomModule()
            :
            base("custommodule", "source/custommodule")
        { }

        protected override void
        Init()
        {
            base.Init();

            var shell = Bam.Core.Graph.Instance.FindReferencedModule<Python.PythonShell>();
            shell.Requires(this);
        }
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
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.RegisterPythonModuleTypesToCollate();
            this.Mapping.Register(
                typeof(Python.PyDocGeneratedHtml),
                Python.PyDocGeneratedHtml.PyDocHtmlKey,
                this.CreateTokenizedString("$(0)/pyapidocs", new[] { this.ExecutableDir }), false
            );

            var appAnchor = this.Include<Python.PythonShell>(C.ConsoleApplication.ExecutableKey);
            this.IncludePythonStandardDistribution(appAnchor, this.Find<Python.PythonLibrary>().First());

            var extensionModule = this.Find<CustomModule>().First();
            this.SetPublishingDirectoryForPythonBinaryModule(extensionModule as Publisher.CollatedObject);

            this.PyInterpreter = appAnchor as Publisher.CollatedFile;
        }
    }

    class CustomModuleAPIDocs :
        Python.PyDocGeneratedHtml
    {
        protected override void
        Init()
        {
            base.Init();

            // must execute after publishing
            var publishing = Bam.Core.Graph.Instance.FindReferencedModule<CustomModuleRuntime>() as CustomModuleRuntime;
            this.Requires(publishing);

            // use the collated Python, which has all of it's dependencies collated beside it
            // to make it runnable
            this.Tool = publishing.PyInterpreter;

            // specify which module to document, and where the HTML should be generated at
            this.ModuleToDocument(
                "custommodule",
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/docs")
            );
        }
    }

    //[Bam.Core.ConfigurationFilter(EConfiguration.Profile)]
    sealed class CustomModuleDebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateSymbolsFrom<CustomModuleRuntime>();
        }
    }

    //[Bam.Core.ConfigurationFilter(EConfiguration.Profile)]
    sealed class CustomModuleStripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.StripBinariesFrom<CustomModuleRuntime, CustomModuleDebugSymbols>();

            var collator = Bam.Core.Graph.Instance.FindReferencedModule<CustomModuleRuntime>();
            this.Include<CustomModuleAPIDocs>(Python.PyDocGeneratedHtml.PyDocHtmlKey, collator, collator.PyInterpreter);
        }
    }
}
