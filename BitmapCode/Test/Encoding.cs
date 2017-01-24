using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using Dwscdv3;

namespace Test
{
    class Encoding
    {
        static void Main ( string [] args )
        {
            var mode = 1;
            var text = "Terraria: May the blocks be with you\r\n泰拉瑞亚：愿方块常伴你左右";

            switch (mode)
            {
            case 1:
                var buf1 = System . Text . Encoding . UTF8 . GetBytes ( text );
                var bmp1 = BitmapCode . FromBytesToBitmap ( buf1 , 64 , BitmapCodeType . Monochrome );
                File . WriteAllBytes ( @"E:\Lab\1.bmp" , bmp1 );
                break;
            case 24:
                var buf24 = System . Text . Encoding . UTF8 . GetBytes ( text );
                var bmp24 = BitmapCode . FromBytesToBitmap ( buf24 , 64 , BitmapCodeType . RGB24 );
                File . WriteAllBytes ( @"E:\Lab\24.bmp" , bmp24 );
                break;
            }
        }
    }
}
