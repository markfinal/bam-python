# [![BuildAMation](http://buildamation.com/BAM-small.png)](https://github.com/markfinal/BuildAMation) Python package repository

This repository only contains the Bam build scripts for Python:

* Python-3.5.1
* Python-3.6.1

Download the [Python source](https://www.python.org/downloads/) and extract into the appropriate package directory in order to use it.

Builds have been tested on Windows, Linux and macOS.

All extension modules should be built on all platforms.
The exceptions are `_ssl`, `_hashlib` and `_sqlite3`, which require external libraries (OpenSSL and Sqlite).

If OpenSSL exists in your master package dependencies (of which Python is already one), then add the following define to your master package definition file, to build and publish `_ssl` and `_hashlib` extension modules:
* `PYTHON_WITH_OPENSSL`

If sqlite exists in your master package dependencies (of which Python is already one), then add the following define to your master package definition file, to build and publish `_sqlite3` extension module:
* `PYTHON_WITH_SQLITE`

Note that the build of Sqlite must not define `SQLITE_OMIT_DEPRECATED`.

If zlib exists in your master package dependencies (of which Python is already one), then add the following define to your master package definition file, to use instead of the included zlib source with Python:
* `PYTHON_USE_ZLIB_PACKAGE`

Test packages exist for
* Exercising an interpreter core
* Building a shell, and publishing a standard Python module distribution
* Building a custom extension module, and including it in a Python distribution
