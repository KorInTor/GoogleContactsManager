using CommunityToolkit.Mvvm.ComponentModel;
using Google.Apis.PeopleService.v1.Data;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ViewModel.Contact.ValuePicker
{
    public class GoogleDatePickerVM : ObservableValidator
    {
        [Newtonsoft.Json.JsonIgnore]
        private bool _supressValidation = false;

        [Newtonsoft.Json.JsonIgnore]
        private int? _day;

        [Newtonsoft.Json.JsonIgnore]
        private Tuple<int, string> _selectedMonth;

        [Newtonsoft.Json.JsonIgnore]
        private int? _year;

        public Date GoogleDate
        {
            get
            {
                var date = new Date
                {
                    Year = Year,
                    Day = Day,
                    Month = SelectedMonth.Item1
                };
                return date;
            }
            set
            {
                _day = value.Day;
                if (value.Month is int month)
                {
                    _selectedMonth = MonthsNamed[month - 1];
                }
                else
                {
                    _selectedMonth = MonthsNamed[0];
                }
                _year = value.Year;
                OnPropertyChanged(nameof(GoogleDate));
                OnPropertyChanged(nameof(Day));
                OnPropertyChanged(nameof(Year));
                OnPropertyChanged(nameof(SelectedMonth));
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public Tuple<int, string>[] MonthsNamed { get; } = Enumerable.Range(1, 12).Select(i => Tuple.Create(i, GetMonthName(i))).ToArray();

        [Newtonsoft.Json.JsonIgnore]
        [CustomValidation(typeof(GoogleDatePickerVM), nameof(ValidateDayCombination))]
        public Tuple<int, string> SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    _supressValidation = false;
                    ValidateProperty(value);
                }
            }
        }

        [CustomValidation(typeof(GoogleDatePickerVM), nameof(ValidateDayCombination))]
        [Newtonsoft.Json.JsonIgnore]
        public int? Day
        {
            get => _day;
            set
            {
                if (SetProperty(ref _day, value))
                {
                    _supressValidation = false;
                    ValidateProperty(value);
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        [CustomValidation(typeof(GoogleDatePickerVM), nameof(ValidateDayCombination))]
        public int? Year
        {
            get => _year;
            set
            {
                if (SetProperty(ref _year, value))
                {
                    _supressValidation = false;
                    ValidateProperty(value);
                }
            }
        }

        public GoogleDatePickerVM()
        {
            _selectedMonth = MonthsNamed[0];
            OnPropertyChanged(nameof(SelectedMonth));
        }

        public GoogleDatePickerVM(Date? date) : this()
        {
            if (date is null)
            {
                _supressValidation = true;
            }

            GoogleDate = date is null ? new() : date;
            ValidateAllProperties();
        }

        public static int GetDaysInMonth(int month, int year = 4)
        {
            return DateTime.DaysInMonth(year, month);
        }

        public static string GetMonthName(int month, string cultureCode = "ru-RU")
        {
            var culture = new System.Globalization.CultureInfo(cultureCode);
            return new DateTime(1, month, 1).ToString("MMMM", culture);
        }

        public static ValidationResult? ValidateDayCombination(object? validationObject, ValidationContext context)
        {
            bool isValid = ((GoogleDatePickerVM)context.ObjectInstance)._supressValidation || ((GoogleDatePickerVM)context.ObjectInstance).IsDateValid();
            if (isValid)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("День некорректный");
        }

        private bool IsDateValid()
        {
            if (Day is null)
                return false;

            int yearToCheck = Year == null ? 4 : (int)Year;

            return Day > 0 && Day <= DateTime.DaysInMonth(yearToCheck, SelectedMonth.Item1);
        }
    }
}
