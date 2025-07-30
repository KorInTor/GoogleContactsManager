using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class SelectableObject<T>
    {
        public bool IsSelected { get; set; }
        public T Value { get; set; }

        public SelectableObject() { }

        public SelectableObject(T value, bool isSelected = false) : this()
        {
            Value = value;
            IsSelected = isSelected;
        }
    }

}
