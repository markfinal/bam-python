using Bam.Core;
namespace Python
{
    sealed class PythonShell :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("python");

            var source = this.CreateCSourceContainer("$(packagedir)/Programs/python.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
            else
            {
                var pyConfigHeader = Bam.Core.Graph.Instance.FindReferencedModule<PyConfigHeader>();
                source.DependsOn(pyConfigHeader);
                source.UsePublicPatches(pyConfigHeader);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.PrivatePatch(settings =>
                        {
                            var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                            gccLinker.CanUseOrigin = true;
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        });
                }
            }

            this.LinkAgainst<PythonLibrary>();
        }
    }

    sealed class ShellRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<PythonShell>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            var pyLibCopy = this.Include<PythonLibrary>(C.DynamicLibrary.Key, ".", app);
            var pyLibDir = (pyLibCopy.SourceModule as Python.PythonLibrary).LibraryDirectory;

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, ".", app);
                platIndependentModules.CopiedFilename = "lib";
            }
            else
            {
                var platIndependentModules = this.IncludeDirectory(pyLibDir, "lib", app);
                platIndependentModules.CopiedFilename = "python3.5";

                this.Include<SysConfigDataPythonFile>(SysConfigDataPythonFile.Key, "lib/python3.5", app);

                // extension modules
                var structModule = this.Include<StructModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                structModule.DependsOn(platIndependentModules);

                var arrayModule = this.Include<ArrayModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                arrayModule.DependsOn(platIndependentModules);

                var cmathModule = this.Include<CMathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                cmathModule.DependsOn(platIndependentModules);

                var mathModule = this.Include<MathModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                mathModule.DependsOn(platIndependentModules);

                var timeModule = this.Include<TimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                timeModule.DependsOn(platIndependentModules);

                var datetimeModule = this.Include<DateTimeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                datetimeModule.DependsOn(platIndependentModules);

                var randomModule = this.Include<RandomModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                randomModule.DependsOn(platIndependentModules);

                var bisectModule = this.Include<BisectModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                bisectModule.DependsOn(platIndependentModules);

                var heapqModule = this.Include<HeapqModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                heapqModule.DependsOn(platIndependentModules);

                var pickleModule = this.Include<PickleModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                pickleModule.DependsOn(platIndependentModules);

                var atexitModule = this.Include<AtexitModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                atexitModule.DependsOn(platIndependentModules);

                var jsonModule = this.Include<JsonModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                jsonModule.DependsOn(platIndependentModules);

                var testcapiModule = this.Include<TestCAPIModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                testcapiModule.DependsOn(platIndependentModules);

                var testBufferModule = this.Include<TestBufferModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                testBufferModule.DependsOn(platIndependentModules);

                var testImportMultipleModule = this.Include<TestImportMultipleModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                testImportMultipleModule.DependsOn(platIndependentModules);

                var testMultiPhaseModule = this.Include<TestMultiPhaseModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                testMultiPhaseModule.DependsOn(platIndependentModules);

                var lsprofModule = this.Include<LSProfModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                lsprofModule.DependsOn(platIndependentModules);

                var unicodeDataModule = this.Include<UnicodeDataModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                unicodeDataModule.DependsOn(platIndependentModules);

                var opcodeModule = this.Include<OpCodeModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                opcodeModule.DependsOn(platIndependentModules);

                var fcntlModule = this.Include<FcntlModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                fcntlModule.DependsOn(platIndependentModules);

                var pwdModule = this.Include<PwdModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                pwdModule.DependsOn(platIndependentModules);

                var grpModule = this.Include<GrpModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                grpModule.DependsOn(platIndependentModules);

                //var spwdModule = this.Include<SPwdModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                //spwdModule.DependsOn(platIndependentModules);

                var selectModule = this.Include<SelectModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                selectModule.DependsOn(platIndependentModules);

                var parserModule = this.Include<ParserModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                parserModule.DependsOn(platIndependentModules);

                var mmapModule = this.Include<MMapModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                mmapModule.DependsOn(platIndependentModules);

                var syslogModule = this.Include<SysLogModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                syslogModule.DependsOn(platIndependentModules);

                var audioopModule = this.Include<AudioOpModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                audioopModule.DependsOn(platIndependentModules);

                var cryptModule = this.Include<CryptModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                cryptModule.DependsOn(platIndependentModules);

                var csvModule = this.Include<CSVModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                csvModule.DependsOn(platIndependentModules);

                var posixSubprocessModule = this.Include<PosixSubprocessModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                posixSubprocessModule.DependsOn(platIndependentModules);

                var socketModule = this.Include<SocketModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                socketModule.DependsOn(platIndependentModules);

                #if false
                var sslModule = this.Include<SSLModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                sslModule.DependsOn(platIndependentModules);

                var hashlibModule = this.Include<HashLibModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                hashlibModule.DependsOn(platIndependentModules);
                #endif

                var sha256Module = this.Include<SHA256Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                sha256Module.DependsOn(platIndependentModules);

                var sha512Module = this.Include<SHA512Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                sha512Module.DependsOn(platIndependentModules);

                var md5Module = this.Include<MD5Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                md5Module.DependsOn(platIndependentModules);

                var sha1Module = this.Include<SHA1Module>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                sha1Module.DependsOn(platIndependentModules);

                var termiosModule = this.Include<TermiosModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                termiosModule.DependsOn(platIndependentModules);

                var resourceModule = this.Include<ResourceModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                resourceModule.DependsOn(platIndependentModules);

                #if false
                var cursesModule = this.Include<CursesModule>(C.DynamicLibrary.Key, "lib/python3.5/lib-dynload", app);
                cursesModule.DependsOn(platIndependentModules);
                #endif
            }
        }
    }
}