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
    class PyConfigHeader :
        C.CModule
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("PyConfig header");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(config)/pyconfig.h"));

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                    }
                });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
            {
                writeFile.WriteLine("#ifndef PYCONFIG_H");
                writeFile.WriteLine("#define PYCONFIG_H");
                writeFile.WriteLine("#define _BSD_SOURCE 1");
                writeFile.WriteLine("#include <limits.h>"); // so that __USE_POSIX is not undeffed
                writeFile.WriteLine("#define __USE_POSIX 1");
                writeFile.WriteLine("#define __USE_POSIX199309 1");
                writeFile.WriteLine("#define HAVE_STDINT_H");
                writeFile.WriteLine("#define HAVE_SYS_TIME_H");
                writeFile.WriteLine("#define HAVE_SYS_STAT_H");
                writeFile.WriteLine("#define HAVE_LONG_LONG 1"); // required to have a value in Modules/arraymodule.c
                writeFile.WriteLine("#define HAVE_STRING_H");
                writeFile.WriteLine("#define HAVE_ERRNO_H");
                writeFile.WriteLine("#define HAVE_LSTAT");
                writeFile.WriteLine("#define PY_INT64_T PY_LONG_LONG");
                writeFile.WriteLine("#define PY_FORMAT_LONG_LONG \"ll\"");
                writeFile.WriteLine("#define PY_FORMAT_SIZE_T \"z\"");
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    writeFile.WriteLine("#define SIZEOF_WCHAR_T 4");
                    // see Include/pymacconfig.h for some defines
                }
                else
                {
                    writeFile.WriteLine("#define SIZEOF_WCHAR_T 2");
                    writeFile.WriteLine("#define SIZEOF_LONG 8");
                    writeFile.WriteLine("#define SIZEOF_SIZE_T 8");
                    writeFile.WriteLine("#define SIZEOF_VOID_P 8");
                    writeFile.WriteLine("#define VA_LIST_IS_ARRAY 1");
                    writeFile.WriteLine("#define SIZEOF_TIME_T 8");
                }
                writeFile.WriteLine("#define SIZEOF_LONG_LONG 8");
                writeFile.WriteLine("#define SIZEOF_INT 4");
                writeFile.WriteLine("#define SIZEOF_SHORT 2");
                writeFile.WriteLine("#define SIZEOF_OFF_T 8");
                writeFile.WriteLine("#define HAVE_STDARG_PROTOTYPES");
                writeFile.WriteLine("#define HAVE_UINTPTR_T");
                writeFile.WriteLine("#define HAVE_WCHAR_H");
                writeFile.WriteLine("#define HAVE_UINT32_T");
                writeFile.WriteLine("#define HAVE_INT32_T");
                writeFile.WriteLine("#define HAVE_FCNTL_H");
                writeFile.WriteLine("#define HAVE_UNISTD_H");
                writeFile.WriteLine("#define HAVE_SIGNAL_H");
                writeFile.WriteLine("#define TIME_WITH_SYS_TIME");
                writeFile.WriteLine("#define HAVE_DIRENT_H");
                writeFile.WriteLine("#define HAVE_CLOCK");
                writeFile.WriteLine("#define HAVE_GETTIMEOFDAY");
                writeFile.WriteLine("#define WITH_THREAD");
                writeFile.WriteLine("#define WITH_PYMALLOC");
                writeFile.WriteLine("#define HAVE_SYSCONF"); // or my_getallocationgranularity is undefined
                writeFile.WriteLine("#define PyAPI_FUNC(RTYPE) __attribute__ ((visibility(\"default\"))) RTYPE");
                writeFile.WriteLine("#define PyAPI_DATA(RTYPE) extern __attribute__ ((visibility(\"default\"))) RTYPE");
                writeFile.WriteLine("#ifdef Py_BUILD_CORE");
                writeFile.WriteLine("#define PyMODINIT_FUNC PyObject*");
                writeFile.WriteLine("#else");
                writeFile.WriteLine("#define PyMODINIT_FUNC extern __attribute__ ((visibility(\"default\"))) PyObject*");
                writeFile.WriteLine("#endif");
                writeFile.WriteLine("#define HAVE_DYNAMIC_LOADING");
                writeFile.WriteLine("#define SOABI \"cpython-35\"");
                writeFile.WriteLine("#define HAVE_DLFCN_H");
                writeFile.WriteLine("#define HAVE_DLOPEN");
                writeFile.WriteLine("#define HAVE_GETADDRINFO"); // for socket extension module
                writeFile.WriteLine("#define HAVE_ADDRINFO"); // for socket extension module
                writeFile.WriteLine("#define HAVE_SOCKADDR_STORAGE"); // for socket extension module
                writeFile.WriteLine("#define HAVE_SYS_WAIT_H"); // for help() to work in the shell
                writeFile.WriteLine("#define HAVE_WAITPID"); // for help() to work in the shell
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    writeFile.WriteLine("#define HAVE_FSTATVFS");
                    writeFile.WriteLine("#define HAVE_SYS_STATVFS_H");
                }
                else
                {
                    writeFile.WriteLine("#define HAVE_CLOCK_GETTIME");
                    writeFile.WriteLine("#define daylight __daylight");
                    writeFile.WriteLine("#define HAVE_LANGINFO_H"); // defines CODESET
                    writeFile.WriteLine("#define HAVE_NET_IF_H"); // for socket extension module
                    writeFile.WriteLine("#define HAVE_LINUX_CAN_H"); // for socket extension module
                    writeFile.WriteLine("#define HAVE_SYS_IOCTL_H"); // for socket extension module
                    writeFile.WriteLine("#define HAVE_NETPACKET_PACKET_H"); // for socket extension module
                    writeFile.WriteLine("#define HAVE_COPYSIGN");
                    writeFile.WriteLine("#define HAVE_ROUND");
                    writeFile.WriteLine("#define HAVE_HYPOT");
                }
                writeFile.WriteLine("#endif");
            }
            Bam.Core.Log.MessageAll("Written '{0}'", destPath);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }
}
