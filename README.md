# ma-toolchain

The tools are written for .NET but should basically support mono as well. The code is in C#, some parts may be written in F#.

The suite offers different tools for the following tasks:

* **FMSuite:**
    * *Convert:* Converts a meta-feature model to optimized SPLConqueror xml based models and feature expression models suitable for TypeChef. The conversion is based on transformation rules and does not validate the expressions itself.
    * *Extract:* Generates open-features and config.h for TypeChef.
* **PreAnalysisSuite**
    * *Metrics:* Extracts simple code metrics like LoC, lines of code per file.
    * *Features:* Extract and group features, feature expressions and try to find common prefixes.
* **AnalysisSuite:**
    * *Filelist:* Create filelists for TypeChef
    * *Run:* Run a typechef analysis
* **BenchmarkSuite:**
    * *Prepare:* Runs SPLConqueror config generation, creates the reporting database with the configurations.
    * *Run:* Run a benchmark analysis
* **ReportingSuite:**
    * *ExportConfigurations:* Exports the configurations to a file.
    * *ExportMeasurements:* Exports the measurements to a file.
* **UI:**
