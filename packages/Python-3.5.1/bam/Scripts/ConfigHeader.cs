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
