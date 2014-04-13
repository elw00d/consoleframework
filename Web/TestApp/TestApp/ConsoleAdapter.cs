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

            bool mousePressed = false;
            body.addEventListener("mousedown", new Action<dynamic>(e => {
                dynamic pos = Builtins.Global["getRelativePos"](e, div);
                int divWidth = div.offsetWidth;
                int divHeight = div.offsetHeight;
                if (((int)pos.x) < divWidth && ((int)pos.y) < divHeight) {
                    int coordX = pos.x/( divWidth*1.0/width );
                    int coordY = pos.y/( divHeight*1.0/height );
                    Builtins.Global["console"].log("mousedown " +
                        pos.x + " " + pos.y + " -> " + coordX + " " + coordY);
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
                mousePressed = true;
                //e.preventDefault( );
            }));

            body.addEventListener("mouseup", new Action<dynamic>(e =>
            {
                dynamic pos = Builtins.Global["getRelativePos"](e, div);
                int divWidth = div.offsetWidth;
                int divHeight = div.offsetHeight;
                if (((int)pos.x) < divWidth && ((int)pos.y) < divHeight)
                {
                    int coordX = pos.x / (divWidth * 1.0 / width);
                    int coordY = pos.y / (divHeight * 1.0 / height);
                    Builtins.Global["console"].log("mouseup: " + pos.x + " " + pos.y + " -> " + coordX + " " + coordY);
                    UserInputReceived.Invoke(this, new UserInputEventArgs(
                        new INPUT_RECORD()
                        {
                            EventType = EventType.MOUSE_EVENT,
                            MouseEvent = new MOUSE_EVENT_RECORD()
                            {
                                dwButtonState = 0,
                                dwControlKeyState = 0,
                                dwEventFlags = MouseEventFlags.PRESSED_OR_RELEASED,
                                dwMousePosition = new COORD((short)coordX, (short)coordY)
                            }
                        }));
                }
                mousePressed = false;
                //e.preventDefault( );
            }));

            body.addEventListener("mousemove", new Action<dynamic>(e =>
            {
                dynamic pos = Builtins.Global["getRelativePos"](e, div);
                int divWidth = div.offsetWidth;
                int divHeight = div.offsetHeight;
                if (((int)pos.x) < divWidth && ((int)pos.y) < divHeight)
                {
                    int coordX = pos.x / (divWidth * 1.0 / width);
                    int coordY = pos.y / (divHeight * 1.0 / height);
//                    Builtins.Global["console"].log("mousemove " + pos.x + " " + pos.y + " -> " + coordX + " " + coordY);
                    UserInputReceived.Invoke(this, new UserInputEventArgs(
                        new INPUT_RECORD()
                        {
                            EventType = EventType.MOUSE_EVENT,
                            MouseEvent = new MOUSE_EVENT_RECORD()
                            {
                                dwButtonState = mousePressed ? MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED : 0,
                                dwControlKeyState = 0,
                                dwEventFlags = MouseEventFlags.MOUSE_MOVED,
                                dwMousePosition = new COORD((short)coordX, (short)coordY)
                            }
                        }));
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
                    buffer[ y, x ] =  new CHAR_INFO(  ) {UnicodeChar = ' '};
                }
            }
        }

        private void flushScreen() {
            StringBuilder sb = new StringBuilder( );
            for ( int y = 0; y < height; y++ ) {
                for ( int x = 0; x < width; x++ ) {
                    if ( buffer[ y, x ].UnicodeChar == '\0' || buffer[ y, x ].UnicodeChar == ' ' )
                        sb.Append( "&nbsp;" );
                    else
                        sb.Append( buffer[ y, x ].UnicodeChar );
                }
                sb.Append( "<br/>" );
            }
            div.innerHTML = sb.ToString( );
        }

        public override void Flush( Rect affectedRect ) {
            flushScreen( );
            //Console.WriteLine("Flush called");
        }
    }
}
