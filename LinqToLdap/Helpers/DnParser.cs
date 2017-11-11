/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;

namespace LinqToLdap.Helpers
{
    /// <summary>
    /// Static class for parsing distinguished name information
    /// </summary>
    public static class DnParser
    {
        /// <summary>
        /// Parses the first name of an entry without the RDN prefix (CN, OU, etc.) from <paramref name="distinguishedName"/> and returns that value.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name to parse.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="distinguishedName"/> is null, empty, white space, or not a valid distinguished name.</exception>
        /// <returns></returns>
        public static string ParseName(string distinguishedName)
        {
            string name;
            if (distinguishedName.IsNullOrEmpty())
            {
                throw new ArgumentException(string.Format("Invalid distinguished name '{0}'.", distinguishedName), "distinguishedName");
            }
            int firstEquals = distinguishedName.IndexOf('=');
            if (firstEquals <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }

            int secondEquals = distinguishedName.IndexOf('=', firstEquals + 1);

            if (secondEquals <= 0)
            {
                name =  distinguishedName.Substring(firstEquals + 1);
                return name;
            }

            string sub = distinguishedName.Substring(firstEquals, secondEquals);
            int lastComma = sub.LastIndexOf(',');
            if (lastComma <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }

            name = distinguishedName.Substring(firstEquals + 1, lastComma - 1);
            return name;
        }

        /// <summary>
        /// Parses the first RDN attribute type.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name.</param>
        /// <example>
        /// OU=Test,DC=local returns OU
        /// </example>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="distinguishedName"/> is null, empty, white space, or has an invalid format.</exception>
        public static string ParseRDN(string distinguishedName)
        {
            if (distinguishedName.IsNullOrEmpty())
            {
                throw new ArgumentException(string.Format("Invalid distinguished name '{0}'.", distinguishedName), "distinguishedName");
            }
            int startIndex = distinguishedName.IndexOf('=');
            if (startIndex <= 0)
            {
                throw new ArgumentException(
                    string.Format("Name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }

            return distinguishedName.Substring(0, startIndex);
        }

        internal static string GetEntryName(string distinguishedName)
        {
            if (distinguishedName.IsNullOrEmpty())
            {
                throw new ArgumentException(string.Format("Invalid distinguished name '{0}'.", distinguishedName), "distinguishedName");
            }
            int firstEquals = distinguishedName.IndexOf('=');
            if (firstEquals <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }

            int secondEquals = distinguishedName.IndexOf('=', firstEquals + 1);

            if (secondEquals <= 0)
            {
                return distinguishedName;
            }

            string sub = distinguishedName.Substring(firstEquals, secondEquals);
            int lastComma = sub.LastIndexOf(',');
            if (lastComma <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }
            return distinguishedName.Substring(0, firstEquals + lastComma);
        }

        internal static string GetEntryContainer(string distinguishedName)
        {
            if (distinguishedName.IsNullOrEmpty())
            {
                throw new ArgumentException(string.Format("Invalid distinguished name '{0}'.", distinguishedName), "distinguishedName");
            }
            int firstEquals = distinguishedName.IndexOf('=');
            if (firstEquals <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }

            int secondEquals = distinguishedName.IndexOf('=', firstEquals + 1);

            if (secondEquals <= 0)
            {
                return distinguishedName;
            }

            string sub = distinguishedName.Substring(firstEquals, secondEquals);
            int lastComma = sub.LastIndexOf(',');
            if (lastComma <= 0)
            {
                throw new ArgumentException(
                    string.Format("Common name could not be parsed from distinguished name '{0}'.", distinguishedName),
                    "distinguishedName");
            }
            return distinguishedName.Substring(firstEquals + lastComma + 1);
        }

        internal static string FormatName(string name, string currentDistinguishedName)
        {
            if (name.IndexOf("=", StringComparison.Ordinal) >= 0) return name;

            int index = currentDistinguishedName.IndexOf("=", StringComparison.Ordinal);

            return currentDistinguishedName.Substring(0, index + 1) + name;
        }
    }
}
