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
    interface IGenerator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetFile"></param>
        void Generate(string targetFile);

    }

}
