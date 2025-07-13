using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PetProfiles.Maui.Converters;

public class EditModeTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEdit && isEdit)
            return "Edit Pet Profile";
        return "Add Pet Profile";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
} 