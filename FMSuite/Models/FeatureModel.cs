using FMSuite.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Models
{

    /// <summary>
    ///     Internal representation of the extended feature model that's used as meta model for generating SPLConqueror and Feature Expression Models (as well as openfeatures.txt and confif.h).
    /// </summary>
    class FeatureModel
    {

        /// <summary>
        ///     Error message if a feature was already defined.
        /// </summary>
        const string ERROR_FEATURE_REDEFINED = "A cannot be defined twice.";

        /// <summary>
        ///     Error message if a feature was not defined before, but should have be.
        /// </summary>
        const string ERROR_FEATURE_UNKNOWN = "Usage of a unknown feature.";

        /// <summary>
        ///     Error message if a directive contains duplicate features, but should not.
        /// </summary>
        const string ERROR_DIRECTIVE_CONTAINS_DUPLICATE_FEATURES = "Directive contains duplicate features.";

        /// <summary>
        ///     The name of the study.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     A set of all features present in the model.
        /// </summary>
        private ISet<string> features = new HashSet<string>();

        /// <summary>
        ///     A set of all mandatory features.
        /// </summary>
        private ISet<string> mandatoryFeatures = new HashSet<string>();

        /// <summary>
        ///     A set of all fully optional features.
        /// </summary>
        private ISet<string> optionalFeatures = new HashSet<string>();

        /// <summary>
        ///     A set of boolean expressions containing restrictions for the model.
        /// </summary>
        private IList<string> constraints = new List<string>();

        /// <summary>
        ///     A set of alternative feature groups, labeled by a parent feature.
        /// </summary>
        private IDictionary<string, ISet<string>> alternatives = new Dictionary<string, ISet<string>>();

        /// <summary>
        ///     Initializes the feature model.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public FeatureModel(string name)
        {
            this.Name = name;
        }

        /// <summary>
        ///     Adds a mandatory feature to the model.
        /// </summary>
        /// <param name="feature">The feature to add.</param>
        public void AddMandatory(string feature)
        {
            this.TryToUpdateFeatures(feature);
            this.mandatoryFeatures.Add(feature);
        }

        /// <summary>
        ///     Adds a fully optional feature to the model.
        /// </summary>
        /// <param name="feature">The feature to add.</param>
        public void AddOptional(string feature)
        {
            this.TryToUpdateFeatures(feature);
            this.optionalFeatures.Add(feature);
        }

        /// <summary>
        ///     Declare a feature used in expressions.
        /// </summary>
        /// <param name="feature">The feature to add.</param>
        public void AddDeclared(string feature)
        {
            this.TryToUpdateFeatures(feature);
        }

        /// <summary>
        ///     Adds a group of alternative features to the model.
        /// </summary>
        /// <param name="parentFeature">The parent feature of the group.</param>
        /// <param name="alternativeChildren">The single features of the group.</param>
        public void AddAlternative(string parentFeature, IEnumerable<string> alternativeChildren)
        {
            this.ValidateFeatureExistance(parentFeature);
            this.ValidateForDuplicates(alternativeChildren);
            this.ValidateFeaturesExistance(alternativeChildren);
            this.alternatives.Add(new KeyValuePair<string, ISet<string>>(parentFeature, new HashSet<string>(alternativeChildren)));
        }

        /// <summary>
        ///     Add a boolean expression representing a constraint.
        /// </summary>
        /// <param name="constraint">The constraint to add.</param>
        public void AddConstraint(string constraint)
        {
            foreach (string term in Utility.ConvertToCNF(constraint))
            {
                this.constraints.Add(term);
            }
        }

        /// <summary>
        ///     Must be called as last. This will be trasformed to a constraint.
        /// </summary>
        /// <param name="excluded">The list of features representing an excluded configuration.</param>
        public void AddExcluded(IEnumerable<string> excluded)
        {
            this.ValidateForDuplicates(excluded);
            this.ValidateFeaturesExistance(excluded);

            /* Convert to an CNF compliant expression and store the expression in the list. */
            this.constraints.Add(Utility.ConvertExcludedConfigurationToCNFCompliantExpression(this.features, excluded));
        }

        /// <summary>
        ///     Gets the mandatory features of the model.
        /// </summary>
        /// <returns>The model's mandatory features.</returns>
        public IEnumerable<string> GetMandatory()
        {
            return this.mandatoryFeatures.ToList();
        }

        /// <summary>
        ///     Gets the optional features of the model.
        /// </summary>
        /// <returns>The model's optional features.</returns>
        public IEnumerable<string> GetOptional()
        {
            return this.optionalFeatures.ToList();
        }

        /// <summary>
        ///     Gets all features of the model.
        /// </summary>
        /// <returns>The model's complete set of features.</returns>
        public IEnumerable<string> GetFeatures()
        {
            return this.features.ToList();
        }

        /// <summary>
        ///     Gets all alternative feature groups of the model.
        /// </summary>
        /// <returns>The model's alternative feature groups.</returns>
        public IDictionary<string, ISet<string>> GetAlternatives()
        {
            return this.alternatives;
        }

        /// <summary>
        ///     Returns the boolean constraints representing a CNF where every list item is in DNF.
        /// </summary>
        /// <returns>A list of the boolean constraints</returns>
        public IEnumerable<string> GetConstraints()
        {
            return this.constraints;
        }

        /// <summary>
        ///     The feature has to be not seen before and will be added to the features list.
        /// </summary>
        /// <param name="feature">The feature to check.</param>
        private void TryToUpdateFeatures(string feature)
        {
            if (this.features.Contains(feature))
            {
                throw new InvalidDataException(FeatureModel.ERROR_FEATURE_REDEFINED);
            }
            this.features.Add(feature);
        }

        /// <summary>
        ///     Checks if a given set of features contains duplicates.
        /// </summary>
        /// <param name="features">The list of features to check.</param>
        /// <exception cref="InvalidDataException">Thrown if the list contains duplicates.</exception>
        private void ValidateForDuplicates(IEnumerable<string> features)
        {
            if (features.GroupBy(feature => feature).Where(grouped => grouped.Count() > 1).Select(grouped => grouped.Key).ToList().Count > 0)
            {
                throw new InvalidDataException(FeatureModel.ERROR_DIRECTIVE_CONTAINS_DUPLICATE_FEATURES);
            }
        }

        /// <summary>
        ///     Check if all featurs have been defined.
        /// </summary>
        /// <param name="features">The list of features to check.</param>
        /// <exception cref="InvalidDataException">Thrown if a feature is undefined.</exception>
        private void ValidateFeaturesExistance(IEnumerable<string> features)
        {
            foreach (string feature in features)
            {
                this.ValidateFeatureExistance(feature);
            }
        }

        /// <summary>
        ///     Check if the feature has been defined.
        /// </summary>
        /// <param name="feature">The feature to check.</param>
        /// <exception cref="InvalidDataException">Thrown if the feature is undefined.</exception>
        private void ValidateFeatureExistance(string feature)
        {
            if (!this.features.Contains(feature))
            {
                throw new InvalidDataException(FeatureModel.ERROR_FEATURE_UNKNOWN);
            }
        }
     
    }

}
