using System;
using System.Collections.Generic;
using System.Text;

namespace TrumpX.Toolkit.Wpf.Mvvm
{
    public class Miscellany
    {
        public static T ConvertType<T>(object parameter)
        {
            dynamic dynamic = parameter;
            dynamic = dynamic is T ? dynamic : Convert.ChangeType(dynamic, typeof(T));
            return (T)dynamic;
        }
    }
}