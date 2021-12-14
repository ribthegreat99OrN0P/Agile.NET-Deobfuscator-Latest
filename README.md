Agile.NET-Deobfuscator-Updated

Deobfuscates Agile.Net Obfuscator, the supported modules are:
Module | Progress | Comments | Status
:------------ | :-------------| :-------------| :-------------
String Encryption | Finished |  Maybe move towards emulation | ✔️
Proxy Delegates | Finished |  N/A | ✔️
Control-Flow | Not Finished |  started | :x:
Virtualization | Not Finished |  Will maybe implement | :x:
Code-Encryption | Not Finished |  Will implement last | :x:


Improvements from original version:
- More reliable string decryption (doesnt need to check for names)
- Accurate external reference resolving (gets the runtime for that specific program and resolves the memberreference)
- Cleans junk as the modules progress (e.g string decryption also removes the improper struct which will cause asmres to not be able to write the file, and removes junk delegate types)
- Will move more towards emulation soon




Credits:
Huge thank you for Washi's https://github.com/Washi1337/AsmResolver and https://github.com/Washi1337/Echo
