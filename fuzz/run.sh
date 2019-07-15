#!/bin/bash

afl-fuzz -i testcases -o findings -m 10000 dotnet gfoidl.Base64.FuzzTests/bin/Debug/netcoreapp3.0/gfoidl.Base64.FuzzTests.dll $*
