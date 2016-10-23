using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Creates a header file with the mandatory features to the corresponding feature expression model for TypeChef. 
    /// </summary>
    sealed class FeatureExpressionHeaderFileGenerator : AbstractGenerator
    {

        /// <inheritDoc/>
        public FeatureExpressionHeaderFileGenerator(FeatureModel featureModel) : base(featureModel) { }

        /// <inheritDoc/>
        public override void Generate(string targetFile)
        {

            /* Ensure the stream is closed and everything is flushed. We write every mandatory feature in a single line containing a #define pre-procesor directive. */
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(targetFile))
            {
                foreach (string mandatoryFeature in this.featureModel.GetMandatory().OrderBy(feature => feature))
                {
                    file.Write($"#define {mandatoryFeature}{AbstractGenerator.NEWLINE}");
                }
            }
        }

    }

}
