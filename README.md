# ma-toolchain

The tools are written for .NET but should basically support mono as well. The code is in C#, some parts may be written in F#.

The suite offers different tools for the following tasks:

* **FMSuite**:
    * *Convert*: Converts a meta-feature model to optimized SPLConqueror xm based models and feature expression models suitable for TypeChef.
    * *Extract*: Generate open-features and config.h for TypeChef
* **PreAnalysisSuite**:
    * *Metrics*: Extract simple code metrics like LoC...
    * *Features*: Extract and group features, feature expressions and try to find common prefixes.
* **AnalysisSuite**:
    * *Filelist*: Create filelists for TypeChef
    * *Run*: Run a typechef analysis
* **BenchmarkSuite**
    * *Run*: Run a benchmark analysis
* **UI**
