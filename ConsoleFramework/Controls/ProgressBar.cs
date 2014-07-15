using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    public class ProgressBar : Control
    {
        private int percent;

        /// <summary>
        /// Percent (from 0 to 100).
        /// </summary>
        public int Percent {
            get { return percent; }
            set {
                if ( percent != value ) {
                    percent = value;
                    RaisePropertyChanged( "Percent" );
                }
            }
        }

        public override void Render( RenderingBuffer buffer ) {
            Attr attr = Colors.Blend( Color.DarkCyan, Color.DarkBlue );
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, UnicodeTable.MediumShade, attr);
            int filled = ( int ) ( ActualWidth*( Percent*0.01 ) );
            buffer.FillRectangle(0, 0, filled, ActualHeight, UnicodeTable.DarkShade, attr);
        }
    }
}
