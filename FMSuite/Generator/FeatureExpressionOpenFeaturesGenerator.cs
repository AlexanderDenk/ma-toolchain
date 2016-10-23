using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Creates a file containing a list of all features present in the model separated by unix newlines.
    /// </summary>
    sealed class FeatureExpressionOpenFeaturesGenerator : AbstractGenerator
    {

        /// <inheritDoc/>
        public FeatureExpressionOpenFeaturesGenerator(FeatureModel featureModel) : base(featureModel) { }

        /// <inheritDoc/>
        public override void Generate(string targetFile)
        {

            /* Ensure the stream is closed and everything is flushed. We write every feature of the model in a single line. */
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(targetFile))
            {
                foreach (string feature in this.featureModel.GetFeatures().OrderBy(feature => feature))
                {
                    file.Write($"{feature}{AbstractGenerator.NEWLINE}");
                }
            }
        }

    }

}
