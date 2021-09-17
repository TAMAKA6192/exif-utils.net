#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2005-2010 Stephen M. McKamey

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
using System.Text;

namespace ExifUtils {
    public class GpsCoordinate {
        #region Constants

        public const string North = "N";
        public const string East = "E";
        public const string West = "W";
        public const string South = "S";

        private static readonly char[] Directions = new char[] { 'N', 'E', 'W', 'S', 'n', 'e', 'w', 's' };

        private const string DecimalFormat = "0.0######";
        private const decimal SecPerMin = 60m;
        private const decimal MinPerDeg = 60m;

        #endregion Constants

        #region Fields

        private Rational<uint> degrees;
        private Rational<uint> minutes;
        private Rational<uint> seconds;
        private string direction;

        #endregion Fields

        #region Properties

        public Rational<uint> Degrees {
            get => degrees;
            set => degrees = value;
        }

        public Rational<uint> Minutes {
            get => minutes;
            set => minutes = value;
        }

        public Rational<uint> Seconds {
            get => seconds;
            set => seconds = value;
        }

        public string Direction {
            get => direction;
            set {
                if (string.IsNullOrEmpty(value)) {
                    direction = null;
                    return;
                }

                if (value.IndexOfAny(GpsCoordinate.Directions) != 0) {
                    throw new ArgumentException("Invalid GPS direction, must be one of 'N', 'E', 'W', 'S'.");
                }

                direction = char.ToUpperInvariant(value[0]).ToString();
            }
        }

        public decimal Value {
            get {
                var val = Convert.ToDecimal(Degrees) + decimal.Divide(Convert.ToDecimal(Minutes) + decimal.Divide(Convert.ToDecimal(Seconds), SecPerMin), MinPerDeg);
                var negative =
                    StringComparer.OrdinalIgnoreCase.Equals(Direction, GpsCoordinate.West) ||
                    StringComparer.OrdinalIgnoreCase.Equals(Direction, GpsCoordinate.South);

                return (negative == val < decimal.Zero) ? val : -val;
            }
        }

        #endregion Properties

        #region Methods

        public void SetDegrees(decimal deg) => Degrees = Rational<uint>.Approximate(deg);

        public void SetMinutes(decimal min) => Minutes = Rational<uint>.Approximate(min);

        public void SetSeconds(decimal sec) => Seconds = Rational<uint>.Approximate(sec);

        public static GpsCoordinate FromDecimal(decimal val) {
            var deg = decimal.Truncate(val);
            var min = decimal.Remainder(val, decimal.One) * MinPerDeg;
            var sec = decimal.Remainder(min, decimal.One) * SecPerMin;
            min = decimal.Truncate(min);

            return GpsCoordinate.FromDecimal(deg, min, sec);
        }

        public static GpsCoordinate FromDecimal(decimal deg, decimal min, decimal sec) {
            var gps = new GpsCoordinate();

            gps.SetDegrees(deg);
            gps.SetMinutes(min);
            gps.SetSeconds(sec);

            return gps;
        }

        public static GpsCoordinate FromURational(Rational<uint> deg, Rational<uint> min, Rational<uint> sec) {
            var gps = new GpsCoordinate {
                Degrees = deg,
                Minutes = min,
                Seconds = sec
            };

            return gps;
        }

        public Rational<uint>[] ToURational() => new Rational<uint>[] {
                Degrees, Minutes, Seconds
            };

        public static GpsCoordinate Parse(string value) {
            if (!GpsCoordinate.TryParse(value, out var gps)) {
                throw new ArgumentException("Invalid GpsCoordinate");
            }

            return gps;
        }

