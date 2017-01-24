using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Navigation;
using System . Windows . Shapes;
using Dwscdv3;
using Microsoft . Win32;

namespace BitmapCodeGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog openImage = new OpenFileDialog ();
        OpenFileDialog openFile = new OpenFileDialog ();
        SaveFileDialog saveBitmap = new SaveFileDialog ();
        SaveFileDialog saveFile = new SaveFileDialog ();

        public MainWindow ()
        {
            InitializeComponent ();
            openImage . Filter = "Image|*.bmp;*.jpg;*.jpeg;*.png;*.gif|Bitmap|*.bmp|JPEG|*.jpg;*.jpeg|PNG|*.png|GIF|*.gif";
            saveBitmap . Filter = "Bitmap|*.bmp";
        }

        private Encoding getEncoding ()
        {
            switch ( encoding . SelectedIndex )
            {
            case 0:
                return Encoding . UTF8;
            case 1:
                return Encoding . Unicode;
            case 2:
                return Encoding . Default;
            default:
                throw new Exception ( "Invalid encoding selected." );
            }
        }

        private BitmapCodeType getBitmapCodeType ()
        {
            switch ( colorMode . SelectedIndex )
            {
            case 0:
                return BitmapCodeType . Monochrome;
            case 1:
                return BitmapCodeType . RGB24;
            default:
                throw new Exception ( "Invalid color mode selected." );
            }
        }

        private void TextBox_DigitOnly ( object sender , TextCompositionEventArgs e )
        {
            foreach ( char c in e . Text )
            {
                if ( !char . IsDigit ( c ) )
                {
                    e . Handled = true;
                }
            }
        }

        private void encodeText_Click ( object sender , RoutedEventArgs e )
        {
            if ( saveBitmap . ShowDialog ( this ) == true )
            {
                try
                {
                    var bytes = getEncoding () . GetBytes ( text . Text );
                    BitmapCodeType type = 0;
                    switch ( colorMode . SelectedIndex )
                    {
                    case 0:
                        type = BitmapCodeType . Monochrome;
                        break;
                    case 1:
                        type = BitmapCodeType . RGB24;
                        break;
                    }
                    var bmp = BitmapCode . FromBytesToBitmap (
                        bytes ,
                        int . Parse ( width . Text ) ,
                        type ,
                        int . Parse ( paddingBottom . Text )
                    );
                    File . WriteAllBytes ( saveBitmap . FileName , bmp );
                }
                catch
                {
                    text . Text = "Encoding failed. An error occured in the program.";
                }
            }
        }

        private void decodeText_Click ( object sender , RoutedEventArgs e )
        {
            if ( openImage . ShowDialog ( this ) == true )
            {
                try
                {
                    using ( var stream = openImage . OpenFile () )
                    {
                        var bytes = BitmapCode . FromBitmapToBytes ( stream );
                        text . Text = getEncoding () . GetString ( bytes );
                    }
                }
                catch
                {
                    text . Text = "Decoding failed. Perhaps this file is not a BitmapCode image, or an error occured in the program.";
                }
            }
        }

        private void encodeFile_Click ( object sender , RoutedEventArgs e )
        {
            if ( openFile . ShowDialog ( this ) == true )
            {
                if ( saveBitmap . ShowDialog ( this ) == true )
                {
                    try
                    {
                        var bytes = File . ReadAllBytes ( openFile . FileName );
                        var type = getBitmapCodeType ();
                        var bmp = BitmapCode . FromBytesToBitmap (
                            bytes ,
                            int . Parse ( width . Text ) ,
                            type ,
                            int . Parse ( paddingBottom . Text )
                        );
                        File . WriteAllBytes ( saveBitmap . FileName , bmp );
                    }
                    catch
                    {
                        text . Text = "Encoding failed. An error occured in the program.";
                    }
                }
            }
        }

        private void decodeFile_Click ( object sender , RoutedEventArgs e )
        {
            if ( openImage . ShowDialog ( this ) == true )
            {
                if ( saveFile . ShowDialog ( this ) == true )
                {
                    try
                    {
                        using ( var stream = openImage . OpenFile () )
                        {
                            var bytes = BitmapCode . FromBitmapToBytes ( stream );
                            File . WriteAllBytes ( saveFile . FileName , bytes );
                        }
                    }
                    catch
                    {
                        text . Text = "Decoding failed. Perhaps this file is not a BitmapCode image, or an error occured in the program.";
                    }
                }
            }
        }
    }
}
