PhoneNumbers C# Library
=======================

This is a C# port of libphonenumber, originally from:
  http://code.google.com/p/libphonenumber/.

Original Java code is Copyright (C) 2009-2011 Google Inc.

Current Repository for google's libphonenumber is:
https://github.com/googlei18n/libphonenumber


Project Layout
--------------

The project is a copy of the original libphonenumber with C# code
added in csharp/ root directory, and build tools added.
The intent is to keep pulling from the main repository and update the 
C# port accordingly.

lib/
  NUnit, Google.ProtoBuffersLite binaries and various conversion
  scripts. These are mostly deprecated at this point, with the exception
  of PomUtil.fsx, which is used to extract the version information from the
  java POM.XML file to produce the assembly version number. 

PhoneNumbers/
  Port of libphonenumber Java library

PhoneNumbers.Test/
  Port of libphonenumber Java tests in NUnit format.


Building
--------

Run build.bat from the project root. This will automatically update the
assembly version, and pull in the latest metadata files before doing the build.

There are two versions of the binary written to the .\build directory, one
is signed, the other is not.

Running Tests
-------------

Tests are run as part of the default build (when running the build from build.bat). 
You can also run only the tests by executing:
.\build.bat test

You can also run the tests from within Visual Studio if you have the NUnit Test Adapter
installed (https://visualstudiogallery.msdn.microsoft.com/6ab922d0-21c0-4f06-ab5f-4ecd1fe7175d)

Known Issues
------------

- Phone numbers metadata is read from XML files and not protocol
  buffers.
- Some of the geocoding stuff probably doesn't work

Porting New Versions
--------------------

To update the port:

Assuming the "master" branch is used to maintain/track changes from the primary
google repository, and you have a git remote named "google" set up to point to
http://github.com/googleil8n/libphonenumber

Execute:

```
git pull google master # Gets the latest from google
git push origin master # Writes the latest back up to the origin master branch
git checkout csharp
git merge master -X theirs --squash # Gets everything from google, but doesn't commit yet
```

At this point if there are changes to any of the .java files from master you will need to 
manually port those changes over. Otherwise there is nothing to do

```
.\build     # Does a full build, updates the Metadata, changes assembly version to match what is in pom.xml
            # runs the tests, and puts the result in .\build
git commit -am "Some clever commit message"
git checkout master
git clean -df
```