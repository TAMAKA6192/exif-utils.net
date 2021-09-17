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
using System.Drawing;

using ExifUtils.Exif.IO;
using ExifUtils.Exif.TagValues;

namespace ExifUtils.Exif {
    /// <summary>
    /// A strongly-typed adapter for common EXIF properties
    /// </summary>
    public class ImageMetaData {
        #region Constants

        private static readonly ExifTag[] StandardTags =
        {
            ExifTag.Aperture,
            ExifTag.Artist,
            ExifTag.ColorSpace,
            ExifTag.CompressedImageHeight,
            ExifTag.CompressedImageWidth,
            ExifTag.Copyright,
            ExifTag.DateTime,
            ExifTag.DateTimeDigitized,
            ExifTag.DateTimeOriginal,
            ExifTag.ExposureBias,
            ExifTag.ExposureMode,
            ExifTag.ExposureProgram,
            ExifTag.ExposureTime,
            ExifTag.Flash,
            ExifTag.FNumber,
            ExifTag.FocalLength,
            ExifTag.FocalLengthIn35mmFilm,
            ExifTag.GpsAltitude,
            ExifTag.GpsDestLatitude,
            ExifTag.GpsDestLatitudeRef,
            ExifTag.GpsDestLongitude,
            ExifTag.GpsDestLongitudeRef,
            ExifTag.GpsLatitude,
            ExifTag.GpsLatitudeRef,
            ExifTag.GpsLongitude,
            ExifTag.GpsLongitudeRef,
            ExifTag.ImageDescription,
            ExifTag.ImageTitle,
            ExifTag.ImageWidth,
            ExifTag.ISOSpeed,
            ExifTag.Make,
            ExifTag.MeteringMode,
            ExifTag.Model,
            ExifTag.MSAuthor,
            ExifTag.MSComments,
            ExifTag.MSKeywords,
            ExifTag.MSSubject,
            ExifTag.MSTitle,
            ExifTag.Orientation,
            ExifTag.ShutterSpeed,
            ExifTag.WhiteBalance
        };

        #endregion Constants

        #region Fields

