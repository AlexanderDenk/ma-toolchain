# ma-toolchain

The tools are written for .NET but should basically support mono as well. The code is in C#, some parts may be written in F#.

The suite offers different tools for the following tasks:

* **FMSuite:** Converts a meta-feature model to optimized SPLConqueror xml based models and feature expression models suitable for TypeChef. The conversion is based on transformation rules and does not validate the expressions itself. It also generates the openfeatures.txt and config.h for TypeChef. Moreover it generates a FeatureIDE compliant XML feature model output that can be used to generate a visualization of the model (this may be improved in future). The input are a project file and the xfm-model.
TODO: Add Validation Feature: Precalculate Configs, Validate Configs etc.
* **PreAnalysisSuite**
    * *Metrics:* Extracts simple code metrics like LoC, lines of code per file.
    * *Features:* Extract and group features, feature expressions and try to find common prefixes.
* **AnalysisSuite:**
    * *Filelist:* Create filelists for TypeChef. 
        * *MakefileFiles:* Uses the make process to get the file list.
        * *AllFiles:* Extracts all files from the project.
    * *Run:* Run a typechef analysis
* **BenchmarkSuite:**
    * *Prepare:* Runs SPLConqueror config generation, creates the reporting database with the configurations.
    * *Run:* Run a benchmark analysis
* **ReportingSuite:**
    * *ExportConfigurations:* Exports the configurations to a file.
    * *ExportExclusions:* Exports the configurations to exclude file.
    * *ExportFeatureFailureStatistics:* Exports number of configurations for each feature that was damaged.
    * *ExportMeasurements:* Exports the measurements to a file.
* **UI:**
