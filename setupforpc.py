# =============================================================================
#     Author: K Perkins
#     Date:   Jul 25, 2013
#     Taken From: http://programmingnotes.freeweq.com/
#     File:  setup.py
#     Description: This is the cx_Freeze setup file for creating an exe program
# =============================================================================
from cx_Freeze import setup, Executable
# NOTE: you can include any other necessary external imports here aswell

includefiles = [] # include any files here that you wish
includes = []
excludes = []
packages = []

exe = Executable(
 # what to build
   script = "Main.py", # the name of your main python script goes here
   initScript = None,
   base = None, # if creating a GUI instead of a console app, type "Win32GUI"
   targetName = "Terminator.exe", # this is the name of the executable file
   copyDependentFiles = True,
   compress = True,
   appendScriptToExe = True,
   appendScriptToLibrary = True,
   icon = None # if you want to use an icon file, specify the file name here
)

setup(
 # the actual setup & the definition of other misc. info
    name = "Terminator", # program name
    version = "0.1",
    description = 'A general enhancement utility',
    author = "K Perkins",
    author_email = "admin@programmingnotes.freeweq.com",
    options = {"build_exe": {"excludes":excludes,"packages":packages,
      "include_files":includefiles}},
    executables = [exe]
)
# http://programmingnotes.freeweq.com/
