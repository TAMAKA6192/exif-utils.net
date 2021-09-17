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
using System.Collections.Generic;
using System.Reflection;

namespace ExifUtils {
    /// <summary>
    /// Represents a rational number
    /// </summary>
    [Serializable]
    public struct Rational<T> :
        IConvertible,
        IComparable,
        IComparable<T>
        where T : IConvertible {
        #region Delegate Types

        private delegate T ParseDelegate(string value);
        private delegate bool TryParseDelegate(string value, out T rational);

        #endregion Delegate Types

        #region Constants

        private const char Delim = '/';
        private static readonly char[] DelimSet = new char[] { Delim };

        #endregion Constants

        #region Fields

        public static readonly Rational<T> Empty = new Rational<T>();

        private static ParseDelegate Parser;
        private static TryParseDelegate TryParser;
        private static decimal maxValue;

        private readonly T numerator;
        private readonly T denominator;

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="numerator">The numerator of the rational number.</param>
        /// <param name="denominator">The denominator of the rational number.</param>
        /// <remarks>reduces by default</remarks>
        public Rational(T numerator, T denominator)
            : this(numerator, denominator, false) {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="numerator">The numerator of the rational number.</param>
        /// <param name="denominator">The denominator of the rational number.</param>
        /// <param name="reduce">determines if should reduce by greatest common divisor</param>
        public Rational(T numerator, T denominator, bool reduce) {
            this.numerator = numerator;
            this.denominator = denominator;

            if (reduce) {
                Rational<T>.Reduce(ref this.numerator, ref this.denominator);
            }
        }

        #endregion Init

        #region Properties

        /// <summary>
        /// Gets and sets the numerator of the rational number
        /// </summary>
        public T Numerator => numerator;

        /// <summary>
        /// Gets and sets the denominator of the rational number
        /// </summary>
        public T Denominator => denominator;

        /// <summary>
        /// Gets a value indicating if this is an empty instance
        /// </summary>
        public bool IsEmpty => Equals(Rational<T>.Empty);

        /// <summary>
        /// Gets the MaxValue
        /// </summary>
        private static decimal MaxValue {
            get {
                if (Rational<T>.maxValue == default(decimal)) {
                    var maxValue = typeof(T).GetField("MaxValue", BindingFlags.Static | BindingFlags.Public);
                    if (maxValue != null) {
                        try {
                            Rational<T>.maxValue = Convert.ToDecimal(maxValue.GetValue(null));
                        } catch (OverflowException) {
                            Rational<T>.maxValue = decimal.MaxValue;
                        }
                    } else {
                        Rational<T>.maxValue = int.MaxValue;
                    }
                }

                return Rational<T>.maxValue;
            }
        }

        #endregion Properties

        #region Parse Methods

        /// <summary>
        /// Approximate the decimal value accurate to a precision of 0.000001m
        /// </summary>
        /// <param name="value">decimal value to approximate</param>
        /// <returns>an approximation of the value as a rational number</returns>
        /// <remarks>
        /// http://stackoverflow.com/questions/95727
        /// </remarks>
        public static Rational<T> Approximate(decimal value) => Rational<T>.Approximate(value, 0.000001m);

        /// <summary>
        /// Approximate the decimal value accurate to a certain precision
        /// </summary>
        /// <param name="value">decimal value to approximate</param>
        /// <param name="epsilon">maximum precision to converge</param>
        /// <returns>an approximation of the value as a rational number</returns>
        /// <remarks>
        /// http://stackoverflow.com/questions/95727
        /// </remarks>
        public static Rational<T> Approximate(decimal value, decimal epsilon) {
            var numerator = decimal.Truncate(value);
            var denominator = decimal.One;
            var fraction = decimal.Divide(numerator, denominator);
            var maxValue = Rational<T>.MaxValue;

            while (Math.Abs(fraction - value) > epsilon && (denominator < maxValue) && (numerator < maxValue)) {
                if (fraction < value) {
                    numerator++;
                } else {
                    denominator++;

                    var temp = Math.Round(decimal.Multiply(value, denominator));
                    if (temp > maxValue) {
                        denominator--;
                        break;
                    }

                    numerator = temp;
                }

                fraction = decimal.Divide(numerator, denominator);
            }

            return new Rational<T>(
                (T)Convert.ChangeType(numerator, typeof(T)),
                (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Rational&lt;T&gt;"/> equivalent.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Rational<T> Parse(string value) {
            if (string.IsNullOrEmpty(value)) {
                return Rational<T>.Empty;
            }

            if (Rational<T>.Parser == null) {
                Rational<T>.Parser = Rational<T>.BuildParser();
            }

            var parts = value.Split(Rational<T>.DelimSet, 2, StringSplitOptions.RemoveEmptyEntries);
            var numerator = Rational<T>.Parser(parts[0]);
            T denominator;
            if (parts.Length > 1) {
                denominator = Rational<T>.Parser(parts[1]);
            } else {
                denominator = default(T);
            }

            return new Rational<T>(numerator, denominator);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Rational&lt;T&gt;"/> equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rational"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Rational<T> rational) {
            if (string.IsNullOrEmpty(value)) {
                rational = Rational<T>.Empty;
                return false;
            }

            if (Rational<T>.TryParser == null) {
                Rational<T>.TryParser = Rational<T>.BuildTryParser();
            }

            T denominator;
            var parts = value.Split(Rational<T>.DelimSet, 2, StringSplitOptions.RemoveEmptyEntries);
            if (!Rational<T>.TryParser(parts[0], out var numerator)) {
                rational = Rational<T>.Empty;
                return false;
            }
            if (parts.Length > 1) {
                if (!Rational<T>.TryParser(parts[1], out denominator)) {
                    rational = Rational<T>.Empty;
                    return false;
                }
            } else {
                denominator = default(T);
            }

            rational = new Rational<T>(numerator, denominator);
            return (parts.Length == 2);
        }

        private static Rational<T>.ParseDelegate BuildParser() {
            var parse = typeof(T).GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(string) },
                null);

            if (parse == null) {
                throw new InvalidOperationException("Underlying Rational type T must support Parse in order to parse Rational<T>.");
            }

            return new Rational<T>.ParseDelegate(
                delegate (string value) {
                    try {
                        return (T)parse.Invoke(null, new object[] { value });
                    } catch (TargetInvocationException ex) {
                        if (ex.InnerException != null) {
                            throw ex.InnerException;
                        }
                        throw;
                    }
                });
        }

        private static Rational<T>.TryParseDelegate BuildTryParser() {
            // http://stackoverflow.com/questions/1933369

            var tryParse = typeof(T).GetMethod(
                "TryParse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(string), typeof(T).MakeByRefType() },
                null);

            if (tryParse == null) {
                throw new InvalidOperationException("Underlying Rational type T must support TryParse in order to try-parse Rational<T>.");
            }

            return new Rational<T>.TryParseDelegate(
                delegate (string value, out T output) {
                    var args = new object[] { value, default(T) };
                    try {
                        var success = (bool)tryParse.Invoke(null, args);
                        output = (T)args[1];
                        return success;
                    } catch (TargetInvocationException ex) {
                        if (ex.InnerException != null) {
                            throw ex.InnerException;
                        }
                        throw;
                    }
                });
        }

        #endregion Parse Methods

        #region Math Methods

        /// <summary>
        /// Finds the greatest common divisor and reduces the fraction by this amount.
        /// </summary>
        /// <returns>the reduced rational</returns>
        public Rational<T> Reduce() {
            var numerator = this.numerator;
            var denominator = this.denominator;

            Rational<T>.Reduce(ref numerator, ref denominator);

            return new Rational<T>(numerator, denominator);
        }

        /// <summary>
        /// Finds the greatest common divisor and reduces the fraction by this amount.
        /// </summary>
        /// <returns>the reduced rational</returns>
        private static void Reduce(ref T numerator, ref T denominator) {
            var reduced = false;

            var n = Convert.ToDecimal(numerator);
            var d = Convert.ToDecimal(denominator);

            // greatest common divisor
            var gcd = Rational<T>.GCD(n, d);
            if (gcd != decimal.One && gcd != decimal.Zero) {
                reduced = true;
                n /= gcd;
                d /= gcd;
            }

            // cancel out signs
            if (d < decimal.Zero) {
                reduced = true;
                n = -n;
                d = -d;
            }

            if (reduced) {
                numerator = (T)Convert.ChangeType(n, typeof(T));
                denominator = (T)Convert.ChangeType(d, typeof(T));
            }
        }

        /// <summary>
        /// Lowest Common Denominator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static decimal LCD(decimal a, decimal b) {
            if (a == decimal.Zero && b == decimal.Zero) {
                return decimal.Zero;
            }

            return (a * b) / Rational<T>.GCD(a, b);
        }

        /// <summary>
        /// Greatest Common Devisor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static decimal GCD(decimal a, decimal b) {
            if (a < decimal.Zero) {
                a = -a;
            }
            if (b < decimal.Zero) {
                b = -b;
            }

            while (a != b) {
                if (a == decimal.Zero) {
                    return b;
                }
                if (b == decimal.Zero) {
                    return a;
                }

                if (a > b) {
                    a %= b;
                } else {
                    b %= a;
                }
            }
            return a;
        }

