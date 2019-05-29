from testconfigurations import TestSetup, visualc, visualc64, visualc32, mingw32, gcc, gcc64, gcc32, clang, clang32, clang64

def configure_repository():
    configs = {}
    #configs["ExtensionModuleTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
    #                                            linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
    #                                            osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    #configs["ExtensionModuleTest2"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
    #                                            linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
    #                                            osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    #configs["InterpreterTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
    #                                        linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
    #                                        osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    #configs["InterpreterTest2"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
    #                                        linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
    #                                        osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["ShellTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                      linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                      osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    #configs["ShellTest2"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
    #                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
    #                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    return configs
