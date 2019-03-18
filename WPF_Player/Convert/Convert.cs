using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WPF_Player
{
    #region for miniButton
    public class X1Convert :IValueConverter
    {
       int intPara = 5;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double doubleValue = (double)value;
            return doubleValue / 2 - intPara;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
     
   public class X2Convert : IValueConverter
   {
       int intPara = 5;
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           double doubleValue = (double)value;
           return doubleValue / 2 + intPara;
       }

       public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           throw new NotImplementedException();
       }
   }
   
   public class Y1Y2Convert : IValueConverter
   {
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           double doubleValue = (double)value;
           return doubleValue / 2;
       }

       public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           throw new NotImplementedException();
       }
   }
    #endregion
    #region for closeButton
   public class Get4_1Value : IValueConverter
   {
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           return ((double)value) / 4;
       }

       public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           throw new NotImplementedException();
       }
   }
   public class Get4_3Value : IValueConverter
   {
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           return ((double)value) / 4*3;
       }

       public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           throw new NotImplementedException();
       }
   }
    #endregion
    #region for title lable
    public class GetTitleLabelWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value - 4 * 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
    public class GetValueByPara : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double par = double.Parse(parameter.ToString());
            return ((double)value) * par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
