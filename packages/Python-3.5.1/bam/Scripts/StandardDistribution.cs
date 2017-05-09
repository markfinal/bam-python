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
namespace Python
{
    static class StandardDistribution
    {
        public readonly static string ModuleDirectory;

        static StandardDistribution()
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                ModuleDirectory = "DLLs";
            }
            else
            {
                ModuleDirectory = System.String.Format("lib/python{0}/lib-dynload", Version.MajorDotMinor);
            }
        }

        public static Publisher.CollatedDirectory
        Publish(
            Publisher.Collation module,
            Publisher.CollatedFile root)
        {
            var pyLibCopy = module.Include<PythonLibrary>(C.DynamicLibrary.Key, ".", root);
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;

            // dynamic library extension modules common to all platforms
            var moduleList = new Bam.Core.Array<Bam.Core.Module>();
            moduleList.Add(module.Include<_multiprocessing>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_ctypes>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_testmultiphase>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_testimportmultiple>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_testbuffer>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_testcapi>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_elementtree>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<unicodedata>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<select>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_socket>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<fpectl>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<fpetest>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<pyexpat>(C.DynamicLibrary.Key, ModuleDirectory, root));

            Publisher.CollatedDirectory platIndependentModules = null;
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                platIndependentModules = module.IncludeDirectory(pyLibDir, ".", root);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                platIndependentModules = module.IncludeDirectory(pyLibDir, "lib", root);
                platIndependentModules.CopiedFilename = System.String.Format("python{0}", Version.MajorDotMinor);

                module.Include<SysConfigDataPythonFile>(
                    SysConfigDataPythonFile.Key,
                    System.String.Format("lib/python{0}", Version.MajorDotMinor),
                    root);

                module.Include<PyConfigHeader>(
                    PyConfigHeader.Key,
                    System.String.Format("include/python{0}", Version.MajorDotMinor),
                    root); // needed by distutils
                module.Include<PyMakeFile>(
                    PyMakeFile.Key,
                    System.String.Format("lib/python{0}/config-{0}", Version.MajorDotMinor),
                    root); // needed by distutils

                // extension modules
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    moduleList.Add(module.Include<_scproxy>(C.DynamicLibrary.Key, ModuleDirectory, root));
                }
                moduleList.Add(module.Include<_opcode>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_lsprof>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_json>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_thread>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<array>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<cmath>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<math>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_struct>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_random>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_pickle>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_datetime>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_bisect>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_heapq>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<fcntl>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<grp>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<mmap>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_csv>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_crypt>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<nis>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<termios>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<resource>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_posixsubprocess>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<audioop>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_md5>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_sha1>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_sha256>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_sha512>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<syslog>(C.DynamicLibrary.Key, ModuleDirectory, root));
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    moduleList.Add(module.Include<_curses>(C.DynamicLibrary.Key, ModuleDirectory, root));
                    moduleList.Add(module.Include<_curses_panel>(C.DynamicLibrary.Key, ModuleDirectory, root));
                }
                moduleList.Add(module.Include<binascii>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<parser>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<zlib>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_multibytecodec>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_cn>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_hk>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_iso2022>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_jp>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_kr>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<_codecs_tw>(C.DynamicLibrary.Key, ModuleDirectory, root));
                moduleList.Add(module.Include<xxsubtype>(C.DynamicLibrary.Key, ModuleDirectory, root));
            }

#if PYTHON_WITH_OPENSSL
            moduleList.Add(module.Include<_ssl>(C.DynamicLibrary.Key, ModuleDirectory, root));
            moduleList.Add(module.Include<_hashlib>(C.DynamicLibrary.Key, ModuleDirectory, root));
#endif
#if PYTHON_WITH_SQLITE
            moduleList.Add(module.Include<_sqlite3>(C.DynamicLibrary.Key, ModuleDirectory, root));
            module.Include<sqlite.SqliteShared>(C.DynamicLibrary.Key, ModuleDirectory, root);
#endif
#if PYTHON_USE_ZLIB_PACKAGE
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                // as zlibmodule is builtin
                module.Include<global::zlib.ZLib>(C.DynamicLibrary.Key, ".", root);
            }
            else
            {
                module.Include<global::zlib.ZLib>(C.DynamicLibrary.Key, ModuleDirectory, root);
            }
#endif

            // currently not buildable
            //moduleList.Add(module.Include<spwd>(C.DynamicLibrary.Key, ModuleDirectory, root));
            //moduleList.Add(module.Include<_tkinter>(C.DynamicLibrary.Key, ModuleDirectory, root));
            //moduleList.Add(module.Include<_gdbm>(C.DynamicLibrary.Key, ModuleDirectory, root));
            //moduleList.Add(module.Include<_dbm>(C.DynamicLibrary.Key, ModuleDirectory, root));

            // ensure that modules are copied AFTER the platform independent modules
            foreach (var mod in moduleList)
            {
                mod.DependsOn(platIndependentModules);
            }

            return platIndependentModules;
        }
    }
}
