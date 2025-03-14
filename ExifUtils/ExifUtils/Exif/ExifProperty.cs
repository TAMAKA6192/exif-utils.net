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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

using ExifUtils.Exif.IO;
using ExifUtils.Exif.TagValues;
using ExifUtils.Exif.TypeConverters;

namespace ExifUtils.Exif {
    /// <summary>
    /// Represents a single EXIF property.
    /// </summary>
    /// <remarks>
    /// Should try to serialize as EXIF+RDF http://www.w3.org/2003/12/exif/
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(ExifConverter))]
    public class ExifProperty {
        #region Fields

        private int id = (int)ExifTag.Unknown;
        private ExifType type = ExifType.Unknown;
        private object value = null;

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor
        /// </summary>
        public ExifProperty() {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public ExifProperty(ExifTag tag, object value) {
            Tag = tag;
            this.value = value;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="property"></param>
        public ExifProperty(PropertyItem property) {
            id = property.Id;
            type = (ExifType)property.Type;
            value = ExifDecoder.FromPropertyItem(property);
        }

        #endregion Init

        #region Properties

        /// <summary>
        /// Gets and sets the Property ID according to the Exif specification for DCF images.
        /// </summary>
        [Category("Key")]
        [DisplayName("Exif ID")]
        [Description("The Property ID according to the Exif specification for DCF images.")]
        [XmlAttribute("ExifID"), DefaultValue((int)ExifTag.Unknown)]
        public int ID {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Gets and sets the property name according to the Exif specification for DCF images.
        /// </summary>
        /// <remarks>
        /// Note: If the ExifTag value specifies the ExifType then ExifProperty.Type is automatically set.
        /// If the ExifTag does not specify the ExifType then ExifProperty.Type is left unchanged.
        /// </remarks>
        [Category("Key")]
        [DisplayName("Exif Tag")]
        [Description("The property name according to the Exif specification for DCF images.")]
        [XmlAttribute("ExifTag"), DefaultValue(ExifTag.Unknown)]
        public ExifTag Tag {
            get {
                if (Enum.IsDefined(typeof(ExifTag), id)) {
                    return (ExifTag)id;
                } else {
                    return ExifTag.Unknown;
                }
            }
            set {
                if (value != ExifTag.Unknown) {
                    id = (int)value;
                    var exifType = ExifDataTypeAttribute.GetExifType(value);
                    if (exifType != ExifType.Unknown) {
                        Type = exifType;
                    }
                }
            }
        }

        /// <summary>
        /// Gets and sets the EXIF data type.
        /// </summary>
        [Category("Value")]
        [Browsable(false)]
        [XmlAttribute("ExifType"), DefaultValue(ExifType.Unknown)]
        public ExifType Type {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// Gets and sets the EXIF value.
        /// </summary>
        [XmlElement(typeof(byte))]
        [XmlElement(typeof(byte[]))]
        [XmlElement(typeof(ushort))]
        [XmlElement(typeof(ushort[]))]
        [XmlElement(typeof(uint))]
        [XmlElement(typeof(uint[]))]
        [XmlElement(typeof(int))]
        [XmlElement(typeof(int[]))]
        [XmlElement(typeof(string))]
        [XmlElement(typeof(string[]))]
        [XmlElement(typeof(Rational<int>))]
        [XmlElement(typeof(Rational<int>[]))]
        [XmlElement(typeof(Rational<uint>))]
        [XmlElement(typeof(Rational<uint>[]))]
        [XmlElement(typeof(DateTime))]
        [XmlElement(typeof(UnicodeEncoding))]
        [XmlElement(typeof(ExifTagColorSpace))]
        [XmlElement(typeof(ExifTagCleanFaxData))]
        [XmlElement(typeof(ExifTagCompression))]
        [XmlElement(typeof(ExifTagContrast))]
        [XmlElement(typeof(ExifTagCustomRendered))]
        [XmlElement(typeof(ExifTagExposureMode))]
        [XmlElement(typeof(ExifTagExposureProgram))]
        [XmlElement(typeof(ExifTagFileSource))]
        [XmlElement(typeof(ExifTagFillOrder))]
        [XmlElement(typeof(ExifTagFlash))]
        [XmlElement(typeof(ExifTagGainControl))]
        [XmlElement(typeof(ExifTagGpsAltitudeRef))]
        [XmlElement(typeof(ExifTagGpsDifferential))]
        [XmlElement(typeof(ExifTagInkSet))]
        [XmlElement(typeof(ExifTagJPEGProc))]
        [XmlElement(typeof(ExifTagLightSource))]
        [XmlElement(typeof(ExifTagMeteringMode))]
        [XmlElement(typeof(ExifTagOrientation))]
        [XmlElement(typeof(ExifTagPhotometricInterpretation))]
        [XmlElement(typeof(ExifTagPlanarConfiguration))]
        [XmlElement(typeof(ExifTagPredictor))]
        [XmlElement(typeof(ExifTagResolutionUnit))]
        [XmlElement(typeof(ExifTagSampleFormat))]
        [XmlElement(typeof(ExifTagSaturation))]
        [XmlElement(typeof(ExifTagSceneCaptureType))]
        [XmlElement(typeof(ExifTagSceneType))]
        [XmlElement(typeof(ExifTagSensingMethod))]
        [XmlElement(typeof(ExifTagSharpness))]
        [XmlElement(typeof(ExifTagSubjectDistanceRange))]
        [XmlElement(typeof(ExifTagThreshholding))]
        [XmlElement(typeof(ExifTagWhiteBalance))]
        [XmlElement(typeof(ExifTagYCbCrPositioning))]
        [Category("Value")]
        public object Value {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// Gets the formatted text representation of the value.
        /// </summary>
        [DisplayName("Display Value")]
        [Category("Value")]
        public string DisplayValue => FormatValue();

        /// <summary>
        /// Gets the formatted text representation of the label.
        /// </summary>
        [DisplayName("Display Name")]
        [Category("Value")]
        public string DisplayName {
            get {
                if (Tag != ExifTag.Unknown) {
                    var label = Utility.GetDescription(Tag);
                    if (string.IsNullOrEmpty(label)) {
                        label = Tag.ToString("g");
                    }
                    return label;
                } else {
                    return string.Format("Exif_0x{0:x4}", ID);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// References:
        /// http://www.media.mit.edu/pia/Research/deepview/exif.html
        /// http://en.wikipedia.org/wiki/APEX_system
        /// http://en.wikipedia.org/wiki/Exposure_value
        /// </remarks>
        protected string FormatValue() {
            var rawValue = Value;
            switch (Tag) {
            case ExifTag.ISOSpeed: {
                if (rawValue is Array) {
                    var array = (Array)rawValue;
                    if (array.Length < 1 || !(array.GetValue(0) is IConvertible)) {
                        goto default;
                    }
                    rawValue = array.GetValue(0);
                }

                if (!(rawValue is IConvertible)) {
                    goto default;
                }

                return string.Format("ISO-{0:###0}", Convert.ToDecimal(rawValue));
            }
            case ExifTag.Aperture:
            case ExifTag.MaxAperture: {
                // The actual aperture value of lens when the image was taken.
                // To convert this value to ordinary F-number(F-stop),
                // calculate this value's power of root 2 (=1.4142).
                // For example, if value is '5', F-number is 1.4142^5 = F5.6.
                var fStop = Math.Pow(2.0, Convert.ToDouble(rawValue) / 2.0);
                return string.Format("f/{0:#0.0}", fStop);
            }
            case ExifTag.FNumber: {
                // The actual F-number (F-stop) of lens when the image was taken.
                return string.Format("f/{0:#0.0}", Convert.ToDecimal(rawValue));
            }
            case ExifTag.FocalLength:
            case ExifTag.FocalLengthIn35mmFilm: {
                return string.Format("{0:#0.#} mm", Convert.ToDecimal(rawValue));
            }
            case ExifTag.ShutterSpeed: {
                if (!(rawValue is Rational<int>)) {
                    goto default;
                }

                // To convert this value to ordinary 'Shutter Speed';
                // calculate this value's power of 2, then reciprocal.
                // For example, if value is '4', shutter speed is 1/(2^4)=1/16 second.
                var shutter = (Rational<int>)rawValue;

                if (shutter.Numerator > 0) {
                    var speed = Math.Pow(2.0, Convert.ToDouble(shutter));
                    return string.Format("1/{0:####0} sec", speed);
                } else {
                    var speed = Math.Pow(2.0, -Convert.ToDouble(shutter));
                    return string.Format("{0:####0.##} sec", speed);
                }
            }
            case ExifTag.ExposureTime: {
                if (!(rawValue is Rational<uint>)) {
                    goto default;
                }

                // Exposure time (reciprocal of shutter speed). Unit is second.
                var exposure = (Rational<uint>)rawValue;

                if (exposure.Numerator < exposure.Denominator) {
                    exposure.Reduce();
                    return string.Format("{0} sec", exposure);
                } else {
                    return string.Format("{0:####0.##} sec", Convert.ToDecimal(rawValue));
                }
            }
            case ExifTag.XResolution:
            case ExifTag.YResolution:
            case ExifTag.ThumbnailXResolution:
            case ExifTag.ThumbnailYResolution:
            case ExifTag.FocalPlaneXResolution:
            case ExifTag.FocalPlaneYResolution: {
                return string.Format("{0:###0} dpi", Convert.ToDecimal(rawValue));
            }
            case ExifTag.ImageHeight:
            case ExifTag.ImageWidth:
            case ExifTag.CompressedImageHeight:
            case ExifTag.CompressedImageWidth:
            case ExifTag.ThumbnailHeight:
            case ExifTag.ThumbnailWidth: {
                return string.Format("{0:###0} pixels", Convert.ToDecimal(rawValue));
            }
            case ExifTag.SubjectDistance: {
                return string.Format("{0:###0.#} m", Convert.ToDecimal(rawValue));
            }
            case ExifTag.ExposureBias:
            case ExifTag.Brightness: {
                string val;
                if (rawValue is Rational<int>) {
                    val = ((Rational<int>)rawValue).Numerator != 0 ? rawValue.ToString() : "0";
                } else {
                    val = Convert.ToDecimal(rawValue).ToString();
                }

                return string.Format("{0} EV", val);
            }
            case ExifTag.CompressedBitsPerPixel: {
                return string.Format("{0:###0.0} bits", Convert.ToDecimal(rawValue));
            }
            case ExifTag.DigitalZoomRatio: {
                return Convert.ToString(rawValue).Replace('/', ':');
            }
            case ExifTag.GpsLatitude:
            case ExifTag.GpsLongitude:
            case ExifTag.GpsDestLatitude:
            case ExifTag.GpsDestLongitude: {
                if (!(rawValue is Array)) {
                    goto default;
                }

                var array = (Array)rawValue;
                if (array.Length < 1) {
                    return string.Empty;
                } else if (array.Length != 3) {
                    goto default;
                }

                // attempt to use the GPSCoordinate XMP formatting guidelines
                var gps = new GpsCoordinate();

                if (array.GetValue(0) is Rational<uint>) {
                    gps.Degrees = (Rational<uint>)array.GetValue(0);
                } else {
                    gps.SetDegrees(Convert.ToDecimal(array.GetValue(0)));
                }
                if (array.GetValue(1) is Rational<uint>) {
                    gps.Minutes = (Rational<uint>)array.GetValue(1);
                } else {
                    gps.SetMinutes(Convert.ToDecimal(array.GetValue(1)));
                }
                if (array.GetValue(2) is Rational<uint>) {
                    gps.Seconds = (Rational<uint>)array.GetValue(2);
                } else {
                    gps.SetSeconds(Convert.ToDecimal(array.GetValue(2)));
                }

                return gps.ToString();
            }
            case ExifTag.GpsTimeStamp: {
                var array = (Array)rawValue;
                if (array.Length < 1) {
                    return string.Empty;
                }

                var time = new string[array.Length];
                for (var i = 0; i < array.Length; i++) {
                    time[i] = Convert.ToDecimal(array.GetValue(i)).ToString();
                }
                return string.Join(":", time);
            }
            default: {
                if (rawValue is Enum) {
                    var description = Utility.GetDescription((Enum)rawValue);
                    if (!string.IsNullOrEmpty(description)) {
                        return description;
                    }
                } else if (rawValue is Array) {
                    var array = (Array)rawValue;
                    if (array.Length < 1) {
                        return string.Empty;
                    }

                    var type = array.GetValue(0).GetType();
                    if (!type.IsPrimitive || type == typeof(char) || type == typeof(float) || type == typeof(double)) {
                        return Convert.ToString(rawValue);
                    }

                    //const int ElemsPerRow = 40;
                    var charSize = 2 * System.Runtime.InteropServices.Marshal.SizeOf(type);
                    var format = "{0:X" + (charSize) + "}";
                    var builder = new StringBuilder(((charSize + 1) * array.Length)/*+(2*array.Length/ElemsPerRow)*/);
                    for (var i = 0; i < array.Length; i++) {
                        if (i > 0) {
                            //if ((i+1)%ElemsPerRow == 0)
                            //{
                            //    builder.AppendLine();
                            //}
                            //else
                            {
                                builder.Append(" ");
                            }
                        }

                        builder.AppendFormat(format, array.GetValue(i));
                    }
                    return builder.ToString();
                }

                return Convert.ToString(rawValue);
            }
            }
        }

        #endregion Methods

        #region Object Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => string.Format("{0}: {1}", DisplayName, DisplayValue);

        #endregion Object Overrides
    }
}
