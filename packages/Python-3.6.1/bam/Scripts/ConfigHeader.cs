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
using Bam.Core;
namespace Python
{
    interface IConfigurePython :
        Bam.Core.IModuleConfiguration
    {
        bool PyDEBUG { get; }
    }

    sealed class ConfigurePython :
        IConfigurePython
    {
        public ConfigurePython(
            Bam.Core.Environment buildEnvironment) => this.PyDEBUG = buildEnvironment.Configuration.HasFlag(Bam.Core.EConfiguration.Debug);

        public bool PyDEBUG { get; set; }
    }

    [Bam.Core.ModuleGroup("Thirdparty/Python")]
    class PyConfigHeader :
        C.ProceduralHeaderFile,
        Bam.Core.IHasModuleConfiguration
    {
        System.Type Bam.Core.IHasModuleConfiguration.ReadOnlyInterfaceType => typeof(IConfigurePython);
        System.Type Bam.Core.IHasModuleConfiguration.WriteableClassType => typeof(ConfigurePython);
        protected override Bam.Core.TokenizedString OutputPath => this.CreateTokenizedString("$(packagebuilddir)/$(config)/pyconfig.h");
        protected override bool UseSystemIncludeSearchPaths => true;

        protected override string Contents
        {
            get
            {
                var bitDepth = (C.EBit)Bam.Core.CommandLineProcessor.Evaluate(new C.Options.DefaultBitDepth());
                var contents = new System.Text.StringBuilder();
                if ((this.Configuration as IConfigurePython).PyDEBUG)
                {
                    contents.AppendLine("#define Py_DEBUG");
                }
                contents.AppendLine("#define _BSD_SOURCE 1");
                contents.AppendLine("#define _DEFAULT_SOURCE 1"); // future replacement of _BSD_SOURCE
                contents.AppendLine("#include <limits.h>"); // so that __USE_POSIX is not undeffed
                contents.AppendLine("#define __USE_POSIX 1");
                contents.AppendLine("#define __USE_POSIX199309 1");
                contents.AppendLine("#define HAVE_STDINT_H");
                contents.AppendLine("#define HAVE_SYS_TIME_H");
                contents.AppendLine("#define HAVE_SYS_STAT_H");
                contents.AppendLine("#define HAVE_LONG_LONG 1"); // required to have a value in Modules/arraymodule.c
                contents.AppendLine("#define HAVE_STRING_H");
                contents.AppendLine("#define HAVE_ERRNO_H");
                contents.AppendLine("#define HAVE_LSTAT");
                contents.AppendLine("#define PY_FORMAT_LONG_LONG \"ll\"");
                contents.AppendLine("#define PY_FORMAT_SIZE_T \"z\"");
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    contents.AppendLine("#define SIZEOF_WCHAR_T 4");
                    // see Include/pymacconfig.h for some defines
                }
                else
                {
                    contents.AppendLine("#define SIZEOF_WCHAR_T 2");
                    // note SIZEOF_SIZE_T and SIZEOF_VOID_P must match in order for pyport.h
                    // to define Py_ssize_t
                    if (bitDepth == C.EBit.SixtyFour)
                    {
                        contents.AppendLine("#define SIZEOF_LONG 8");
                        contents.AppendLine("#define SIZEOF_SIZE_T 8");
                        contents.AppendLine("#define SIZEOF_VOID_P 8");
                        contents.AppendLine("#define SIZEOF_TIME_T 8");
                    }
                    else
                    {
                        contents.AppendLine("#define SIZEOF_LONG 4");
                        contents.AppendLine("#define SIZEOF_SIZE_T 4");
                        contents.AppendLine("#define SIZEOF_VOID_P 4");
                        contents.AppendLine("#define SIZEOF_TIME_T 4");
                    }
                }
                contents.AppendLine("#define SIZEOF_LONG_LONG 8");
                contents.AppendLine("#define SIZEOF_INT 4");
                contents.AppendLine("#define SIZEOF_SHORT 2");
                if (bitDepth == C.EBit.SixtyFour)
                {
                    contents.AppendLine("#define SIZEOF_OFF_T 8");
                }
                else
                {
                    contents.AppendLine("#define SIZEOF_OFF_T 4");
                }
                contents.AppendLine("#define HAVE_STDARG_PROTOTYPES");
                contents.AppendLine("#define HAVE_UINTPTR_T");
                contents.AppendLine("#define HAVE_WCHAR_H");
                contents.AppendLine("#define HAVE_UINT32_T");
                contents.AppendLine("#define HAVE_INT32_T");
                contents.AppendLine("#define HAVE_FCNTL_H");
                contents.AppendLine("#define HAVE_UNISTD_H 1"); // required to have a value in _scproxy.c for Xcode9+
                contents.AppendLine("#define HAVE_SIGNAL_H");
                contents.AppendLine("#define TIME_WITH_SYS_TIME");
                contents.AppendLine("#define HAVE_DIRENT_H");
                contents.AppendLine("#define HAVE_CLOCK");
                contents.AppendLine("#define HAVE_GETTIMEOFDAY");
                contents.AppendLine("#define WITH_THREAD");
                contents.AppendLine("#define WITH_PYMALLOC");
                contents.AppendLine("#define HAVE_SYSCONF"); // or my_getallocationgranularity is undefined
                contents.AppendLine("#define PyAPI_FUNC(RTYPE) __attribute__ ((visibility(\"default\"))) RTYPE");
                contents.AppendLine("#define PyAPI_DATA(RTYPE) extern __attribute__ ((visibility(\"default\"))) RTYPE");
                contents.AppendLine("#ifdef Py_BUILD_CORE");
                contents.AppendLine("#define PyMODINIT_FUNC PyObject*");
                contents.AppendLine("#else");
                contents.AppendLine("#define PyMODINIT_FUNC extern __attribute__ ((visibility(\"default\"))) PyObject*");
                contents.AppendLine("#endif");
                contents.AppendLine("#define HAVE_DYNAMIC_LOADING");
                contents.AppendLine($"#define SOABI \"{Version.SOABI}\"");
                contents.AppendLine("#define HAVE_DLFCN_H");
                contents.AppendLine("#define HAVE_DLOPEN");
                contents.AppendLine("#define HAVE_DECL_RTLD_LAZY 1");
                contents.AppendLine("#define HAVE_DECL_RTLD_LOCAL 1");
                contents.AppendLine("#define HAVE_DECL_RTLD_GLOBAL 1");
                contents.AppendLine("#define HAVE_GETADDRINFO"); // for socket extension module
                contents.AppendLine("#define HAVE_ADDRINFO"); // for socket extension module
                contents.AppendLine("#define HAVE_SOCKADDR_STORAGE"); // for socket extension module
                contents.AppendLine("#define HAVE_SYS_WAIT_H"); // for help() to work in the shell
                contents.AppendLine("#define HAVE_WAITPID"); // for help() to work in the shell
                contents.AppendLine("#define HAVE_GETPID"); // for os.getpid()
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    contents.AppendLine("#define HAVE_FSTATVFS");
                    contents.AppendLine("#define HAVE_SYS_STATVFS_H");
                }
                else
                {
                    contents.AppendLine("#define HAVE_CLOCK_GETTIME");
                    contents.AppendLine("#define daylight __daylight");
                    contents.AppendLine("#define HAVE_LANGINFO_H"); // defines CODESET
                    contents.AppendLine("#define HAVE_NET_IF_H"); // for socket extension module
                    contents.AppendLine("#define HAVE_LINUX_CAN_H"); // for socket extension module
                    contents.AppendLine("#define HAVE_SYS_IOCTL_H"); // for socket extension module
                    contents.AppendLine("#define HAVE_NETPACKET_PACKET_H"); // for socket extension module
                    contents.AppendLine("#define HAVE_COPYSIGN");
                    contents.AppendLine("#define HAVE_ROUND");
                    contents.AppendLine("#define HAVE_HYPOT");
                }
                contents.AppendLine("#define WITH_DOC_STRINGS"); // or there is no documentation
                contents.AppendLine("#define HAVE_UNAME"); // available on *nix style OSs, exposes os.uname()
                contents.AppendLine("#define HAVE_SYS_UTSNAME_H"); // required for uname
                contents.AppendLine("#define HAVE_STRFTIME"); // required for time.strftime
                contents.AppendLine("#define HAVE_READLINK 1"); // required for os.readlink
                contents.AppendLine("#define HAVE_GETPEERNAME 1"); // required for SSLSocket.getpeername()
                contents.AppendLine("#define HAVE_SYMLINK 1"); // required for os.symlink
                contents.AppendLine("#define HAVE_TZNAME 1"); // required for time.timezone
                contents.AppendLine("#define HAVE_TIMEGM 1"); // required for timegm
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    contents.AppendLine("#define SIZEOF__BOOL 1"); // required for ctypes
                }
                return contents.ToString();
            }
        }
    }
}
