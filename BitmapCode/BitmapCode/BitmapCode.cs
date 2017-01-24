using System;
using System . Collections . Generic;
using System . Drawing;
using System . Drawing . Imaging;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace Dwscdv3
{
    /** Format:
     *  Type        0x0
     *  Reserved    0x1 ~ 0x3
     *  Length      0x4 ~ 0x7
     *  Data        0x8 ~
     */
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
                default:
                    throw new Exception ( "Unknown field in header." );
                }
                buffer [ i ] = b;
            }
            bmp . Dispose ();
            return buffer;
        }

        public static byte [] FromBytesToBitmap ( byte [] b , int width , BitmapCodeType type , int paddingBottom = 0 )
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
            default:
                throw new Exception ( "Unknown type." );
            }
            var bmp = new Bitmap ( width , ( totalPixels / width + ( totalPixels % width == 0 ? 0 : 1 ) ) + paddingBottom );

            for ( int i = 0 ; i < HeaderLength ; i++ )
            {
                set8Pixels1 ( bmp , i , header [ i ] );
            }

            switch ( type )
            {
            case BitmapCodeType . Monochrome:
                for ( int i = 0 ; i < b . Length ; i++ )
                {
                    set8Pixels1 ( bmp , i + HeaderLength , b [ i ] );
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

        private static void set8Pixels1 ( Bitmap bmp , int index , byte b )
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
        RGB24 = 1
    }
}
