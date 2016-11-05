using FMSuite.Generator;
using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FMSuite.Parser
{

    /// <summary>
    /// 
    ///     This class parses an extended feature expression model that's basically based on the feature expression model.
    ///     An extended feature expression model is used as base model for the generation of a normal feature expression model
    ///     and a SPLConqueror feature model.
    /// 
    ///     This could also be build using JSON or XML. To keep the things more readable the simple text annotation made the game.
    /// 
    ///     Extended Feature Models are build upon the following processing rules/directives:
    ///         - Blank lines will be ignored
    ///         - %% COMMENT:               Introduces a comment line
    ///         - mandatory FEATURE:       Declares a mandatory feature
    ///         - optional FEATURE:        Declares an optional feature
    ///         - declared FEATURE:        Declares a feature, thats not fully optional or mandatory but used in expressions
    ///         - constraint EXPRESSION:   Creates an boolean expression* 
    ///         - excluded FEATURE, ...:   Declares an excluded configuration
    ///                                    For performance reasons this statement cannot be followed by other statements than %exclude
    ///         - alternative PARENT, FEATURE, ... Declares an alternative group of features, where PARENT is the parent feature
    /// 
    ///     Constraint: The "excluded"-directive is parsed reasons only at the end of the model (all features have to be known, for performance reasons).
    /// 
    /// </summary>
    sealed class ExtendedFeatureModelParser
    {

        /// <summary>
        ///     The prefix of comment lines.
        /// </summary>
        const string DIRECTIVE_COMMENT = "%%";

        /// <summary>
        ///     Prefix for mandatory feature definition.
        /// </summary>
        const string DIRECTIVE_MANDATORY = "mandatory";

        /// <summary>
        ///     Prefix for optional feature definition.
        /// </summary>
        const string DIRECTIVE_OPTIONAL = "optional";

        /// <summary>
        ///     Prefix for feature definition used in expressions.
        /// </summary>
        const string DIRECTIVE_DECLARED = "declared";

        /// <summary>
        ///     Prefix for constraint definition.
        /// </summary>
        const string DIRECTIVE_CONSTRAINT = "constraint";

        /// <summary>
        ///     Prefix for exluded configuration.
        /// </summary>
        const string DIRECTIVE_EXCLUDED = "excluded";

        /// <summary>
        ///     Prefix for alternative groups.
        /// </summary>
        const string DIRECTIVE_ALTERNATIVE = "alternative";

        /// <summary>
        ///     Error message if a directive is not defined and connot be processed.
        /// </summary>
        const string ERROR_DIRECTIVE_UNKNOWN = "Unknown directive.";

        /// <summary>
        ///     Error message if a feature was expected but not found.
        /// </summary>
        const string ERROR_FEATURE_EXPECTED = "A feature was expected.";

        /// <summary>
        ///     Error message if an exclude directive occured and was followed by another directive.
        /// </summary>
        const string ERROR_ONLY_EXCLUDES_ALLOWED = "The feature model can only contain exclude-statements after the first occurence of an exclude statement.";

        /// <summary>
        ///     Separation character in a list of features.
        /// </summary>
        const char SEPARATOR_FEATURES = ' ';

        /// <summary>
        ///     This is set to true on first exclude line. Any other line following will create an exception.
        /// </summary>
        private bool isExclusionModeStarted = false;

        /// <summary>
        ///     Pattern for valid names of features.
        /// </summary>
        private Regex patternFeature = new Regex("^[A-Za-z0-9_]+$");

        /// <summary>
        ///     The feature model representing the parsed meta-model.
        /// </summary>
        private FeatureModel model = null;

        /// <summary>
        ///     Parses the input file.
        /// </summary>
        /// <param name="inputPath">The path of the file to parse.</param>
        /// <param name="name">The name of the study.</param>
        /// <exception cref="InvalidDataException">Thrown if the model couldn't be processed.</exception>
        public ExtendedFeatureModelParser(string inputPath, string name)
        {
            this.model = new FeatureModel(name);
            this.Parse(inputPath);
        }
        
        /// <summary>
        ///     Returns the model representing the parsed file.
        /// </summary>
        /// <returns></returns>
        public FeatureModel GetModel()
        {
            return this.model;
        }

        /// <summary>
        ///     Parses the whole extended feature model file 
        /// </summary>
        /// <param name="inputPath"></param>
        /// <exception cref="InvalidDataException">Thrown if the model couldn't be processed.</exception>
        private void Parse(string inputPath)
        {
            using (StreamReader file = new StreamReader(inputPath))
            {
                string line = null;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0)
                    {
                        continue;
                    }
                    this.ParseLine(line.Trim());
                }
            }
        }

        /// <summary>
        ///     Parses a single line of the extended feature expression model.
        ///     The parser will determine...
        /// </summary>
        /// <param name="line">The content of the line to parse.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ParseLine(string line)
        {

            /* Guard: The FM must not contain non-exclude constraints after the occurence of the first exclude constraint. */
            if (!line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_EXCLUDED) && this.isExclusionModeStarted)
            {
                throw new InvalidDataException(ExtendedFeatureModelParser.ERROR_ONLY_EXCLUDES_ALLOWED);
            }

            /* Because the generated model should only be read by machines comments will be removed. */
            if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_COMMENT))
            {
                return;
            }

            /* Process all known directives. */
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_MANDATORY))
            {
                this.ProcessMandatory(line);
            }
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_OPTIONAL))
            {
                this.ProcessOptional(line);
            }
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_DECLARED))
            {
                this.ProcessDeclared(line);
            }
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_ALTERNATIVE))
            {
                this.ProcessAlternative(line);
            }
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_CONSTRAINT))
            {
                this.ProcessConstraint(line);
            }
            else if (line.StartsWith(ExtendedFeatureModelParser.DIRECTIVE_EXCLUDED))
            {
                this.ProcessExcluded(line);
            }
            
            /* The directive is unknown. */
            else
            {
                throw new InvalidDataException(ExtendedFeatureModelParser.ERROR_DIRECTIVE_UNKNOWN);
            }
        }

        /// <summary>
        ///     Parses a mandatoy directive. The directive must contain a feature not seen before.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessMandatory(string line)
        {
            string feature = this.ExtractFeature(line, ExtendedFeatureModelParser.DIRECTIVE_MANDATORY);
            this.model.AddMandatory(feature);
        }

        /// <summary>
        ///     Parses a optional directive. The directive must contain a feature not seen before.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessOptional(string line)
        {
            string feature = this.ExtractFeature(line, ExtendedFeatureModelParser.DIRECTIVE_OPTIONAL);
            this.model.AddOptional(feature);
        }

        /// <summary>
        ///     Parses a declared directive. The directive must contain a feature not seen before.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessDeclared(string line)
        {
            string feature = this.ExtractFeature(line, ExtendedFeatureModelParser.DIRECTIVE_DECLARED);
            this.model.AddDeclared(feature);
        }

        /// <summary>
        ///     Parses a alternative directive. The alternative must contain a parent feature not seen before and known child features.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessAlternative(string line)
        {
            IEnumerable<string> features = line.Substring(ExtendedFeatureModelParser.DIRECTIVE_ALTERNATIVE.Length)
                    .Trim().Split(ExtendedFeatureModelParser.SEPARATOR_FEATURES)
                    .Select(feature => feature.Trim());
            this.model.AddAlternative(features.First(), features.Skip(1));
        }

        /// <summary>
        ///     Parses a constraint directive. The directive must contain a boolean expression made up of features seen before.
        ///     ...
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessConstraint(string line)
        {
            string constraint = line.Substring(ExtendedFeatureModelParser.DIRECTIVE_CONSTRAINT.Length).Trim();
            this.model.AddConstraint(constraint);
        }

        /// <summary>
        ///     Parses a excluded directive. The directive must contain a list of feature not seen before representing a configuration.
        ///     Therefore feature must be unique within the directive.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <exception cref="InvalidDataException">Thrown if the line couldn't be processed.</exception>
        private void ProcessExcluded(string line)
        {
            IEnumerable<string> features = line.Substring(ExtendedFeatureModelParser.DIRECTIVE_EXCLUDED.Length)
                    .Trim().Split(ExtendedFeatureModelParser.SEPARATOR_FEATURES)
                    .Select(feature => feature.Trim());
            this.model.AddExcluded(features);
            this.isExclusionModeStarted = true;
        }

        /// <summary>
        ///     Extracts the feature from a line.
        /// </summary>
        /// <param name="line">The line containing the directive.</param>
        /// <param name="directive">The directive to process.</param>
        /// <returns>The feature provided to the directive.</returns>
        /// <exception cref="InvalidDataException">Thrown if the feature seems to be invalid.</exception>
        private string ExtractFeature(string line, string directive)
        {
            string feature = line.Substring(directive.Length).Trim();
            if (!this.patternFeature.IsMatch(feature))
            {
                throw new InvalidDataException(ExtendedFeatureModelParser.ERROR_FEATURE_EXPECTED);
            }
            return feature;
        } 

    }

}
