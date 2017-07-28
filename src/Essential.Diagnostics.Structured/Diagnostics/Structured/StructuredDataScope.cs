using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Encompases a logical operation using the diagnostics correlation manager,
    /// recording properties as structured data, to augment structured data logging.
    /// </summary>
    class StructuredDataScope : IDisposable
    {
        /// <summary>
        /// Constructor. 
        /// Encompases a logical operation with the specified structured data properties.
        /// </summary>
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public StructuredDataScope(IStructuredData structuredData)
        {
            Trace.CorrelationManager.StartLogicalOperation(structuredData);
        }

        /// <summary>
        /// Dispose.
        /// Ends the logical operation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// Ends the logical operation.
        /// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Trace.CorrelationManager.StopLogicalOperation();
            }
        }

    }
}
