using System;
using System . Collections . Generic;
using System . Drawing;
using System . Drawing . Imaging;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using static System . Math;

namespace Dwscdv3
{
    /** Format:
     *  Type        0x0
     *  Reserved    0x1 ~ 0x3
     *  Length      0x4 ~ 0x7
     *  Data        0x8 ~
     */
    public class HslColor
    {
        private double a;
        public double A
        {
            get { return a; }
            set
            {
                if ( value < 0 )
                {
                    a = 0;
                }
                else if ( value > 1 )
                {
                    a = 1;
                }
                else
                {
                    a = value;
                }
            }
        }
        private double h;
        public double H
        {
            get { return h; }
            set
            {
                if ( value < 0 )
                {
                    h = value + 1;
                }
                else if ( value >= 1 )
                {
                    h = value - 1;
                }
                else
                {
                    h = value;
                }
            }
        }
        private double s;
        public double S
        {
            get { return s; }
            set
            {
                if ( value < 0 )
                {
                    s = 0;
                }
                else if ( value > 1 )
                {
                    s = 1;
                }
                else
                {
                    s = value;
                }
            }
        }
        private double l;
        public double L
        {
            get { return l; }
            set
            {
                if ( value < 0 )
                {
                    l = 0;
                }
                else if ( value > 1 )
                {
                    l = 1;
                }
                else
                {
                    l = value;
                }
            }
        }

        public Color ToArgb ()
        {
            return ColorUtils . FromAhsl ( this );
        }
    }
    public static class ColorUtils
    {
        public static Color FromAhsl ( HslColor hsl )
        {
            return FromAhsl ( hsl . A , hsl . H , hsl . S , hsl . L );
        }
        public static Color FromAhsl ( double a , double h , double s , double l )
        {
            if ( a < 0 || a > 1 || h < 0 || h >= 1 || s < 0 || s > 1 || l < 0 || l > 1 )
            {
                throw new ArgumentOutOfRangeException ();
            }
            double r = 0, g = 0, b = 0;
            var c = ( 1 - Abs ( 2 * l - 1 ) ) * s;
            var h0 = h * 6;
            var section = (int) h0;
            var x = c * ( 1 - Abs ( h0 % 2 - 1 ) );
            switch ( section )
            {
            case 0:
                r = c;
                g = x;
                break;
            case 1:
                r = x;
                g = c;
                break;
            case 2:
                g = c;
                b = x;
                break;
            case 3:
                g = x;
                b = c;
                break;
            case 4:
                b = c;
                r = x;
                break;
            case 5:
                b = x;
                r = c;
                break;
            }
            var m = l - c * 0.5;
            r += m;
            g += m;
            b += m;
            return Color . FromArgb (
                a >= 1 ? 255 : (int) Floor ( a * 256 ) ,
                r >= 1 ? 255 : (int) Floor ( r * 256 ) ,
                g >= 1 ? 255 : (int) Floor ( g * 256 ) ,
                b >= 1 ? 255 : (int) Floor ( b * 256 )
            );
        }
        public static HslColor ToAhsl ( Color color )
        {
            return ToAhsl ( color . A , color . R , color . G , color . B );
        }
        public static HslColor ToAhsl ( byte a , byte r , byte g , byte b )
        {
            return ToAhsl ( a / 255.0 , r / 255.0 , g / 255.0 , b / 255.0 );
        }
        public static HslColor ToAhsl ( double a , double r , double g , double b )
        {
            double h = 0, s = 0, l = 0;
            var cmax = Max ( Max ( r , g ) , b );
            var cmin = Min ( Min ( r , g ) , b );
            var delta = cmax - cmin;
            if ( delta != 0 )
            {
                if ( cmax == r )
                {
                    h = ( 60 / 360.0 ) * ( ( g - b ) / delta % 6 );
                    if ( h < 0 )
                    {
                        h += 1;
                    }
                }
                else if ( cmax == g )
                {
                    h = ( 60 / 360.0 ) * ( ( b - r ) / delta + 2 );
                }
                else if ( cmax == b )
                {
                    h = ( 60 / 360.0 ) * ( ( r - g ) / delta + 4 );
                }
            }
            l = ( cmax + cmin ) / 2;
            if ( delta != 0 )
            {
                s = delta / ( 1 - Abs ( 2 * l - 1 ) );
            }
            return new HslColor
            {
                A = a ,
                H = h ,
                S = s ,
                L = l
            };
        }
    }
    public static class BitmapCode
    {
        const int LengthPos = 4;
        const int HeaderLength = 8;

