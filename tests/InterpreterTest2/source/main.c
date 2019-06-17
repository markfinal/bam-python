/*
Copyright (c) 2010-2019, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#include "Python.h"
#include <stdio.h>

int
run_from_file()
{
    FILE *file = fopen("Main.py", "rt");
    int result = PyRun_SimpleFileExFlags(
        file,
        "Main.py",
        1,
        NULL
    );
    return result;
}

int
run_from_code()
{
    int result = PyRun_SimpleString("from time import time,ctime\n"
        "print('From embedded code, today is',ctime(time()))\n");
    return result;
}

int main(int argc, char *argv[])
{
    /*
    Py_VerboseFlag++;
    Py_VerboseFlag++;
    Py_VerboseFlag++;
    */
    wchar_t *programName = Py_DecodeLocale(argv[0], 0);
    (void)argc;
    Py_SetProgramName(programName);
    Py_Initialize();
    run_from_file();
    run_from_code();
    Py_Finalize();
    return 0;
}
