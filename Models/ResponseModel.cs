using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api.Models
{
    /// <summary>
    /// Defines the <see cref="ResponseModel" />.
    /// </summary>
    [Serializable]
    public class ResponseModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseModel"/> class.
        /// </summary>
        public ResponseModel()
        {
            RequestId = Guid.NewGuid().ToString();
            Version = "1.0";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ErrorMessage.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets the RequestId.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// Gets or sets the Result.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the StatusCode.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        public string Version { get; set; }

        #endregion
    }
}