        public static byte [] FromBitmapToBytes ( byte [] b )
        {
            var ms = new MemoryStream ( b );
            return FromBitmapToBytes ( ms );
        }
        public static byte [] FromBitmapToBytes ( Stream stream )
        {
            var bmp = new Bitmap ( stream );
            var type = (BitmapCodeType) getByte1 ( bmp , 0 );

            var length = getByte1 ( bmp , LengthPos ) << 24
                       | getByte1 ( bmp , LengthPos + 1 ) << 16
                       | getByte1 ( bmp , LengthPos + 2 ) << 8
                       | getByte1 ( bmp , LengthPos + 3 );
            var buffer = new byte [ length ];
            for ( int i = 0 ; i < length ; i++ )
            {
                byte b;
                switch ( type )
                {
                case BitmapCodeType . Monochrome:
                    b = getByte1 ( bmp , i + HeaderLength );
                    break;
                case BitmapCodeType . RGB24:
                    b = getByte24 ( bmp , i + HeaderLength * 24 );
                    break;
                case BitmapCodeType . Hue2:
                    b = getByteHue ( bmp , i + HeaderLength * 2 , 2 );
                    break;
                case BitmapCodeType . Hue4:
                    b = getByteHue ( bmp , i + HeaderLength * 4 , 4 );
                    break;
                default:
                    throw new Exception ( "Unknown field in header." );
                }
                buffer [ i ] = b;
            }
            bmp . Dispose ();
            return buffer;
        }

        public static byte [] FromBytesToBitmap (
            byte [] b ,
            int width ,
            BitmapCodeType type ,
            int paddingBottom = 0 )
        {
            var header = new byte [ HeaderLength ];
            header [ 0 ] = (byte) type;
            unchecked
            {
                header [ LengthPos ] = (byte) ( b . Length >> 24 );
                header [ LengthPos + 1 ] = (byte) ( b . Length >> 16 );
                header [ LengthPos + 2 ] = (byte) ( b . Length >> 8 );
                header [ LengthPos + 3 ] = (byte) ( b . Length );
            }

            var totalPixels = HeaderLength * 8;
            switch ( type )
            {
            case BitmapCodeType . Monochrome:
                totalPixels += b . Length * 8;
                break;
            case BitmapCodeType . RGB24:
                totalPixels += b . Length / 3 + ( b . Length % 3 == 0 ? 0 : 1 );
                break;
            case BitmapCodeType . Hue2:
                totalPixels += b . Length * 4;
                break;
            case BitmapCodeType . Hue4:
                totalPixels += b . Length * 2;
                break;
            default:
                throw new Exception ( "Unknown type." );
            }
            var bmp = new Bitmap ( width , ( totalPixels / width + ( totalPixels % width == 0 ? 0 : 1 ) ) + paddingBottom );

            for ( int i = 0 ; i < HeaderLength ; i++ )
            {
                setByte1 ( bmp , i , header [ i ] );
            }

            switch ( type )
            {
            case BitmapCodeType . Monochrome:
                for ( int i = 0 ; i < b . Length ; i++ )
                {
                    setByte1 ( bmp , i + HeaderLength , b [ i ] );
                }
                break;
            case BitmapCodeType . RGB24:
                var length = b . Length / 3 + ( b . Length % 3 == 0 ? 0 : 1 );
                for ( int i = 0 ; i < length ; i++ )
                {
                    if ( i < length - 1 )
                    {
                        setPixel24 ( bmp , i + HeaderLength * 8 ,
                            b [ i * 3 ] ,
                            b [ i * 3 + 1 ] ,
                            b [ i * 3 + 2 ] );
                    }
                    else
                    {
                        setPixel24 ( bmp , i + HeaderLength * 8 ,
                            b [ i * 3 ] ,
                            i * 3 + 1 < b . Length ? b [ i * 3 + 1 ] : byte . MinValue ,
                            i * 3 + 2 < b . Length ? b [ i * 3 + 2 ] : byte . MinValue );
                    }
                }
                break;
            case BitmapCodeType . Hue2:
                for ( int i = 0 ; i < b . Length ; i++ )
                {
                    setByteHue ( bmp , i + HeaderLength * 2 , 2 , b [ i ] );
                }
                break;
            case BitmapCodeType . Hue4:
                for ( int i = 0 ; i < b . Length ; i++ )
                {
                    setByteHue ( bmp , i + HeaderLength * 4 , 4 , b [ i ] );
                }
                break;
            }

            var ms = new MemoryStream ();
            bmp . Save ( ms , ImageFormat . Bmp );
            bmp . Dispose ();
            return ms . ToArray ();
        }

