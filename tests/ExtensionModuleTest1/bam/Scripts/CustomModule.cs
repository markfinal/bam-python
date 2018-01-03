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
using Python.StandardDistribution;
using System.Linq;
namespace ExtensionModuleTest1
{
    [Bam.Core.ModuleGroup("ExtensionModuleTest1")]
    sealed class CustomModule :
        Python.DynamicExtensionModule
    {
        public CustomModule()
            :
            base("custommodule", "source/custommodule")
        { }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var shell = Bam.Core.Graph.Instance.FindReferencedModule<Python.PythonShell>();
            shell.Requires(this);
        }
    }

    sealed class CustomModuleRuntime :
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

            var appAnchor = this.Include<Python.PythonShell>(C.ConsoleApplication.Key);
            this.IncludePythonStandardDistribution(appAnchor);

            var extensionModule = this.Find<CustomModule>().First();
            (extensionModule as Publisher.CollatedObject).SetPublishingDirectory("$(0)/" + Python.StandardDistribution.PublisherExtensions.ModuleDirectory, new[] { this.ExecutableDir });
#else
            var app = this.Include<Python.PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            var platformIndependentModulesPublish = Python.StandardDistribution.Publish(this, app);

            var custommodule = this.Include<CustomModule>(C.Plugin.Key, Python.StandardDistribution.ModuleDirectory, app);
            custommodule.Requires(platformIndependentModulesPublish); // publish after everything else
#endif
        }
    }
}