        private decimal aperture;
        private string artist;
        private ExifTagColorSpace colorSpace;
        private string copyright;
        private DateTime dateTaken;
        private Rational<int> exposureBias;
        private ExifTagExposureMode exposureMode;
        private ExifTagExposureProgram exposureProgram;
        private ExifTagFlash flash;
        private decimal focalLength;
        private decimal gpsAltitude;
        private GpsCoordinate gpsLatitude;
        private GpsCoordinate gpsLongitude;
        private string imageDescription;
        private int imageHeight;
        private string imageTitle;
        private int imageWidth;
        private int isoSpeed;
        private ExifTagMeteringMode meteringMode;
        private string make;
        private string model;
        private string msAuthor;
        private string msComments;
        private string msKeywords;
        private string msSubject;
        private string msTitle;
        private ExifTagOrientation orientation;
        private Rational<uint> shutterSpeed;
        private ExifTagWhiteBalance whiteBalance;

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="image">image from which to populate properties</param>
        public ImageMetaData(Bitmap image)
            : this(ExifReader.GetExifData(image, ImageMetaData.StandardTags)) {
            if (image != null) {
                // override EXIF with actual values
                if (image.Height > 0) {
                    ImageHeight = image.Height;
                }

                if (image.Width > 0) {
                    ImageWidth = image.Width;
                }
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="properties">EXIF properties from which to populate</param>
        public ImageMetaData(ExifPropertyCollection properties) {
            if (properties == null) {
                throw new ArgumentNullException("properties");
            }

            // References:
            // http://www.media.mit.edu/pia/Research/deepview/exif.html
            // http://en.wikipedia.org/wiki/APEX_system
            // http://en.wikipedia.org/wiki/Exposure_value

            object rawValue;

            #region Aperture

            rawValue = properties[ExifTag.FNumber].Value;
            if (rawValue is IConvertible) {
                // f/x.x
                Aperture = Convert.ToDecimal(rawValue);
            } else {
                rawValue = properties[ExifTag.Aperture].Value;
                if (rawValue is IConvertible) {
                    // f/x.x
                    Aperture = (decimal)Math.Pow(2.0, Convert.ToDouble(rawValue) / 2.0);
                }
            }

            #endregion Aperture

            Artist = Convert.ToString(properties[ExifTag.Artist].Value);

            #region ColorSpace

            rawValue = properties[ExifTag.ColorSpace].Value;
            if (rawValue is Enum) {
                ColorSpace = (ExifTagColorSpace)rawValue;
            }

            #endregion ColorSpace

            Copyright = Convert.ToString(properties[ExifTag.Copyright].Value);

            #region DateTaken

            rawValue = properties[ExifTag.DateTimeOriginal].Value;
            if (rawValue is DateTime) {
                DateTaken = (DateTime)rawValue;
            } else {
                rawValue = properties[ExifTag.DateTimeDigitized].Value;
                if (rawValue is DateTime) {
                    DateTaken = (DateTime)rawValue;
                } else {
                    rawValue = properties[ExifTag.DateTime].Value;
                    if (rawValue is DateTime) {
                        DateTaken = (DateTime)rawValue;
                    }
                }
            }

            #endregion DateTaken

            #region ExposureBias

            rawValue = properties[ExifTag.ExposureBias].Value;
            if (rawValue is Rational<int>) {
                ExposureBias = (Rational<int>)rawValue;
            }

            #endregion ExposureBias

            #region ExposureMode

            rawValue = properties[ExifTag.ExposureMode].Value;
            if (rawValue is Enum) {
                ExposureMode = (ExifTagExposureMode)rawValue;
            }

            #endregion ExposureMode

            #region ExposureProgram

            rawValue = properties[ExifTag.ExposureProgram].Value;
            if (rawValue is Enum) {
                ExposureProgram = (ExifTagExposureProgram)rawValue;
            }

            #endregion ExposureProgram

            #region Flash

            rawValue = properties[ExifTag.Flash].Value;
            if (rawValue is Enum) {
                Flash = (ExifTagFlash)rawValue;
            }

            #endregion Flash

            #region FocalLength

            rawValue = properties[ExifTag.FocalLength].Value;
            if (rawValue is IConvertible) {
                FocalLength = Convert.ToDecimal(rawValue);
            } else {
                rawValue = properties[ExifTag.FocalLengthIn35mmFilm].Value;
                if (rawValue is IConvertible) {
                    FocalLength = Convert.ToDecimal(rawValue);
                }
            }

            #endregion FocalLength

            #region GpsAltitude

            rawValue = properties[ExifTag.GpsAltitude].Value;
            if (rawValue is IConvertible) {
                GpsAltitude = Convert.ToDecimal(rawValue);
            }

            #endregion GpsAltitude

            string gpsDir;

            #region GpsLatitude

            gpsDir = Convert.ToString(properties[ExifTag.GpsLatitudeRef].Value);
            rawValue = properties[ExifTag.GpsLatitude].Value;
            if (!(rawValue is Array)) {
                gpsDir = Convert.ToString(properties[ExifTag.GpsDestLatitudeRef].Value);
                rawValue = properties[ExifTag.GpsDestLatitude].Value;
            }
            if (rawValue is Array) {
                GpsLatitude = AsGps((Array)rawValue, gpsDir);
            }

            #endregion GpsLatitude

            #region GpsLongitude

            gpsDir = Convert.ToString(properties[ExifTag.GpsLongitudeRef].Value);
            rawValue = properties[ExifTag.GpsLongitude].Value;
            if (!(rawValue is Array)) {
                gpsDir = Convert.ToString(properties[ExifTag.GpsDestLongitudeRef].Value);
                rawValue = properties[ExifTag.GpsDestLongitude].Value;
            }
            if (rawValue is Array) {
                GpsLongitude = AsGps((Array)rawValue, gpsDir);
            }

            #endregion GpsLongitude

            ImageDescription = Convert.ToString(properties[ExifTag.ImageDescription].Value);

            #region ImageHeight

            rawValue = properties[ExifTag.ImageHeight].Value;
            if (rawValue is IConvertible) {
                ImageHeight = Convert.ToInt32(rawValue);
            } else {
                rawValue = properties[ExifTag.CompressedImageHeight].Value;
                if (rawValue is IConvertible) {
                    ImageHeight = Convert.ToInt32(rawValue);
                }
            }

            #endregion ImageHeight

            #region ImageWidth

            rawValue = properties[ExifTag.ImageWidth].Value;
            if (rawValue is IConvertible) {
                ImageWidth = Convert.ToInt32(rawValue);
            } else {
                rawValue = properties[ExifTag.CompressedImageWidth].Value;
                if (rawValue is IConvertible) {
                    ImageWidth = Convert.ToInt32(rawValue);
                }
            }

            #endregion ImageWidth

            ImageTitle = Convert.ToString(properties[ExifTag.ImageTitle].Value);

            #region ISOSpeed

            rawValue = properties[ExifTag.ISOSpeed].Value;
            if (rawValue is Array) {
                var array = (Array)rawValue;
                if (array.Length > 0) {
                    rawValue = array.GetValue(0);
                }
            }
            if (rawValue is IConvertible) {
                ISOSpeed = Convert.ToInt32(rawValue);
            }

            #endregion ISOSpeed

            Make = Convert.ToString(properties[ExifTag.Make].Value);
            Model = Convert.ToString(properties[ExifTag.Model].Value);

            #region MeteringMode

            rawValue = properties[ExifTag.MeteringMode].Value;
            if (rawValue is Enum) {
                MeteringMode = (ExifTagMeteringMode)rawValue;
            }

            #endregion MeteringMode

            MSAuthor = Convert.ToString(properties[ExifTag.MSAuthor].Value);
            MSComments = Convert.ToString(properties[ExifTag.MSComments].Value);
            MSKeywords = Convert.ToString(properties[ExifTag.MSKeywords].Value);
            MSSubject = Convert.ToString(properties[ExifTag.MSSubject].Value);
            MSTitle = Convert.ToString(properties[ExifTag.MSTitle].Value);

            #region Orientation

            rawValue = properties[ExifTag.Orientation].Value;
            if (rawValue is Enum) {
                Orientation = (ExifTagOrientation)rawValue;
            }

            #endregion Orientation

            #region ShutterSpeed

            rawValue = properties[ExifTag.ExposureTime].Value;
            if (rawValue is Rational<uint>) {
                ShutterSpeed = (Rational<uint>)rawValue;
            } else {
                rawValue = properties[ExifTag.ShutterSpeed].Value;
                if (rawValue is Rational<int>) {
                    ShutterSpeed = Rational<uint>.Approximate((decimal)Math.Pow(2.0, -Convert.ToDouble(rawValue)));
                }
            }

            #endregion ShutterSpeed

            #region WhiteBalance

            rawValue = properties[ExifTag.WhiteBalance].Value;
            if (rawValue is Enum) {
                WhiteBalance = (ExifTagWhiteBalance)rawValue;
            }

            #endregion WhiteBalance
        }

        private GpsCoordinate AsGps(Array array, string gpsDir) {
            if (array.Length != 3) {
                return null;
            }

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

            try {
                gps.Direction = gpsDir;
            } catch { }

            return gps;
        }

        #endregion Init

        #region Properties

        /// <summary>
        /// Gets and sets the aperture
        /// </summary>
        public decimal Aperture {
            get => aperture;
            set => aperture = value;
        }

        /// <summary>
        /// Gets and sets the artist
        /// </summary>
        public string Artist {
            get => artist;
            set => artist = value;
        }

        /// <summary>
        /// Gets and sets the color space
        /// </summary>
        public ExifTagColorSpace ColorSpace {
            get => colorSpace;
            set => colorSpace = value;
        }

        /// <summary>
        /// Gets and sets the copyright
        /// </summary>
        public string Copyright {
            get => copyright;
            set => copyright = value;
        }

        /// <summary>
        /// Gets and sets the date the photo was taken
        /// </summary>
        public DateTime DateTaken {
            get => dateTaken;
            set => dateTaken = value;
        }

        /// <summary>
        /// Gets and sets the exposure bias
        /// </summary>
        public Rational<int> ExposureBias {
            get => exposureBias;
            set => exposureBias = value;
        }

        /// <summary>
        /// Gets and sets the exposure mode
        /// </summary>
        public ExifTagExposureMode ExposureMode {
            get => exposureMode;
            set => exposureMode = value;
        }

        /// <summary>
        /// Gets and sets the exposure program
        /// </summary>
        public ExifTagExposureProgram ExposureProgram {
            get => exposureProgram;
            set => exposureProgram = value;
        }

        /// <summary>
        /// Gets and sets the flash
        /// </summary>
        public ExifTagFlash Flash {
            get => flash;
            set => flash = value;
        }

        /// <summary>
        /// Gets and sets the focal length
        /// </summary>
        public decimal FocalLength {
            get => focalLength;
            set => focalLength = value;
        }

        /// <summary>
        /// Gets and sets the GPS altitude
        /// </summary>
        public decimal GpsAltitude {
            get => gpsAltitude;
            set => gpsAltitude = value;
        }

        /// <summary>
        /// Gets and sets the GPS latitude
        /// </summary>
        public GpsCoordinate GpsLatitude {
            get => gpsLatitude;
            set => gpsLatitude = value;
        }

        /// <summary>
        /// Gets and sets the GPS longitude
        /// </summary>
        public GpsCoordinate GpsLongitude {
            get => gpsLongitude;
            set => gpsLongitude = value;
        }

        /// <summary>
        /// Gets and sets the image description
        /// </summary>
        public string ImageDescription {
            get => imageDescription;
            set => imageDescription = value;
        }

        /// <summary>
        /// Gets and sets the image height
        /// </summary>
        public int ImageHeight {
            get => imageHeight;
            set => imageHeight = value;
        }

        /// <summary>
        /// Gets and sets the image title
        /// </summary>
        public string ImageTitle {
            get => imageTitle;
            set => imageTitle = value;
        }

        /// <summary>
        /// Gets and sets the image width
        /// </summary>
        public int ImageWidth {
            get => imageWidth;
            set => imageWidth = value;
        }

        /// <summary>
        /// Gets and sets the ISO speed
        /// </summary>
        public int ISOSpeed {
            get => isoSpeed;
            set => isoSpeed = value;
        }

        /// <summary>
        /// Gets and sets the metering mode
        /// </summary>
        public ExifTagMeteringMode MeteringMode {
            get => meteringMode;
            set => meteringMode = value;
        }

        /// <summary>
        /// Gets and sets the camera make
        /// </summary>
        public string Make {
            get => make;
            set => make = value;
        }

        /// <summary>
        /// Gets and sets the camera model
        /// </summary>
        public string Model {
            get => model;
            set => model = value;
        }

        /// <summary>
        /// Gets and sets the author
        /// </summary>
        public string MSAuthor {
            get => msAuthor;
            set => msAuthor = value;
        }

        /// <summary>
        /// Gets and sets comments
        /// </summary>
        public string MSComments {
            get => msComments;
            set => msComments = value;
        }

        /// <summary>
        /// Gets and sets keywords
        /// </summary>
        public string MSKeywords {
            get => msKeywords;
            set => msKeywords = value;
        }

        /// <summary>
        /// Gets and sets the subject
        /// </summary>
        public string MSSubject {
            get => msSubject;
            set => msSubject = value;
        }

        /// <summary>
        /// Gets and sets the title
        /// </summary>
        public string MSTitle {
            get => msTitle;
            set => msTitle = value;
        }

        /// <summary>
        /// Gets and sets the orientation
        /// </summary>
        public ExifTagOrientation Orientation {
            get => orientation;
            set => orientation = value;
        }

        /// <summary>
        /// Gets and sets the shutter speed in seconds
        /// </summary>
        public Rational<uint> ShutterSpeed {
            get => shutterSpeed;
            set => shutterSpeed = value;
        }

        /// <summary>
        /// Gets and sets the white balance
        /// </summary>
        public ExifTagWhiteBalance WhiteBalance {
            get => whiteBalance;
            set => whiteBalance = value;
        }

        #endregion Properties
    }
}
