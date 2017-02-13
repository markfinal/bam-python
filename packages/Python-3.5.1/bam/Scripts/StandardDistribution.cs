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

            // dynamic library extension modules common to all platforms
            var moduleList = new Bam.Core.Array<Bam.Core.Module>();
            moduleList.Add(module.Include<_testcapi>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_elementtree>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<unicodedata>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<select>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<_socket>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<fpectl>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<fpetest>(C.DynamicLibrary.Key, execDir, root));
            moduleList.Add(module.Include<pyexpat>(C.DynamicLibrary.Key, execDir, root));

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
                // new list
                moduleList.Add(module.Include<math>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_struct>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_random>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_pickle>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_datetime>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_bisect>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_heapq>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<fcntl>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<grp>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<mmap>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_csv>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_crypt>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<nis>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<termios>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<resource>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_posixsubprocess>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<audioop>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_md5>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_sha1>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_sha256>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_sha512>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<syslog>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_curses>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_curses_panel>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<binascii>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<parser>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<zlib>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_multibytecodec>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_cn>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_hk>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_iso2022>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_jp>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_kr>(C.DynamicLibrary.Key, execDir, root));
                moduleList.Add(module.Include<_codecs_tw>(C.DynamicLibrary.Key, execDir, root));

                // old list
                var arrayModule = module.Include<ArrayModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                arrayModule.DependsOn(platIndependentModules);

                var cmathModule = module.Include<CMathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                cmathModule.DependsOn(platIndependentModules);

                var timeModule = module.Include<TimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                timeModule.DependsOn(platIndependentModules);

                var atexitModule = module.Include<AtexitModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                atexitModule.DependsOn(platIndependentModules);

                var jsonModule = module.Include<JsonModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                jsonModule.DependsOn(platIndependentModules);

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

                var pwdModule = module.Include<PwdModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                pwdModule.DependsOn(platIndependentModules);

                #if false
                var sslModule = module.Include<SSLModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                sslModule.DependsOn(platIndependentModules);

                var hashlibModule = module.Include<HashLibModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                hashlibModule.DependsOn(platIndependentModules);
                #endif

                #if false
                var cursesModule = module.Include<CursesModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", root);
                cursesModule.DependsOn(platIndependentModules);
                #endif
            }

            // currently not buildable
            //moduleList.Add(module.Include<spwd>(C.DynamicLibrary.Key, execDir, root));
            //moduleList.Add(module.Include<_ssl>(C.DynamicLibrary.Key, execDir, root));
            //moduleList.Add(module.Include<_tkinter>(C.DynamicLibrary.Key, execDir, root));
            //moduleList.Add(module.Include<_gdbm>(C.DynamicLibrary.Key, execDir, root));
            //moduleList.Add(module.Include<_dbm>(C.DynamicLibrary.Key, execDir, root));

            // ensure that modules are copied AFTER the platform independent modules
            foreach (var mod in moduleList)
            {
                mod.DependsOn(platIndependentModules);
            }
        }
    }
}
