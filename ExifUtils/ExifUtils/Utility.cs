namespace ExifUtils {
    #region License
    /*---------------------------------------------------------------------------------*\

        Distributed under the terms of an MIT-style license:

        The MIT License

        Copyright (c) 2005-2009 Stephen M. McKamey

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.

    \*---------------------------------------------------------------------------------*/
    #endregion License

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Internal Utility Class
    /// </summary>
    internal static class Utility {
        #region Constants

        private const string FlagsDelim = ", ";

        #endregion Constants

        #region Enumeration Methods

        /// <summary>
        /// Checks if an enum is able to be combined as bit flags.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsFlagsEnum(Type type) {
            if (!type.IsEnum) {
                return false;
            }

            var flags = GetAttribute(type, typeof(FlagsAttribute), true);
            return flags != null;
        }

        /// <summary>
        /// Splits a bitwise-OR'd set of enums into a list.
        /// </summary>
        /// <param name="enumType">the enum type</param>
        /// <param name="value">the combined value</param>
        /// <returns>list of flag enums</returns>
        public static Enum[] GetFlagList(Type enumType, object value) {
            var longVal = Convert.ToUInt64(value);
            _ = Enum.GetNames(enumType);
            var enumValues = Enum.GetValues(enumType);

            var enums = new List<Enum>(enumValues.Length);

            // check for empty
            if (longVal == 0L) {
                // Return the value of empty, or zero if none exists
                if (Convert.ToUInt64(enumValues.GetValue(0)) == 0L) {
                    enums.Add(enumValues.GetValue(0) as Enum);
                } else {
                    enums.Add(null);
                }
                return enums.ToArray();
            }

            for (var i = enumValues.Length - 1; i >= 0; i--) {
                var enumValue = Convert.ToUInt64(enumValues.GetValue(i));

                if ((i == 0) && (enumValue == 0L)) {
                    continue;
                }

                // matches a value in enumeration
                if ((longVal & enumValue) == enumValue) {
                    // remove from val
                    longVal -= enumValue;

                    // add enum to list
                    enums.Add(enumValues.GetValue(i) as Enum);
                }
            }

            if (longVal != 0x0L) {
                enums.Add(Enum.ToObject(enumType, longVal) as Enum);
            }

            return enums.ToArray();
        }

        #endregion Enumeration Methods

        #region Attribute Methods

        /// <summary>
        /// Gets an attribute of an object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetAttribute(object value, Type attributeType, bool inherit) {
            if (value == null || attributeType == null) {
                return null;
            }

            object[] array;

            switch (value) {
            case Type _:
                array = (value as Type).GetCustomAttributes(attributeType, inherit);
                break;
            case MemberInfo _:
                array = (value as MemberInfo).GetCustomAttributes(attributeType, inherit);
                break;
            case Enum _:
                array = value.GetType().GetField(value.ToString()).GetCustomAttributes(attributeType, inherit);
                break;
            default:
                throw new NotSupportedException("object doesn't support attributes.");
            }

            return (array != null && array.Length > 0) ? array[0] as Attribute : null;
        }

        /// <summary>
        /// Gets the value of the DescriptionAttribute for the object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(object value) {
            if (value == null) {
                return null;
            }

            var attribute = GetAttribute(value, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attribute?.Description;
        }

        /// <summary>
        /// Gets the value of the DescriptionAttribute for the enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(Enum value) {
            if (!IsFlagsEnum(value.GetType())) {
                return GetDescription((object)value);
            }

            var enumValues = GetFlagList(value.GetType(), value);
            var builder = new StringBuilder();

            for (var i = 0; i < enumValues.Length; i++) {
                _ = builder.Append(GetDescription(enumValues[i] as object));

                if (i != enumValues.Length - 1) {
                    builder.Append(FlagsDelim);
                }
            }

            return builder.ToString();
        }

        #endregion Attribute Methods
    }
}
