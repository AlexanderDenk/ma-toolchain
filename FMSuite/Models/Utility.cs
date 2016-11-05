using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Models
{

    /// <summary>
    ///     Contains utility methods for feature model conversions.
    /// </summary>
    sealed class Utility
    {

        /// <summary>
        ///     The file counter. Basically allows parallel calls without collisions.
        ///     The class is not yet fully thread-safe.
        /// </summary>
        private static int expressionFileCounter = 0;

        /// <summary>
        ///     The main expression in the temporary file. This must be the same name as in CNFGenerator.py!
        ///     The parameter is the expression.
        /// </summary>
        private const string EXPRESSION_PATTERN = "Main_Exp: {0}";

        /// <summary>
        ///     The name of the input file. The parameter is the current file count.
        /// </summary>
        private const string EXPRESSION_FILE_NAME_INPUT = "tmp/convert.{0}.in.expression";

        /// <summary>
        ///     The name of the output file. The parameter is the current file count.
        /// </summary>
        private const string EXPRESSION_FILE_NAME_OUTPUT = "tmp/convert.{0}.out.expression";

        /// <summary>
        ///     The temporary directory.
        /// </summary>
        private const string TEMPORARY_FOLDER = "tmp/";

        /// <summary>
        ///     The name of the variable that is passed to PBL and contains the input file path.
        /// </summary>
        private const string COMMUNICATION_PBL_FILE_INPUT = "expressionFileInput";

        /// <summary>
        ///     The name of the variable that is passed to PBL and contains the output file path.
        /// </summary>
        private const string COMMUNICATION_PBL_FILE_OUTPUT = "expressionFileOutput";

        /// <summary>
        ///     Path to the PBL frontend (TODO: Setting!).
        /// </summary>
        private const string PBL_FRONTEND = @"../../External/CNFGenerator.py";

        /// <summary>
        ///     Path to the PBL library (TODO: Setting!).
        /// </summary>
        private const string PBL_LIBRARY = @"../../External/PBL/include";

        /// <summary>
        ///     Path to the Python Standard Library (TODO: Setting!).
        /// </summary>
        private const string PYTHON_LIBRARY = @"../../Lib";

        /// <summary>
        ///     The character used for representing disjunctions.
        /// </summary>
        private const char BOOL_DISJUNCTION = '|';

        /// <summary>
        ///     The character used for representing conjunctions.
        /// </summary>
        private const char BOOL_CONJUNCTION = '&';

        /// <summary>
        ///     The negation character used by the PBL library.
        /// </summary>
        private const char BOOL_NEGATION_PBL = '~';

        /// <summary>
        ///     The universal negation character.
        /// </summary>
        private const char BOOL_NEGATION = '!';

        /// <summary>
        ///     Utility class.
        /// </summary>
        private Utility() { }

        /// <summary>
        ///     Takes a configuration (that's represented as array of features names) and creates a CNF compliant clause:
        ///     !(A & B & C & D & E) will be transformed to: (!A | !B | !C | !D | !E)
        ///     This can unintentionally exclude configurations like (A & B & C & D & E & X). So the exclusion expression has to be extended with all other
        ///     possible features resulting in  (!A | !B | !C | !D | !E | X | Y | Z).
        /// </summary>
        /// <param name="features">All features present in the model. This must be a superset of excludedConfiguration.</param>
        /// <param name="excludedConfiguration">The features of the configuration to exclude. This must not be empty and must be a subset of features.</param>
        /// <returns>The exclusion clause as string representing a boolean expression.</returns>
        public static string ConvertExcludedConfigurationToCNFCompliantExpression(IEnumerable<string> features, IEnumerable<string> excludedConfiguration)
        {
            string negativeFeaturesClause = excludedConfiguration.Select(feature => $"{Utility.BOOL_NEGATION}{feature}").Aggregate((current, next) => $"{current} {Utility.BOOL_DISJUNCTION} {next}");
            string positiveFeaturesClause = features.Except(excludedConfiguration).Select(feature => feature).Aggregate((current, next) => $"{current} {Utility.BOOL_DISJUNCTION} {next}");
            return negativeFeaturesClause + (String.IsNullOrWhiteSpace(positiveFeaturesClause) ? "" : $" {Utility.BOOL_DISJUNCTION} {positiveFeaturesClause}");
        }

        /// <summary>
        ///     Resolves an alternative group (oneOf()) to an boolean expression.
        /// </summary>
        /// <param name="features">The alternative group.</param>
        /// <returns>A resolved one of.</returns>
        public static string ConvertOneOfToCNFCompliantExpression(IEnumerable<string> features)
        {
            IList<string> oneFeatureActiveGroups = new List<string>();

            /* Generate for every feature a group that look's like the following: one AND NOT all others. */
            foreach (string activeFeature in features)
            {
                oneFeatureActiveGroups.Add(features.Select(feature => (activeFeature == feature) ? feature : $"{Utility.BOOL_NEGATION}{feature}")
                        .Aggregate((current, next) => $"{current} {Utility.BOOL_CONJUNCTION} {next}"));
            }
            return string.Join(Utility.BOOL_DISJUNCTION.ToString(), oneFeatureActiveGroups.Select(group => $" ({group}) ")).Trim(); 
        }

        /// <summary>
        ///     Converts an boolean expression into its CNF. This uses the Python library PBL and expressions are exchanged via the file system.
        ///     This may decrease performance.
        /// 
        ///     NOTES:
        ///     - PBL used "is" for string comparisons. That's terribly wrong because id(a) == id(b) IS NOT NECESSARILY the same as a == b
        /// </summary>
        /// <param name="expression">The expression to convert in CNF.</param>
        /// <returns>A list of terms. The items are conjuncted.</returns>
        public static IEnumerable<string> ConvertToCNF(string expression)
        {

            /* Store the expression in a file. This is neceassy because we don't want to touch the PBL wich only offers support for files. */
            int currentFileIndex = Utility.expressionFileCounter++;
            string expressionFileInput = string.Format(Utility.EXPRESSION_FILE_NAME_INPUT, currentFileIndex);
            string expressionFileOutput = string.Format(Utility.EXPRESSION_FILE_NAME_OUTPUT, currentFileIndex);

            /* Write the input file. */
            Directory.CreateDirectory(Utility.TEMPORARY_FOLDER);
            using (StreamWriter file = new StreamWriter(expressionFileInput))
            {
                file.WriteLine(string.Format(Utility.EXPRESSION_PATTERN, expression));
            }

            /* We need to enable frames and full-frames options for PBL, otherwise we get an exception. */
            IDictionary<string, object> options = new Dictionary<string, object>();
            options["Frames"] = true;
            options["FullFrames"] = true;
            ScriptEngine engine = Python.CreateEngine(options);

            /* Setup the search paths for the libraries. */
            ICollection<string> paths = engine.GetSearchPaths();
            paths.Add(Utility.PYTHON_LIBRARY);
            paths.Add(Utility.PBL_LIBRARY);
            engine.SetSearchPaths(paths);

            /* Pass the variables containing the files and run the conversion. */
            ScriptScope scope = engine.CreateScope();
            scope.SetVariable(Utility.COMMUNICATION_PBL_FILE_INPUT, expressionFileInput);
            scope.SetVariable(Utility.COMMUNICATION_PBL_FILE_OUTPUT, expressionFileOutput);
            engine.ExecuteFile(Utility.PBL_FRONTEND, scope);

            /* Read the result. */
            IEnumerable<string> terms = File.ReadAllText(expressionFileOutput).Split(Utility.BOOL_CONJUNCTION)
                    .Select(term => term.Trim().Replace(Utility.BOOL_NEGATION_PBL, Utility.BOOL_NEGATION))
                    .Select(term => ((term.First() == '(') && (term.Last() == ')')) ? term.Substring(1, term.Length - 2) : term);

            /* Delete the files. */
            File.Delete(expressionFileInput);
            File.Delete(expressionFileOutput);

            /* Return the terms. */
            return terms;
        }

    }

}
