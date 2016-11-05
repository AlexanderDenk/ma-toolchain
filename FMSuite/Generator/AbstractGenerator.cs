using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Abstract base class for generators. Contains the default constructor.
    /// </summary>
    abstract class AbstractGenerator : IGenerator
    {

        /// <summary>
        ///     We use for all files Unix line breaks instead of the platform dependent line breaks.
        /// </summary>
        protected const char NEWLINE = '\n';

        /// <summary>
        ///     The meta feature model to generate the export from.
        /// </summary>
        protected FeatureModel featureModel;

        /// <summary>
        ///     Initializes the generator.
        /// </summary>
        /// <param name="featureModel">The meta feature model to generate the export from.</param>
        public AbstractGenerator(FeatureModel featureModel)
        {
            this.featureModel = featureModel;
        }

        /// <inhertitDoc/>
        public abstract void Generate(string targetFile);

    }

}
