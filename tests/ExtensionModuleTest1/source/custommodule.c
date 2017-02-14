#include "Python.h"

PyDoc_STRVAR(custommodule_docstring,
    "This is an example custom module.");

static PyMethodDef custommodule_functions[] = {
    { NULL, NULL } /* sentinel */
};

static struct PyModuleDef custommodule = {
    PyModuleDef_HEAD_INIT,
    "custommodule",
    custommodule_docstring,
    -1,
    custommodule_functions,
    NULL,
    NULL,
    NULL,
    NULL
};

PyMODINIT_FUNC
PyInit_custommodule()
{
    PyObject *module = PyModule_Create(&custommodule);
    if (NULL == module)
        return NULL;

    return module;
}
