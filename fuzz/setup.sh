#!/bin/bash

set -e

aflVersion=2.52b

# Download and extract the afl-fuzz source package
wget https://lcamtuf.coredump.cx/afl/releases/afl-"$aflVersion".tgz
tar -xvf afl-"$aflVersion".tgz

rm afl-"$aflVersion".tgz
cd afl-"$aflVersion"/

# Patch afl-fuzz so that it doesn't check whether the binary
# being fuzzed is instrumented (we have to do this because
# we are going to run our programs with the dotnet run command,
# and the dotnet binary would fail this check)
wget https://github.com/Metalnem/sharpfuzz/raw/master/patches/RemoveInstrumentationCheck.diff
patch < RemoveInstrumentationCheck.diff

# Install afl-fuzz
sudo make install
cd ..
rm -rf afl-"$aflVersion"/

# Install SharpFuzz.CommandLine global .NET tool
dotnet tool install --global SharpFuzz.CommandLine
