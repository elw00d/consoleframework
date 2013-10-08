using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Controls;

namespace ConsoleFramework.Core
{
    public class VisualTreeHelper
    {
        /// <summary>
        /// Рекурсивно ищёт дочерний элемент по указанному Name.
        /// Если в результате поиска подходящий элемент не был найден, возвращается null.
        /// </summary>
        public static Control FindChildByNameRecoursively( Control control, string childName ) {
            if ( null == control )
                throw new ArgumentNullException( "control" );
            if ( string.IsNullOrEmpty( childName ) )
                throw new ArgumentException( "String is null or empty", "childName" );
            //
            return findChildByNameRecoursively( control, childName );
        }

        private static Control findChildByNameRecoursively( Control control, string childName ) {
            List< Control > children = control.Children;
            foreach ( Control child in children ) {
                if ( child.Name == childName ) {
                    return child;
                } else {
                    Control result = findChildByNameRecoursively( child, childName );
                    if ( null != result )
                        return result;
                }
            }
            return null;
        }

        public static bool IsConnectedToRoot( Control control ) {
            if ( null == control ) {
                throw new ArgumentNullException("control");
            }
            Control root = ConsoleApplication.Instance.RootControl;
            Control current = control;
            while (current != null)
            {
                if (current == root)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        /// <summary>
        /// Ищет ближайший родительский элемент контрола типа T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <returns></returns>
        public static T FindClosestParent< T >( Control control ) where T : Control {
            Control tmp = control;
            while ( tmp != null && !( tmp is T ) )
                tmp = tmp.Parent;
            return (T) tmp;
        }
    }
}