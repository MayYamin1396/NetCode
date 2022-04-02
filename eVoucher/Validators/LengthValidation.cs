using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class LengthValidation : Attribute
    {
        public int LengthMin { get; private set; }

        public int LengthMax { get; }

        public LengthValidation()
        {
            this.LengthMin = 0;
            this.LengthMax = int.MaxValue;
        }

        public LengthValidation(int maxLength)
        {
            this.LengthMin = 0;
            this.LengthMax = maxLength;
        }

        public LengthValidation(int minLength, int maxLength)
        {
            this.LengthMin = minLength;
            this.LengthMax = maxLength;
        }
        public bool CheckValid(object value)
        {
            int length;
            if (value == null)
            {
                return false;
            }
            else
            {
                var str = value as string;
                if (str != null)
                {
                    length = str.Length;
                }
                else
                {
                    // We expect a cast exception if a non-{string|array} property was passed in.
                    length = ((Array)value).Length;
                }
            }

            return length >= this.LengthMin && length <= this.LengthMax;
        }
    }
}
