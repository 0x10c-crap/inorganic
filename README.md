Inorganic
========

Inorganic is a disassembler for DCPU-16 that allows you to produce basic disassemblies for any pre-assembled DCPU-16 code.

Usage
-----

If you are on Linux or Mac, install Mono first, and use "mono inorganic.exe \[arguments\]", like you'd do with a Java program.

    inorganic.exe \[flags\] inputfile.bin outputfile.dasm
    
This will disassemble the big-endian inputfile.bin and output it to outputfile.dasm. If you don't specify an output file, it will use \[inputfile\].dasm

**Flags**

*--little-endian*: Treats the input file as little endian.

*--unsigned*: For short literals, the default behavior is to treat them as signed and allow -1 as a value. This disabled that feature.
