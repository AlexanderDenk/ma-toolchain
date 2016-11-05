using FMSuite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Creates the SPLConqueror feature model export.
    /// </summary>
    sealed class SPLConquerorGenerator: AbstractGenerator
    {

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_ROOT = "vm";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_BINARY_OPTIONS = "binaryOptions";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_OPTION = "configurationOption";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_OPTION_CHILDREN = "children";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_OPTION_IMPLICATIONS = "impliedOptions";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_OPTION_EXCLUSIONS = "excludedOptions";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_RELATED_OPTION = "option";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_CONFIGURATION_RELATED_OPTION_E = "options";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_BOOLEAN_CONSTRAINTS = "booleanConstraints";

        /// <summary>
        /// 
        /// </summary>
        private const string ELEMENT_BOOLEAN_CONSTRAINT = "constraint";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_NAME = "name";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_OUTPUT = "outputString";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_PREFIX = "prefix";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_POSTFIX = "postfix";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_PARENT_FEATURE = "parent";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_DEFAULT_VALUE = "defaultValue";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_CONFIGURATION_OPTION_IS_OPTIONAL = "optional";

        /// <summary>
        /// 
        /// </summary>
        private const string ATTRIBUTE_ROOT_NAME = "name";

        /// <summary>
        /// 
        /// </summary>
        private const string FEATURE_ROOT = "root";

        /// <summary>
        /// 
        /// </summary>
        private const string XML_NAMESPACE_PREFIX = "";

        /// <inheritDoc/>
        public SPLConquerorGenerator(FeatureModel featureModel) : base(featureModel) { }

        /// <inheritDoc/>
        public override void Generate(string targetFile)
        {

            /* Xml-Writer Settings. */
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;
            settings.Indent = true;

            /* For performance reasons we do not build a DOM. */
            using (XmlWriter writer = XmlWriter.Create(targetFile, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(SPLConquerorGenerator.ELEMENT_ROOT);
                writer.WriteAttributeString(SPLConquerorGenerator.ATTRIBUTE_ROOT_NAME, SPLConquerorGenerator.XML_NAMESPACE_PREFIX, featureModel.Name);
                writer.WriteStartElement(SPLConquerorGenerator.ELEMENT_BINARY_OPTIONS);

                /* Root node entry. */
                this.GenerateBinaryFeature(writer, SPLConquerorGenerator.FEATURE_ROOT, false);
                this.GenerateMandatoryFeatures(writer);
                this.GenerateOptionalFeatures(writer);
                this.GenerateAlternatives(writer);
                writer.WriteEndElement();

                /* Handle Constraints. */
                writer.WriteStartElement(SPLConquerorGenerator.ELEMENT_BOOLEAN_CONSTRAINTS);
                this.GenerateConstraints(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        ///     Generates mandatory feature entries. The are all children of the roof feature and are set to non-optional.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        private void GenerateMandatoryFeatures(XmlWriter writer)
        {
            foreach (string mandatoryFeature in this.featureModel.GetMandatory())
            {
                this.GenerateBinaryFeature(writer, mandatoryFeature, false, SPLConquerorGenerator.FEATURE_ROOT);
            }
        }

        /// <summary>
        ///     Generates optional features. The are all children of the roof feature and are set to optional.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        private void GenerateOptionalFeatures(XmlWriter writer)
        {
            foreach (string optionalFeature in this.featureModel.GetOptional())
            {
                this.GenerateBinaryFeature(writer, optionalFeature, true, SPLConquerorGenerator.FEATURE_ROOT);
            }
        }

        /// <summary>
        ///     Generate an alternative group. 
        /// 
        ///     TODO: Support Optionals here?
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        private void GenerateAlternatives(XmlWriter writer)
        {
            foreach (KeyValuePair<string, ISet<string>> alternativesGroup in this.featureModel.GetAlternatives())
            {
                foreach (string feature in alternativesGroup.Value)
                {
                    this.GenerateBinaryFeature(writer, feature, true, alternativesGroup.Key, null, null, alternativesGroup.Value.Where(excludedFeature => (feature != excludedFeature)));
                }
            }
        }

        /// <summary>
        ///     Generate the boolean constraints.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        private void GenerateConstraints(XmlWriter writer)
        {
            foreach (string constraint in this.featureModel.GetConstraints())
            {
                writer.WriteElementString(SPLConquerorGenerator.ELEMENT_BOOLEAN_CONSTRAINT, constraint);
            }
        }

        /// <summary>
        ///     Writes a single binary option.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="name">The name of the option.</param>
        /// <param name="optional">Flag if the feature is optional.</param>
        /// <param name="parent">The parent option. Can by empty.</param>
        /// <param name="childFeatures">A list of child features.</param>
        /// <param name="implicatedFeatures">A list of implicated features.</param>
        /// <param name="excludedFeatures">A list of exckuded features.</param>
        private void GenerateBinaryFeature(XmlWriter writer, string name, bool optional, string parent = null, IEnumerable<string> childFeatures = null, IEnumerable<string> implicatedFeatures = null, IEnumerable<string> excludedFeatures = null)
        {

            /* Start the option. */
            writer.WriteStartElement(SPLConquerorGenerator.ELEMENT_CONFIGURATION_OPTION);

            /* Set the properties. */
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_NAME, name);
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_OUTPUT, name);
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_PREFIX, "");
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_POSTFIX, "");
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_PARENT_FEATURE, (parent == null) ? "" : parent);
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_DEFAULT_VALUE, "Selected");
            writer.WriteElementString(SPLConquerorGenerator.ATTRIBUTE_CONFIGURATION_OPTION_IS_OPTIONAL, optional.ToString());

            /* Create the list based entries of features. */
            this.GenerateFeatureList(writer, childFeatures, SPLConquerorGenerator.ELEMENT_CONFIGURATION_OPTION_CHILDREN, SPLConquerorGenerator.ELEMENT_CONFIGURATION_RELATED_OPTION);
            this.GenerateFeatureList(writer, implicatedFeatures, SPLConquerorGenerator.ELEMENT_CONFIGURATION_OPTION_IMPLICATIONS, SPLConquerorGenerator.ELEMENT_CONFIGURATION_RELATED_OPTION_E);
            this.GenerateFeatureList(writer, excludedFeatures, SPLConquerorGenerator.ELEMENT_CONFIGURATION_OPTION_EXCLUSIONS, SPLConquerorGenerator.ELEMENT_CONFIGURATION_RELATED_OPTION_E);

            /* End the option. */
            writer.WriteEndElement();
        }

        /// <summary>
        ///     Write a list of features.
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        /// <param name="subFeatures">The list of sub-features.</param>
        /// <param name="nodeType">The type of the node that wraps the list.</param>
        /// <param name="featureNodeType">The type of the node that represents a list item.</param>
        private void GenerateFeatureList(XmlWriter writer, IEnumerable<string> subFeatures, string nodeType, string featureNodeType)
        {
            writer.WriteStartElement(nodeType);
            if (subFeatures != null)
            {
                foreach (string feature in subFeatures)
                {
                    writer.WriteElementString(featureNodeType, feature);
                }
            }
            writer.WriteEndElement();
        }        

    }

}
