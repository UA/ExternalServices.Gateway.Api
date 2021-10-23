using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api.Infrastructure.Exceptions
{
    /// <summary>
    /// Defines the <see cref="DomainException" />.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        #region Fields
        public string Code { get; }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        public DomainException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        public DomainException(string message, string code = null) : base(message)
        {
            Code = code;
        }

        #endregion
    }
}
