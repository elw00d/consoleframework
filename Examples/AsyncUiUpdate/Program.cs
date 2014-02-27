using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.AsyncUiUpdate
{
    class Program
    {
        public static void Main( string[ ] args ) {
            

            WindowsHost windowsHost = new WindowsHost( );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.AsyncUiUpdate.main.xml", null );
            windowsHost.Show( mainWindow );
            TextBlock textBlock = mainWindow.FindChildByName< TextBlock >( "text" );
            //textBlock.Text = "1";
            Thread thread = new Thread( ( ) => {
                int i = 1;
                for ( ;; ) {
                    ConsoleApplication.Instance.Post( new Action(( ) => {
                        textBlock.Text = i.ToString();
                    }) );
                    Thread.Sleep( 1000 );
					i++;
                }
            } );
            thread.IsBackground = true;
            thread.Start();

            mainWindow.FindChildByName< Button >( "btnMaximize" ).OnClick += ( sender, eventArgs ) => {
                ConsoleApplication.Instance.Maximize();
            };
            mainWindow.FindChildByName< Button >( "btnRestore" ).OnClick += ( sender, eventArgs ) => {
                ConsoleApplication.Instance.Restore();
            };

            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