        private static byte getByte1 ( Bitmap bmp , int index )
        {
            var width = bmp . Width;
            var b = 0;
            for ( int i = 0 ; i < 8 ; i++ )
            {
                var color = bmp . GetPixel ( ( index * 8 + i ) % width , ( index * 8 + i ) / width );
                b |= ( color . R + color . G + color . B ) > 0x17f ? 1 << ( 7 - i ) : 0;
            }
            return (byte) b;
        }
        private static byte getByte24 ( Bitmap bmp , int index )
        {
            var pos = index / 3;
            var rgb = index % 3;
            var width = bmp . Width;
            var color = bmp . GetPixel ( pos % width , pos / width );
            switch ( rgb )
            {
            case 0:
                return color . R;
            case 1:
                return color . G;
            case 2:
                return color . B;
            default:
                throw new Exception ();
            }
        }
        private static byte getByteHue ( Bitmap bmp , int index , int bitsPerPixel )
        {
            if ( bitsPerPixel > 0 || bitsPerPixel <= 8 )
            {
                if ( ( bitsPerPixel & bitsPerPixel - 1 ) == 0 )
                {
                    var width = bmp . Width;
                    var length = 8 / bitsPerPixel;
                    var b = 0;
                    for ( int i = 0 ; i < length ; i++ )
                    {
                        var color = ColorUtils . ToAhsl (
                            bmp . GetPixel ( ( index * length + i ) % width , ( index * length + i ) / width ) );
                        var pieces = bitsPerPixel * bitsPerPixel;
                        var section = Ceiling ( color . H / ( 1.0 / ( pieces * 2 ) ) ) % ( pieces * 2 );
                        b |= ( (int) section / 2 ) << ( bitsPerPixel * ( length - 1 - i ) );
                    }
                    return (byte) b;
                }
                else
                {
                    throw new ArgumentException ();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException ();
            }
        }

        private static void setByte1 ( Bitmap bmp , int index , byte b )
        {
            for ( int i = 0 ; i < 8 ; i++ )
            {
                var pos = index * 8 + i;
                bmp . SetPixel ( pos % bmp . Width , pos / bmp . Width ,
                    ( b & 1 << ( 7 - i ) ) == 1 << ( 7 - i ) ? Color . White : Color . Black );
            }
        }
        private static void setPixel24 ( Bitmap bmp , int pos , byte b1 , byte b2 , byte b3 )
        {
            bmp . SetPixel ( pos % bmp . Width , pos / bmp . Width , Color . FromArgb ( b1 , b2 , b3 ) );
        }
        private static void setByteHue ( Bitmap bmp , int pos , int bitsPerPixel , byte b )
        {
            var length = 8 / bitsPerPixel;
            var actualPos = pos * length;
            for ( int i = 0 ; i < length ; i++ )
            {
                setPixelHue (
                    bmp , actualPos + i ,
                    ( b & ( (int) Pow ( 2 , bitsPerPixel ) - 1 << ( ( length - 1 - i ) * bitsPerPixel ) ) )
                        >> ( ( length - 1 - i ) * bitsPerPixel ) ,
                    bitsPerPixel
                );
            }
        }
        private static void setPixelHue ( Bitmap bmp , int pos , double hueIndex , int bitsPerPixel )
        {
            if ( bitsPerPixel > 0 || bitsPerPixel <= 8 )
            {
                if ( ( bitsPerPixel & bitsPerPixel - 1 ) == 0 )
                {
                    bmp . SetPixel ( pos % bmp . Width , pos / bmp . Width ,
                        ColorUtils . FromAhsl ( 1 , ( hueIndex / ( bitsPerPixel * bitsPerPixel ) ) , 1 , 0.5 ) );
                }
                else
                {
                    throw new ArgumentException ();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException ();
            }
        }
    }
    public enum BitmapCodeType : byte
    {
        /// <summary>
        /// Completely safe for lossy formats, but requires the largest image size.
        /// </summary>
        Monochrome = 0,
        /// <summary>
        /// If the image will be compressed to any lossy formats, DO NOT USE THIS TYPE!
        /// </summary>
        RGB24 = 1,
        Hue2 = 10,
        Hue4 = 11,
    }
}
