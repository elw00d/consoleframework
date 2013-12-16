using System;

namespace ConsoleFramework.Xaml
{
    /// <summary>
    /// Примитивные типы, такие как строки, целые числа, - могут быть заданы в XAML
    /// отдельным тегом. Но так как у примитивов нет свойств и Content-свойства тем более,
    /// то для удобства написания парсера задание таких примитивов производится через обёртки.
    /// Класс-обёртка имеет свойство соответствующего примитивного типа, пользователь задаёт его
    /// в XAML, а при обработке парсер видит, что это именно IPrimitive-объект, и вместо самого
    /// объекта подставляет значение его свойства Content. В результате в родительский объект
    /// приходит значение примитива, которое можно задавать различными способами (в том числе
    /// и с использованием расширений разметки).
    /// </summary>
    interface IPrimitive
    {
        Object ContentNonGeneric { get; } 
    }

    /// <summary>
    /// Available in XAML markup as "string", "int", "double",
    /// "float", "char", "bool" elements.
    /// </summary>
    class Primitive<T> : IPrimitive
    {
        public T Content { get; set; }

        public object ContentNonGeneric { get { return Content; } }
    }
}
