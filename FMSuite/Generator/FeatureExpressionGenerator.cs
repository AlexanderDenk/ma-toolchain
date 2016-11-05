using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Generates the feature expression model output.
    ///     The output consists of:
    ///     - All mandatory features
    ///     - All alternative groups
    ///     - All boolean constraints, including the excluded configurations
    /// </summary>
    sealed class FeatureExpressionGenerator : AbstractGenerator
    {

        /// <summary>
        ///     The replacement for feature definition expressions.
        /// </summary>
        private const string DEFINITION_REPLACEMENT = "defined($1)";

        /// <summary>
        ///     The pattern of a feature name.
        /// </summary>
        private const string PATTERN_FEATURE = "(\\w+)";

        /// <inheritDoc/>
        public FeatureExpressionGenerator(FeatureModel featureModel) : base(featureModel) { }

        /// <inheritDoc/>
        public override void Generate(string targetFile)
        {

            /* Ensure the stream is closed and everything is flushed. */
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(targetFile))
            {
                this.WriteMandatories(file);
                this.WriteAlternatives(file);
                this.WriteConstraints(file);
            }
        }

        /// <summary>
        ///     Write the mandatory features
        /// </summary>
        /// <param name="file">The output file.</param>
        private void WriteMandatories(StreamWriter file)
        {
            foreach (string feature in this.featureModel.GetMandatory())
            {
                file.WriteLine(this.AddDefined(feature));
                file.WriteLine();
            }
        }

        /// <summary>
        ///     Write the boolean constraints. We need to transform the features into defined(feature) expressions.
        /// </summary>
        /// <param name="file">The output file.</param>
        private void WriteConstraints(StreamWriter file)
        {
            foreach (string constraint in this.featureModel.GetConstraints())
            {
                file.WriteLine(this.AddDefined(constraint));
                file.WriteLine();
            }
        }

        /// <summary>
        ///     Write the alternative groups.
        /// </summary>
        /// <param name="file">The output file.</param>
        private void WriteAlternatives(StreamWriter file)
        {
            IDictionary<string, ISet<string>> alternatives = this.featureModel.GetAlternatives();
            foreach (KeyValuePair<string, ISet<string>> entry in alternatives)
            {
                string alternativeGroups = Utility.ConvertOneOfToCNFCompliantExpression(entry.Value);
                string alternative = $"{entry.Key} => {alternativeGroups}";
                file.WriteLine(this.AddDefined(alternative));
                file.WriteLine();
            }
        }

        /// <summary>
        ///     Wraps around each term of a feature "defined()" So "FEATURE" will be transformed to "defined(FEATURE)".
        /// </summary>
        /// <param name="expression">The expression to wrap it's features.</param>
        private string AddDefined(string expression)
        {
            Regex feature = new Regex(FeatureExpressionGenerator.PATTERN_FEATURE);
            return feature.Replace(expression, FeatureExpressionGenerator.DEFINITION_REPLACEMENT);
        }

    }

}
