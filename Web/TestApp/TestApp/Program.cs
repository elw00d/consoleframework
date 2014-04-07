using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //When choosing types for variables that are part of the DOM API,
            //You will want to use var when it's possible and dynamic when it's not.

            //Builtins.Eval( "alert(1);" );
            buffer[ 4, 4 ] = 'H';
            refresh(  );

            var document = Builtins.Global["document"];

            var newDiv = document.createElement("div");
            newDiv.innerHTML = "click me and check the console";
            newDiv.style.backgroundColor = "yellow";
            turnDivIntoButton(newDiv);

            var body = (document.getElementsByTagName("body"))[0];
            body.appendChild(newDiv);

            body.addEventListener( "keydown", new Action<dynamic>( e => {
                //Builtins.Global[ "alert" ]( 123 );
                e.preventDefault( );
            } ) );

            body.addEventListener( "mousedown", new Action< dynamic >( e => {
                dynamic div = document.getElementById( "div1" );
                dynamic pos = Builtins.Global[ "getRelativePos" ]( e, div );
//                Builtins.Global[ "console" ].log(  );

                int width = div.offsetWidth;
                int height = div.offsetHeight;
                if ( ((int) pos.x) < width && (( int)pos.y) < height ) {
                    int coordX = pos.x/( width*1.0/80 );
                    int coordY = pos.y/( height*1.0/25 );
                    Builtins.Global["console"].log(pos.x + " " + pos.y + " -> " + coordX + " " + coordY);
                    buffer[ coordX, coordY ] = 'X';
                    refresh(  );
                }
                //e.preventDefault( );
            } ) );
        }

        private static char[,] buffer = new char[80, 25];

        public static void initBuffer( ) {
            for ( int y = 0; y < 25; y++ ) {
                for ( int x = 0; x < 80; x++ ) {
                    buffer[ x, y ] = ' ';
                }
            }
        }

        public static void refresh( ) {
            var document = Builtins.Global["document"];
            dynamic div = document.getElementById( "div1" );
            StringBuilder sb = new StringBuilder();
            for ( int y = 0; y < 25; y++ ) {
                for ( int x = 0; x < 80; x++ ) {
                    if ( buffer[ x, y ] == '\0' || buffer[ x, y ] == ' ' )
                        sb.Append( "&nbsp;" );
                    else
                        sb.Append( buffer[ x, y ] );
                }
                sb.Append( "<br/>" );
            }
            div.innerHTML = sb.ToString( );
        }

        static void turnDivIntoButton(dynamic div)
        {
            //You will want your event listeners typed as Actions.
            //Using Actions makes the syntax for closures very friendly
            div.addEventListener(
              "click",
              attachListenerToDiv(div),
              false
            );
        }

        //This is how you can make a closure. An action is returned
        static Action attachListenerToDiv(dynamic div)
        {
            return
                (Action)(() =>
                {
                    var console = Builtins.Global["console"];
                    console.log("Div clicked");
                    div.innerHTML = "clicked. you may click again and check the console";
                });
        }
    }
}
