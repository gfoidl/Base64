#!/bin/bash
#
## Helper for running Benchmarks. Per default Benchmarks are run on .NET Core 3.1.
#  If Benchmarks should be run for .NET 4.6.1 the Environment Variables
#  BENCH_FX must be set (see below).
#
# Environment Variables:
#   BENCH_FX            netcoreapp3.1 (default) or net461
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

tfm=${BENCH_FX:-netcoreapp3.1}

echo ""
echo "Running benchmarks with tfm: $tfm"
echo "-------------------------------------------------"

if [[ "$tfm" == "netcoreapp3.1" ]]; then
    echo "installed sdks:"
    dotnet --list-sdks
    echo "-------------------------------------------------"
fi

cd perf/gfoidl.Base64.Benchmarks
dotnet build -c Release -f "$tfm"

cd bin/Release/$tfm
exe=gfoidl.Base64.Benchmarks
cmd=

if [[ -f "$exe" || -f "$exe".exe ]]; then
    cmd="./$exe"
    echo "using exe"
else
    cmd="dotnet ./$exe.dll"
    echo "using dotnet command"
fi

$cmd --list tree

#$cmd -f *Base64EncoderBenchmark*
#$cmd -f *Base64UrlEncoderBenchmark*
#$cmd -f *DecodeStringBenchmark*
#$cmd -f *DecodeStringUrlBenchmark*
#$cmd -f *DecodeUtf8Benchmark*
#$cmd -f *EncodeStringBenchmark*
#$cmd -f *EncodeStringUrlBenchmark*
$cmd -f *EncodeUtf8Benchmark*

if [[ "$tfm" == "netcoreapp3.1" ]]; then
    $cmd -f *ReadOnlySequenceBase64Benchmark*
    $cmd -f *ReadOnlySequenceBase64UrlBenchmark*
fi
