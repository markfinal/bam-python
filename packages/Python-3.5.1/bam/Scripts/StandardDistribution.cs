#region License
// Copyright (c) 2010-2017, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, module
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   module list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   module software without specific prior written permission.
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
    static class StandardDistribution
    {
        public static void
        Publish(
            Publisher.Collation module,
            Publisher.CollatedFile root)
        {
            var pyLibCopy = module.Include<PythonLibrary>(C.DynamicLibrary.Key, ".", root);
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;

            var execDir = module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) ? "DLLs" : "lib/python3.5/lib-dynload";

            // extension modules
            var moduleList = new Bam.Core.Array<Bam.Core.Module>();
            moduleList.Add(module.Include<_multibytecodec>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_cn>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_hk>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_iso2022>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_jp>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_kr>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_codecs_tw>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<unicodedata>(C.DynamicLibrary.Key, execDir, root));

            Publisher.CollatedDirectory platIndependentModules = null;
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                platIndependentModules = module.IncludeDirectory(pyLibDir, ".", root);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                platIndependentModules = module.IncludeDirectory(pyLibDir, "lib", root);
                platIndependentModules.CopiedFilename = "python3.5";

                module.Include<SysConfigDataPythonFile>(SysConfigDataPythonFile.Key, "lib/python3.5", root);

                // extension modules
                // old list
                var structModule = module.Include<StructModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                structModule.DependsOn(platIndependentModules);

                var arrayModule = module.Include<ArrayModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                arrayModule.DependsOn(platIndependentModules);

                var cmathModule = module.Include<CMathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                cmathModule.DependsOn(platIndependentModules);

                var mathModule = module.Include<MathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                mathModule.DependsOn(platIndependentModules);

                var timeModule = module.Include<TimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                timeModule.DependsOn(platIndependentModules);

                var datetimeModule = module.Include<DateTimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                datetimeModule.DependsOn(platIndependentModules);

                var randomModule = module.Include<RandomModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                randomModule.DependsOn(platIndependentModules);

                var bisectModule = module.Include<BisectModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                bisectModule.DependsOn(platIndependentModules);

                var heapqModule = module.Include<HeapqModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                heapqModule.DependsOn(platIndependentModules);

                var pickleModule = module.Include<PickleModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                pickleModule.DependsOn(platIndependentModules);

                var atexitModule = module.Include<AtexitModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                atexitModule.DependsOn(platIndependentModules);

                var jsonModule = module.Include<JsonModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                jsonModule.DependsOn(platIndependentModules);

                var testcapiModule = module.Include<TestCAPIModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                testcapiModule.DependsOn(platIndependentModules);

                var testBufferModule = module.Include<TestBufferModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                testBufferModule.DependsOn(platIndependentModules);

                var testImportMultipleModule = module.Include<TestImportMultipleModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                testImportMultipleModule.DependsOn(platIndependentModules);

                var testMultiPhaseModule = module.Include<TestMultiPhaseModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                testMultiPhaseModule.DependsOn(platIndependentModules);

                var lsprofModule = module.Include<LSProfModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                lsprofModule.DependsOn(platIndependentModules);

                var opcodeModule = module.Include<OpCodeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                opcodeModule.DependsOn(platIndependentModules);

                var fcntlModule = module.Include<FcntlModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                fcntlModule.DependsOn(platIndependentModules);

                var pwdModule = module.Include<PwdModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                pwdModule.DependsOn(platIndependentModules);

                var grpModule = module.Include<GrpModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                grpModule.DependsOn(platIndependentModules);

                //var spwdModule = module.Include<SPwdModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                //spwdModule.DependsOn(platIndependentModules);

                var selectModule = module.Include<SelectModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                selectModule.DependsOn(platIndependentModules);

                var parserModule = module.Include<ParserModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                parserModule.DependsOn(platIndependentModules);

                var mmapModule = module.Include<MMapModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                mmapModule.DependsOn(platIndependentModules);

                var syslogModule = module.Include<SysLogModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                syslogModule.DependsOn(platIndependentModules);

                var audioopModule = module.Include<AudioOpModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                audioopModule.DependsOn(platIndependentModules);

                var cryptModule = module.Include<CryptModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                cryptModule.DependsOn(platIndependentModules);

                var csvModule = module.Include<CSVModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                csvModule.DependsOn(platIndependentModules);

                var posixSubprocessModule = module.Include<PosixSubprocessModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                posixSubprocessModule.DependsOn(platIndependentModules);

                var socketModule = module.Include<SocketModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                socketModule.DependsOn(platIndependentModules);

                #if false
                var sslModule = module.Include<SSLModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                sslModule.DependsOn(platIndependentModules);

                var hashlibModule = module.Include<HashLibModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                hashlibModule.DependsOn(platIndependentModules);
                #endif

                var sha256Module = module.Include<SHA256Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                sha256Module.DependsOn(platIndependentModules);

                var sha512Module = module.Include<SHA512Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                sha512Module.DependsOn(platIndependentModules);

                var md5Module = module.Include<MD5Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                md5Module.DependsOn(platIndependentModules);

                var sha1Module = module.Include<SHA1Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                sha1Module.DependsOn(platIndependentModules);

                var termiosModule = module.Include<TermiosModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                termiosModule.DependsOn(platIndependentModules);

                var resourceModule = module.Include<ResourceModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                resourceModule.DependsOn(platIndependentModules);

                #if false
                var cursesModule = module.Include<CursesModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                cursesModule.DependsOn(platIndependentModules);
                #endif
            }

            // ensure that modules are copied AFTER the platform independent modules
            foreach (var mod in moduleList)
            {
                mod.DependsOn(platIndependentModules);
            }
        }
    }
}
