#include "Python.h"

#ifdef D_BAM_PLATFORM_WINDOWS
int wmain(int argc, wchar_t *argv[])
#else
int main(int argc, char *argv[])
#endif
{
    (void)argc;
    /*
    Py_VerboseFlag++;
    Py_VerboseFlag++;
    Py_VerboseFlag++;
    */
    wchar_t *programName = Py_DecodeLocale(argv[0], 0);
    Py_SetProgramName(programName);
    Py_Initialize();
    PyRun_SimpleString("from time import time,ctime\n"
                       "print('Today is',ctime(time()))\n");
    Py_Finalize();
    return 0;
}
