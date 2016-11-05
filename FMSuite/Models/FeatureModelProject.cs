using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Models
{

    /// <summary>
    ///     Representation of the feature model project.
    ///     Contains paths of input and output files.
    /// </summary>
    sealed class FeatureModelProject
    {

        /// <summary>
        ///     Creates a model representing the project.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="metaModelFile">The file containing the meta-model.</param>
        /// <param name="splConquerorModelFile">The file containing the xml model.</param>
        /// <param name="typeChefModelFile">The file containing the feature expression model.</param>
        /// <param name="typeChefHeaderFile">The file containing all mandatory features as "#define" pragmas.</param>
        /// <param name="typeChefOpenFeaturesFile">The file containing all available features, that are not mandatory.</param>
        public FeatureModelProject(string name, string metaModelFile, string splConquerorModelFile, string typeChefModelFile, string typeChefHeaderFile, string typeChefOpenFeaturesFile)
        {
            this.Name = name;
            this.MetaModelFile = metaModelFile;
            this.SPLConquerorModelFile = splConquerorModelFile;
            this.TypeChefModelFile = typeChefModelFile;
            this.TypeChefHeaderFile = typeChefHeaderFile;
            this.TypeChefOpenFeaturesFile = typeChefOpenFeaturesFile;
        }

        /// <summary>
        ///     The name of the project.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     The file containing the meta-model.
        /// </summary>
        public string MetaModelFile { set; get; }

        /// <summary>
        ///     The file containing the xml model.
        /// </summary>
        public string SPLConquerorModelFile { set; get; }

        /// <summary>
        ///     The file containing the feature expression model.
        /// </summary>
        public string TypeChefModelFile { set; get; }

        /// <summary>
        ///     The file containing all mandatory features as "#define" pragmas.
        /// </summary>
        public string TypeChefHeaderFile { set; get; }

        /// <summary>
        ///     The file containing all available features, that are not mandatory.
        /// </summary>
        public string TypeChefOpenFeaturesFile { set; get; }

    }

}