        public static bool TryParse(string value, out GpsCoordinate gps) {
            if (string.IsNullOrEmpty(value)) {
                gps = null;
                return false;
            }

            int index = 0,
                last = 0,
                length = value.Length;
            char ch;

            #region parse degrees

            for (; index < length; index++) {
                ch = value[index];

                if (ch != '.' &&
                    ch != '-' &&
                    ch != '+' &&
                    (ch < '0' || ch > '9')) {
                    break;
                }
            }

            if (!decimal.TryParse(value.Substring(last, index - last), out var deg)) {
                gps = null;
                return false;
            }

            for (; index < length; index++) {
                ch = value[index];
                if (ch != ',' &&
                    ch != '\u00B0' &&
                    ch != ' ') {
                    break;
                }
            }
            last = index;

            #endregion parse degrees

            #region parse minutes

            if (last + 1 >= length) {
                gps = GpsCoordinate.FromDecimal(deg);
                if (last < length &&
                    value.IndexOfAny(GpsCoordinate.Directions, last, 1) == last) {
                    gps.Direction = value.Substring(last);
                }
                return true;
            }

            for (; index < length; index++) {
                ch = value[index];

                if (ch != '.' &&
                    ch != '-' &&
                    ch != '+' &&
                    (ch < '0' || ch > '9')) {
                    break;
                }
            }

            if (!decimal.TryParse(value.Substring(last, index - last), out var min)) {
                gps = null;
                return false;
            }

            for (; index < length; index++) {
                ch = value[index];
                if (ch != ',' &&
                    ch != '\'' &&
                    ch != ' ') {
                    break;
                }
            }
            last = index;

            #endregion parse minutes

            #region parse seconds

            if (last + 1 >= length) {
                gps = GpsCoordinate.FromDecimal(deg, min, 0m);
                if (last < length &&
                    value.IndexOfAny(GpsCoordinate.Directions, last, 1) == last) {
                    gps.Direction = value.Substring(last);
                }
                return true;
            }

            for (; index < length; index++) {
                ch = value[index];

                if (ch != '.' &&
                    ch != '-' &&
                    ch != '+' &&
                    (ch < '0' || ch > '9')) {
                    break;
                }
            }

            if (!decimal.TryParse(value.Substring(last, index - last), out var sec)) {
                gps = null;
                return false;
            }

            for (; index < length; index++) {
                ch = value[index];
                if (ch != ',' &&
                    ch != '"' &&
                    ch != ' ') {
                    break;
                }
            }
            last = index;

            #endregion parse seconds

            if (last + 1 >= length) {
                gps = GpsCoordinate.FromDecimal(deg, min, sec);
                if (last < length &&
                    value.IndexOfAny(GpsCoordinate.Directions, last, 1) == last) {
                    gps.Direction = value.Substring(last);
                }
                return true;
            }

            gps = null;
            return false;
        }

        #endregion Methods

        #region Object Overrides

        /// <summary>
        /// Formats the GPS coordinate as an XMP GPSCoordinate string
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToString(null);

        /// <summary>
        /// Formats the GPS coordinate as an XMP GPSCoordinate string
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Accepts "X", "x" for XMP style formatting, or "D", "d" for degree style formatting, or "N", "n" for numeric
        /// </remarks>
        public string ToString(string format) {
            if (string.IsNullOrEmpty(format)) {
                format = "X";
            } else {
                switch (format) {
                case "X":
                case "x":
                case "D":
                case "d": {
                    format = format.ToUpperInvariant();
                    break;
                }
                case "N":
                case "n": {
                    return Value.ToString(GpsCoordinate.DecimalFormat);
                }
                default: {
                    throw new ArgumentException("format");
                }
                }
            }

            var gps = new StringBuilder();

            if (Degrees.IsEmpty || Minutes.IsEmpty || Seconds.IsEmpty || Degrees.Denominator != 1) {
                // use full decimal formatting
                if (string.IsNullOrEmpty(Direction)) {
                    gps.Append(Value.ToString(GpsCoordinate.DecimalFormat));
                } else {
                    gps.Append(Math.Abs(Value).ToString(GpsCoordinate.DecimalFormat));
                }
                if (format == "D") {
                    gps.Append("\u00B0");
                }
                if (!string.IsNullOrEmpty(Direction)) {
                    if (format == "D") {
                        gps.Append(' ');
                    }
                    gps.Append(Direction);
                }
                return gps.ToString();
            }

            gps.Append(Degrees.Numerator);
            switch (format) {
            case "D": {
                gps.Append("\u00B0 ");
                break;
            }
            case "X": {
                gps.Append(',');
                break;
            }
            }

            // DD,MM.mmk
            if (Minutes.Denominator != 1 || Seconds.Denominator != 1) {
                var MMmm = Convert.ToDecimal(Minutes) + decimal.Divide(Convert.ToDecimal(Seconds), SecPerMin);
                gps.Append(MMmm.ToString(GpsCoordinate.DecimalFormat));
                if (format == "D") {
                    gps.Append('\'');
                }
                if (!string.IsNullOrEmpty(Direction)) {
                    if (format == "D") {
                        gps.Append(' ');
                    }
                    gps.Append(Direction);
                }
                return gps.ToString();
            }

            // DD,MM,SSk
            gps.Append(Minutes.Numerator);
            switch (format) {
            case "D": {
                gps.Append("' ");
                break;
            }
            case "X": {
                gps.Append(',');
                break;
            }
            }

            gps.Append(Seconds.Numerator);
            if (format == "D") {
                gps.Append('"');
            }

            if (!string.IsNullOrEmpty(Direction)) {
                if (format == "D") {
                    gps.Append(' ');
                }
                gps.Append(Direction);
            }

            return gps.ToString();
        }

        #endregion Object Overrides
    }
}
