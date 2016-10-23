using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Generator
{

    /// <summary>
    /// 
    /// </summary>
    abstract class AbstractGenerator : IGenerator
    {

        /// <summary>
        ///     We use for all files Unix line breaks instead of the platform dependent line breaks.
        /// </summary>
        protected const char NEWLINE = '\n';

        /// <summary>
        /// 
        /// </summary>
        protected FeatureModel featureModel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureModel"></param>
        public AbstractGenerator(FeatureModel featureModel)
        {
            this.featureModel = featureModel;
        }

        /// <inhertitDoc/>
        public abstract void Generate(string targetFile);

    }

}
