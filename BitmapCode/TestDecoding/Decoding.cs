using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using Dwscdv3;

namespace Test
{
    class Program
    {
        static void Main ( string [] args )
        {
            var mode = 1;

            switch ( mode )
            {
            case 1:
                var buf1 = BitmapCode . FromBitmapToBytes ( new FileStream ( @"E:\Lab\1.bmp" , FileMode . Open ) );
                Console . WriteLine ( Encoding . UTF8 . GetString ( buf1 ) );
                Console . ReadKey ( true );
                break;
            case 24:
                var buf24 = BitmapCode . FromBitmapToBytes ( new FileStream ( @"E:\Lab\24.bmp" , FileMode . Open ) );
                Console . WriteLine ( Encoding . UTF8 . GetString ( buf24 ) );
                Console . ReadKey ( true );
                break;
            }
        }
    }
}
