#include "Python.h"

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
    PyRun_SimpleString("from time import time,ctime\n"
                       "print('Today is',ctime(time()))\n");
    Py_Finalize();
    return 0;
}
