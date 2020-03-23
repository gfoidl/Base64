#!/bin/bash
#
## Helper for running Benchmarks. Per default Benchmarks are run on .NET Core 3.0.
#  If Benchmarks should be run for .NET 4.6.1 the Environment Variables
#  BENCH_FX must be set (see below).
#
# Environment Variables:
#   BENCH_FX            netcoreapp3.0 (default) or net461
#------------------------------------------------------------------------------
set -e
#------------------------------------------------------------------------------
if [[ "$1" == "--install-sdk" ]]; then
    echo "installing SDK $SDK_VERSION"
    echo ""

    curl -o dotnet-install.sh https://dot.net/v1/dotnet-install.sh
    mkdir dotnet
    chmod u+x ./dotnet-install.sh
    ./dotnet-install.sh --install-dir $(pwd)/dotnet -v $SDK_VERSION
    rm $(pwd)/dotnet-install.sh
    export PATH="$(pwd)/dotnet:$PATH"

    echo ""
fi

tfm=${BENCH_FX:-netcoreapp3.0}

echo ""
echo "Running benchmarks with tfm: $tfm"
echo "-------------------------------------------------"

if [[ "$tfm" == "netcoreapp3.0" ]]; then
    echo "installed sdks:"
    dotnet --list-sdks
    echo "-------------------------------------------------"
fi

cd perf/gfoidl.Base64.Benchmarks
dotnet build -c Release -f "$tfm"

cd bin/Release/$tfm
./gfoidl.Base64.Benchmarks --list tree

./gfoidl.Base64.Benchmarks -f *Base64EncoderBenchmark*
./gfoidl.Base64.Benchmarks -f *Base64UrlEncoderBenchmark*
./gfoidl.Base64.Benchmarks -f *DecodeStringBenchmark*
./gfoidl.Base64.Benchmarks -f *DecodeStringUrlBenchmark*
./gfoidl.Base64.Benchmarks -f *DecodeUtf8Benchmark*
./gfoidl.Base64.Benchmarks -f *EncodeStringBenchmark*
./gfoidl.Base64.Benchmarks -f *EncodeStringUrlBenchmark*
./gfoidl.Base64.Benchmarks -f *EncodeUtf8Benchmark*

if [[ "$tfm" == "netcoreapp3.0" ]]; then
    ./gfoidl.Base64.Benchmarks -f *ReadOnlySequenceBase64Benchmark*
    ./gfoidl.Base64.Benchmarks -f *ReadOnlySequenceBase64UrlBenchmark*
fi
