using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumbers.Internal
{
    /// <summary>
    /// Internal phonenumber matching API used to isolate the underlying implementation of the
    /// matcher and allow different implementations to be swapped in easily.
    /// </summary>
    public interface IMatcherApi
    {
        /// <summary>
        /// Returns whether the given national number (a string containing only decimal digits) matches
        /// the national number pattern defined in the given {@code PhoneNumberDesc} message.
        /// </summary>  
        bool MatchesNationalNumber(string nationalNumber, PhoneNumberDesc numberDesc, bool allowPrefixMatch);

        /// <summary>
        /// Returns whether the given national number (a string containing only decimal digits) matches
        /// the possible number pattern defined in the given {@code PhoneNumberDesc} message.
        /// </summary>
        bool MatchesPossibleNumber(string nationalNumber, PhoneNumberDesc numberDesc);

    }
}
