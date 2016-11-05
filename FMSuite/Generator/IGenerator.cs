using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSuite.Generator
{

    /// <summary>
    ///     Interface for generators that convert the meta feature model into an export model.
    /// </summary>
    interface IGenerator
    {

        /// <summary>
        ///     Generate the export file.
        /// </summary>
        /// <param name="targetFile">The path of the export file.</param>
        void Generate(string targetFile);

    }

}
