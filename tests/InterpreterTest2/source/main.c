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
