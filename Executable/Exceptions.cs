using System;
using System.Collections.Generic;
using System.Text;

namespace PixelWhimsy
{
    public class TooManyExceptions : Exception
    {
        public TooManyExceptions(string message) : base(message) { }
        public TooManyExceptions(string message, Exception innerException) : base(message, innerException) { }
    }
}
