using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using Dwscdv3;

namespace TestColorConversion
{
    class Program
    {
        static void Main ( string [] args )
        {
            var hsl = ColorUtils . ToAhsl ( 255 , 42 , 9 , 233 );
            Console . WriteLine ( "H = {0}\nS = {1}\nL = {2}\n" , hsl . H , hsl . S , hsl . L );
            Console . ReadKey ( true );
        }
    }
}
