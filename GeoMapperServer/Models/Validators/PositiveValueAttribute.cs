using System;
using System.ComponentModel.DataAnnotations;

namespace GeoMapper.Models.Validators
{
    public class PositiveValueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                if (value == null)
                    return false;
                else
                    return (int)value > 0;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
    }
}
