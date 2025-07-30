using Google.Apis.PeopleService.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Contact.ValuePicker;

namespace ViewModel.Extensions
{
    public static class GoogleDatePickerVMExtensions
    {
        public static Date ToDate(this GoogleDatePickerVM datePickerVM)
        {
            return datePickerVM.GoogleDate;
        }

        public static Birthday ToBirthday(this GoogleDatePickerVM datePickerVM)
        {
            return new Birthday() { Date = datePickerVM.GoogleDate };
        }
    }
}
