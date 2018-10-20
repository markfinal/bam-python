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
namespace Python
{
    namespace VisualC.WarningSuppression
    {
        sealed class PythonLibraryParser :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryParser()
            {
                this.Add("grammar.c", "4244");
                this.Add("myreadline.c", "4456", "4706");
                this.Add("node.c", "4244");
                this.Add("tokenizer.c", "4244", "4100", "4706");
            }
        }

        sealed class PythonLibraryObjects :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryObjects()
            {
                this.Add("abstract.c", "4100", "4706");
                this.Add("boolobject.c", "4100");
                this.Add("bytearrayobject.c", "4100", "4244", "4702");
                this.Add("bytesobject.c", "4100", "4244", "4702");
                this.Add("bytes_methods.c", "4100", "4244", "4702");
                this.Add("cellobject.c", "4100");
                this.Add("classobject.c", "4100");
                this.Add("codeobject.c", "4100", "4244");
                this.Add("complexobject.c", "4100", "4701");
                this.Add("descrobject.c", "4100");
                this.Add("dictobject.c", "4100", "4702", "4706");
                this.Add("dictobject.c", 19, "4456");
                this.Add("exceptions.c", "4100");
                this.Add("fileobject.c", "4100", "4244");
                this.Add("floatobject.c", "4100", "4244");
                this.Add("frameobject.c", "4100");
                this.Add("funcobject.c", "4100", "4244");
                this.Add("genobject.c", "4100");
                this.Add("genobject.c", 19, "4457");
                this.Add("listobject.c", "4100");
                this.Add("longobject.c", "4100", "4701");
                this.Add("longobject.c", 19, "4456");
                this.Add("memoryobject.c", "4100");
                this.Add("memoryobject.c", 19, "4456");
                this.Add("methodobject.c", "4100");
                this.Add("moduleobject.c", "4100", "4152");
                this.Add("namespaceobject.c", "4100");
                this.Add("object.c", "4100");
                this.Add("obmalloc.c", "4100");
                this.Add("setobject.c", "4245");
                this.Add("structseq.c", "4706");
                this.Add("tupleobject.c", "4245");
                this.Add("typeobject.c", "4204");
                this.Add("typeobject.c", 19, "4456");
                this.Add("unicodeobject.c", "4127", "4310", "4389", "4701", "4702", "4706");
                this.Add("unicodeobject.c", 19, "4456", "4457");
            }
        }

        sealed class PythonLibraryPython :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryPython()
            {
                this.Add("ast.c", "4100", "4702");
                this.Add("ast.c", 19, "4457");
                this.Add("bltinmodule.c", "4100", "4204", "4706");
                this.Add("ceval.c", "4100", "4918");
                this.Add("ceval.c", 19, "4456", "4457");
                this.Add("codecs.c", "4310", "4244", "4100", "4706");
                this.Add("codecs.c", 19, "4456");
                this.Add("compile.c", "4100", "4244", "4702");
                this.Add("compile.c", 19, "4457");
                this.Add("dtoa.c", "4244", "4706");
                this.Add("errors.c", "4706");
                this.Add("fileutils.c", "4244", "4706");
                this.Add("formatter_unicode.c", "4100");
                this.Add("getargs.c", "4100", "4127", "4244", "4706");
                this.Add("getargs.c", 19, "4456");
                this.Add("import.c", "4100", "4706");
                this.Add("marshal.c", "4100", "4244");
                this.Add("marshal.c", 19, "4456");
                this.Add("peephole.c", "4100", "4244", "4267");
                this.Add("pyfpe.c", "4100");
                this.Add("pylifecycle.c", "4100", "4210", "4706");
                this.Add("pylifecycle.c", 19, "4456");
                this.Add("pystate.c", "4706");
                this.Add("Python-ast.c", 19, "4456");
                this.Add("pythonrun.c", "4100");
                this.Add("pytime.c", "4100");
                this.Add("random.c", "4100");
                this.Add("symtable.c", "4100", "4706");
                this.Add("symtable.c", 19, "4457");
                this.Add("sysmodule.c", "4100", "4706");
                this.Add("thread.c", "4100", "4189");
                this.Add("traceback.c", "4100");
                this.Add("_warnings.c", "4100");
                this.Add("_warnings.c", 19, "4456");
            }
        }

        sealed class PythonLibraryBuiltinModules :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryBuiltinModules()
            {
                this.Add("main.c", "4706");
                this.Add("_opcode.c", "4100");
                this.Add("_lsprof.c", "4100");
                this.Add("_json.c", "4100", "4244");
                this.Add("_threadmodule.c", "4100", "4706");
                this.Add("arraymodule.c", "4100", "4127", "4244", "4152");
                this.Add("arraymodule.c", 19, "4456");
                this.Add("cmathmodule.c", "4100");
                this.Add("mathmodule.c", "4100", "4701");
                this.Add("_struct.c", "4100");
                this.Add("pickle.c", "4100", "4127", "4702", "4706");
                this.Add("pickle.c", 19, "4456", "4457");
                this.Add("_datetimemodule.c", "4100", "4244");
                this.Add("_datetimemodule.c", 19, "4457");
                this.Add("_bisectmodule.c", "4100");
                this.Add("_heapqmodule.c", "4100");
                this.Add("mmapmodule.c", "4100", "4057");
                this.Add("_csv.c", "4100", "4245", "4706");
                this.Add("audioop.c", "4100", "4244");
                this.Add("md5module.c", "4100", "4701");
                this.Add("sha1module.c", "4100", "4701");
                this.Add("sha256module.c", "4100", "4701");
                this.Add("sha512module.c", "4100", "4701");
                this.Add("binascii.c", "4100", "4244");
                this.Add("parsermodule.c", "4100");
                this.Add("parsermodule.c", 19, "4456");
                this.Add("zlibmodule.c", "4100", "4267", "4706");
                this.Add("xxsubtype.c", "4100", "4152");
                this.Add("blake2s_impl.c", "4100", "4244", "4245");
                this.Add("blake2b_impl.c", "4100", "4244", "4245");
                this.Add("sha3module.c", "4100", "4324", "4245");
                this.Add("signalmodule.c", "4100", "4057", "4706");
                this.Add("gcmodule.c", "4100", "4244", "4706");
                this.Add("posixmodule.c", "4100", "4057", "4389", "4201", "4701", "4702", "4703", "4706");
                this.Add("_sre.c", "4100", "4918");
                this.Add("codecsmodule.c", "4100");
                this.Add("_weakref.c", "4100");
                this.Add("_functoolsmodule.c", "4100", "4701", "4706");
                this.Add("_operator.c", "4100");
                this.Add("_operator.c", 19, "4456");
                this.Add("_collectionsmodule.c", "4100");
                this.Add("itertoolsmodule.c", "4100", "4702");
                this.Add("atexitmodule.c", "4100", "4701", "4703");
                this.Add("_stat.c", "4100");
                this.Add("timemodule.c", "4100", "4244");
                this.Add("_localemodule.c", "4100");
                this.Add("zipimport.c", "4100", "4127");
                this.Add("faulthandler.c", "4100", "4702", "4706");
                this.Add("faulthandler.c", 19, "4459");
                this.Add("_tracemalloc.c", "4100", "4204", "4359", "4706");
                this.Add("hashtable.c", "4100");
                this.Add("symtablemodule.c", "4100");
                this.Add("_winapi.c", "4100", "4201", "4204", "4702");
                this.Add("msvcrtmodule.c", "4100", "4244");
            }
        }

        sealed class PythonLibraryCJKCodecs :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryCJKCodecs()
            {
                this.Add("multibytecodec.c", "4100", "4127");
                this.Add("_codecs_cn.c", "4100", "4244");
                this.Add("_codecs_hk.c", "4100");
                this.Add("_codecs_iso2022.c", "4100", "4244");
                this.Add("_codecs_jp.c", "4100", "4244");
                this.Add("_codecs_kr.c", "4100", "4244");
                this.Add("_codecs_tw.c", "4100");
            }
        }

        sealed class PythonLibraryIO :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryIO()
            {
                this.Add("bufferedio.c", "4100", "4701", "4703");
                this.Add("bufferedio.c", 19, "4456");
                this.Add("bytesio.c", "4100");
                this.Add("fileio.c", "4100", "4701", "4703", "4706");
                this.Add("iobase.c", "4100");
                this.Add("stringio.c", "4100");
                this.Add("textio.c", "4100", "4244", "4701", "4703");
                this.Add("textio.c", 19, "4456");
                this.Add("winconsoleio.c", "4100", "4189", "4389", "4701", "4703");
                this.Add("_iomodule.c", "4100", "4706");
            }
        }
    }

    namespace Gcc.WarningSuppression
    {
        sealed class PythonLibraryParser :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryParser()
            {
                this.Add("grammar.c", "format");
                this.Add("metagrammar.c", "missing-field-initializers");
                this.Add("tokenizer.c", "unused-parameter");
            }
        }

        sealed class PythonLibraryObjects :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryObjects()
            {
                this.Add("listobject.c", "unused-parameter", "missing-field-initializers");
                this.Add("object.c", "format", "unused-parameter", "missing-field-initializers");
                this.Add("bytes_methods.c", "unused-parameter", "missing-field-initializers");
                this.Add("methodobject.c", "pedantic");
                this.Add("typeobject.c", "pedantic");
                this.Add("bytesobject.c", "unused-function");
                this.Add("bytearrayobject.c", "unused-function");
                this.Add("obmalloc.c", "unused-parameter");
                this.Add("capsule.c", "missing-field-initializers");
                this.Add("moduleobject.c", "pedantic");
                this.Add("abstract.c", "unused-parameter");
                this.Add("unicodeobject.c", "unused-function");
                this.Add("structseq.c", "missing-field-initializers");
                this.Add("exceptions.c", "missing-field-initializers", "unused-parameter");
            }
        }

        sealed class PythonLibraryPython :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryPython()
            {
                this.Add("sysmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("graminit.c", "missing-field-initializers");
                this.Add("ast.c", "unused-parameter");
                this.Add("traceback.c", "unused-parameter", "missing-field-initializers");
                this.Add("symtable.c", "unused-parameter", "missing-field-initializers");
                this.Add("peephole.c", "unused-parameter");
                this.Add("formatter_unicode.c", "unused-parameter");
                this.Add("import.c", "unused-parameter", "missing-field-initializers");
                this.Add("codecs.c", "unused-parameter", "missing-field-initializers");
                this.Add("pylifecycle.c", "unused-parameter");
                this.Add("getargs.c", "unused-parameter");
                this.Add("random.c", "unused-parameter", "missing-field-initializers");
                this.Add("compile.c", "unused-parameter");
                this.Add("bltinmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("_warnings.c", "unused-parameter", "missing-field-initializers");
                this.Add("Python-ast.c", "missing-field-initializers");
                this.Add("pyfpe.c", "unused-parameter");
                this.Add("pythonrun.c", "unused-parameter");
                this.Add("ceval.c", "unused-parameter");
                this.Add("marshal.c", "unused-parameter", "missing-field-initializers");
            }
        }

        sealed class PythonLibraryBuiltinModules :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryBuiltinModules()
            {
            }
        }

        sealed class PythonLibraryCJKCodecs :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryCJKCodecs()
            {
            }
        }

        sealed class PythonLibraryIO :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryIO()
            {
            }
        }
    }

    namespace Clang.WarningSuppression
    {
        sealed class PythonLibraryParser :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryParser()
            {
                this.Add("grammar.c", "format-pedantic");
                this.Add("metagrammar.c", "missing-field-initializers");
                this.Add("tokenizer.c", "unused-parameter");
            }
        }

        sealed class PythonLibraryObjects :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryObjects()
            {
                this.Add("setobject.c", "unused-parameter", "missing-field-initializers");
                this.Add("odictobject.c", "missing-field-initializers");
                this.Add("moduleobject.c", "unused-parameter", "missing-field-initializers", "pedantic");
                this.Add("enumobject.c", "missing-field-initializers");
                this.Add("object.c", "unused-parameter", "missing-field-initializers", "format-pedantic");
                this.Add("complexobject.c", "extended-offsetof");
                this.Add("capsule.c", "missing-field-initializers");
                this.Add("typeobject.c", "extended-offsetof");
                this.Add("obmalloc.c", "unused-parameter", "format-pedantic");
                this.Add("unicodeobject.c", "unused-function");
                this.Add("structseq.c", "missing-field-initializers");
                this.Add("bytearrayobject.c", "unused-function");
                this.Add("bytes_methods.c", "unused-parameter", "missing-field-initializers");
                this.Add("bytesobject.c", "unused-function");
                this.Add("abstract.c", "unused-parameter");
                this.Add("exceptions.c", "unused-parameter", "missing-field-initializers");
            }
        }

        sealed class PythonLibraryPython :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryPython()
            {
                this.Add("codecs.c", "unused-parameter", "missing-field-initializers");
                this.Add("formatter_unicode.c", "unused-parameter");
                this.Add("compile.c", "unused-parameter");
                this.Add("ast.c", "unused-parameter");
                this.Add("thread.c", "missing-field-initializers");
                this.Add("symtable.c", "unused-parameter", "missing-field-initializers");
                this.Add("pyfpe.c", "unused-parameter");
                this.Add("traceback.c", "unused-parameter", "missing-field-initializers");
                this.Add("Python-ast.c", "missing-field-initializers");
                this.Add("pythonrun.c", "unused-parameter");
                this.Add("import.c", "unused-parameter", "missing-field-initializers");
                this.Add("pytime.c", "unused-parameter");
                this.Add("random.c", "unused-parameter", "missing-field-initializers");
                this.Add("graminit.c", "missing-field-initializers");
                this.Add("sysmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("bltinmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("marshal.c", "unused-parameter", "missing-field-initializers");
                this.Add("getargs.c", "unused-parameter");
                this.Add("_warnings.c", "unused-parameter", "missing-field-initializers");
                this.Add("peephole.c", "unused-parameter");
                this.Add("ceval.c", "unused-parameter");
                this.Add("pylifecycle.c", "unused-parameter", "unused-function");
            }
        }

        sealed class PythonLibraryBuiltinModules :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryBuiltinModules()
            {
                this.Add("pwdmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("signalmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("gcmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("posixmodule.c", "unused-parameter", "missing-field-initializers", "implicit-function-declaration", "unused-function");
                this.Add("errnomodule.c", "missing-field-initializers");
                this.Add("_sre.c", "unused-parameter", "missing-field-initializers");
                this.Add("_codecsmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("_weakref.c", "unused-parameter");
                this.Add("_functoolsmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("_operator.c", "unused-parameter", "missing-field-initializers");
                this.Add("_collectionsmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("itertoolsmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("atexitmodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("_stat.c", "unused-parameter", "missing-field-initializers");
                this.Add("timemodule.c", "unused-parameter", "missing-field-initializers", "unused-function");
                this.Add("_localemodule.c", "unused-parameter", "missing-field-initializers");
                this.Add("zipimport.c", "unused-parameter", "missing-field-initializers");
                this.Add("faulthandler.c", "unused-parameter", "missing-field-initializers");
                this.Add("_tracemalloc.c", "unused-parameter", "missing-field-initializers");
                this.Add("hashtable.c", "unused-parameter");
                this.Add("symtablemodule.c", "unused-parameter", "missing-field-initializers");
            }
        }

        sealed class PythonLibraryCJKCodecs :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryCJKCodecs()
            {
            }
        }

        sealed class PythonLibraryIO :
            C.SuppressWarningsDelegate
        {
            public PythonLibraryIO()
            {
                this.Add("_iomodule.c", "unused-parameter", "missing-field-initializers", "overlength-strings");
                this.Add("fileio.c", "unused-parameter", "missing-field-initializers");
                this.Add("textio.c", "unused-parameter", "missing-field-initializers");
                this.Add("bufferedio.c", "unused-parameter", "missing-field-initializers");
                this.Add("iobase.c", "unused-parameter", "missing-field-initializers");
                this.Add("bytesio.c", "unused-parameter", "missing-field-initializers");
                this.Add("stringio.c", "unused-parameter", "missing-field-initializers");
            }
        }
    }
}
