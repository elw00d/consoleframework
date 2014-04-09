using System;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using JSIL;

namespace TestApp
{
    public delegate void UserInputEventHandler( object sender, UserInputEventArgs args );

    public class UserInputEventArgs : EventArgs
    {
        private readonly INPUT_RECORD inputRecord;

        public INPUT_RECORD InputRecord {
            get { return inputRecord; }
        }

        public UserInputEventArgs( INPUT_RECORD inputRecord ) {
            this.inputRecord = inputRecord;
        }
    }

    public class ConsoleAdapter : PhysicalCanvas 
    {
        public event UserInputEventHandler UserInputReceived = delegate { };

        private readonly int width;
        private readonly int height;

        public ConsoleAdapter( int width, int height ): base(width, height) {
            this.width = width;
            this.height = height;
        }

        private dynamic body;
        private dynamic div;

        public void Initialize( ) {
            var document = Builtins.Global["document"];
            body = (document.getElementsByTagName("body"))[0];
            div = document.getElementById("div1");

            body.addEventListener( "keydown", new Action< dynamic >( e => {
                //Builtins.Global[ "alert" ]( 123 );
                e.preventDefault( );
            } ) );

            body.addEventListener("mousedown", new Action<dynamic>(e => {
                dynamic pos = Builtins.Global["getRelativePos"](e, div);
                //                Builtins.Global[ "console" ].log(  );

                int divWidth = div.offsetWidth;
                int divHeight = div.offsetHeight;
                if (((int)pos.x) < divWidth && ((int)pos.y) < divHeight) {
                    int coordX = pos.x/( divWidth*1.0/width );
                    int coordY = pos.y/( divHeight*1.0/height );
                    Builtins.Global["console"].log(pos.x + " " + pos.y + " -> " + coordX + " " + coordY);
                    //buffer[coordX, coordY] = 'X';
                    //flushScreen();
                    UserInputReceived.Invoke( this, new UserInputEventArgs( 
                        new INPUT_RECORD(  )
                            {
                                EventType = EventType.MOUSE_EVENT,
                                MouseEvent = new MOUSE_EVENT_RECORD(  )
                                    {
                                        dwButtonState = MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED,
                                        dwControlKeyState = 0,
                                        dwEventFlags = MouseEventFlags.PRESSED_OR_RELEASED,
                                        dwMousePosition = new COORD((short)coordX, (short)coordY)
                                    }
                            }) );
                }
                //e.preventDefault( );
            }));

            initBuffer(  );
            //flushScreen(  );
        }

        //private char[,] buffer = new char[80, 25];

        private void initBuffer() {
            for ( int y = 0; y < height; y++ ) {
                for ( int x = 0; x < width; x++ ) {
                    buffer[ x, y ] =  new CHAR_INFO(  ) {UnicodeChar = ' '};
                }
            }
        }

        private void flushScreen() {
            StringBuilder sb = new StringBuilder( );
            for ( int y = 0; y < height; y++ ) {
                for ( int x = 0; x < width; x++ ) {
                    if ( buffer[ x, y ].UnicodeChar == '\0' || buffer[ x, y ].UnicodeChar == ' ' )
                        sb.Append( "&nbsp;" );
                    else
                        sb.Append( buffer[ x, y ] );
                }
                sb.Append( "<br/>" );
            }
            div.innerHTML = sb.ToString( );
        }

        public override void Flush( Rect affectedRect ) {
            flushScreen( );
        }
    }
}