        #endregion Math Methods

        #region IConvertible Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider) => string.Concat(
                numerator.ToString(provider),
                Rational<T>.Delim,
                denominator.ToString(provider));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public decimal ToDecimal(IFormatProvider provider) {
            try {
                var denominator = this.denominator.ToDecimal(provider);
                if (denominator == decimal.Zero) {
                    return decimal.Zero;
                }
                return numerator.ToDecimal(provider) / denominator;
            } catch (InvalidCastException) {
                var denominator = this.denominator.ToInt64(provider);
                if (denominator == 0L) {
                    return 0L;
                }
                return ((IConvertible)numerator.ToInt64(provider)).ToDecimal(provider) /
                    ((IConvertible)denominator).ToDecimal(provider);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public double ToDouble(IFormatProvider provider) {
            var denominator = this.denominator.ToDouble(provider);
            if (denominator == 0.0) {
                return 0.0;
            }
            return numerator.ToDouble(provider) / denominator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public float ToSingle(IFormatProvider provider) {
            var denominator = this.denominator.ToSingle(provider);
            if (denominator == 0.0f) {
                return 0.0f;
            }
            return numerator.ToSingle(provider) / denominator;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToBoolean(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToByte(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToChar(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToInt64(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToSByte(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToUInt16(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToUInt32(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)ToDecimal(provider)).ToUInt64(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => new DateTime(((IConvertible)this).ToInt64(provider));

        TypeCode IConvertible.GetTypeCode() => numerator.GetTypeCode();

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
            if (conversionType == null) {
                throw new ArgumentNullException("conversionType");
            }

            var thisType = GetType();
            if (thisType == conversionType) {
                // no conversion needed
                return this;
            }

            if (!conversionType.IsGenericType ||
                typeof(Rational<>) != conversionType.GetGenericTypeDefinition()) {
                // fall back to basic conversion
                return Convert.ChangeType(this, conversionType, provider);
            }

            // auto-convert between Rational<T> types by converting Numerator/Denominator
            var genericArg = conversionType.GetGenericArguments()[0];
            object[] ctorArgs =
            {
                Convert.ChangeType(Numerator, genericArg, provider),
                Convert.ChangeType(Denominator, genericArg, provider)
            };

            var ctor = conversionType.GetConstructor(new Type[] { genericArg, genericArg });
            if (ctor == null) {
                throw new InvalidCastException("Unable to find constructor for Rational<" + genericArg.Name + ">.");
            }
            return ctor.Invoke(ctorArgs);
        }

        #endregion IConvertible Members

        #region IComparable Members

        /// <summary>
        /// Compares this instance to a specified System.Object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object that) {
            if (that is Rational<T>) {
                // differentiate between a real zero and a divide by zero
                // work around divide by zero value to get meaningful comparisons
                var other = (Rational<T>)that;
                if (Convert.ToDecimal(denominator) == decimal.Zero) {
                    if (Convert.ToDecimal(other.denominator) == decimal.Zero) {
                        return Convert.ToDecimal(numerator).CompareTo(Convert.ToDecimal(other.numerator));
                    } else if (Convert.ToDecimal(other.numerator) == decimal.Zero) {
                        return Convert.ToDecimal(denominator).CompareTo(Convert.ToDecimal(other.denominator));
                    }
                } else if (Convert.ToDecimal(other.denominator) == decimal.Zero) {
                    if (Convert.ToDecimal(numerator) == decimal.Zero) {
                        return Convert.ToDecimal(denominator).CompareTo(Convert.ToDecimal(other.denominator));
                    }
                }
            }

            return Convert.ToDecimal(this).CompareTo(Convert.ToDecimal(that));
        }

        #endregion IComparable Members

        #region IComparable<T> Members

        /// <summary>
        /// Compares this instance to another <typeparamref name="T"/> instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(T that) => decimal.Compare(Convert.ToDecimal(this), Convert.ToDecimal(that));

        #endregion IComparable<T> Members

        #region Operators

        /// <summary>
        /// Negation
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rational<T> operator -(Rational<T> r) {
            var numerator = (T)Convert.ChangeType(-Convert.ToDecimal(r.numerator), typeof(T));
            return new Rational<T>(numerator, r.denominator);
        }

        /// <summary>
        /// Addition
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rational<T> operator +(Rational<T> r1, Rational<T> r2) {
            var n1 = Convert.ToDecimal(r1.numerator);
            var d1 = Convert.ToDecimal(r1.denominator);
            var n2 = Convert.ToDecimal(r2.numerator);
            var d2 = Convert.ToDecimal(r2.denominator);

            var denominator = Rational<T>.LCD(d1, d2);
            if (denominator > d1) {
                n1 *= (denominator / d1);
            }
            if (denominator > d2) {
                n2 *= (denominator / d2);
            }

            var numerator = n1 + n2;

            return new Rational<T>((T)Convert.ChangeType(numerator, typeof(T)), (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Subtraction
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rational<T> operator -(Rational<T> r1, Rational<T> r2) => r1 + (-r2);

        /// <summary>
        /// Multiplication
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rational<T> operator *(Rational<T> r1, Rational<T> r2) {
            var numerator = Convert.ToDecimal(r1.numerator) * Convert.ToDecimal(r2.numerator);
            var denominator = Convert.ToDecimal(r1.denominator) * Convert.ToDecimal(r2.denominator);

            return new Rational<T>((T)Convert.ChangeType(numerator, typeof(T)), (T)Convert.ChangeType(denominator, typeof(T)));
        }

        /// <summary>
        /// Division
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rational<T> operator /(Rational<T> r1, Rational<T> r2) => r1 * new Rational<T>(r2.denominator, r2.numerator);

        /// <summary>
        /// Less than
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator <(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) < 0;

        /// <summary>
        /// Less than or equal to
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator <=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) <= 0;

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator >(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) > 0;

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator >=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) >= 0;

        /// <summary>
        /// Equal to
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator ==(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) == 0;

        /// <summary>
        /// Not equal to
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool operator !=(Rational<T> r1, Rational<T> r2) => r1.CompareTo(r2) != 0;

        #endregion Operators

        #region Object Overrides

        public override string ToString() => Convert.ToString(this);

        public override bool Equals(object obj) => (CompareTo(obj) == 0);

        public override int GetHashCode() {
            // adapted from Anonymous Type: { uint Numerator, uint Denominator }
            var num = 0x1fb8d67d;
            num = (-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(numerator);
            return ((-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(denominator));
        }

        #endregion Object Overrides
    }
}
