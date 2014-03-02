using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Modules.ValIdation
{
    public static class RegularExpressions
    {
        public const string Text = @"^([^\t])*$";
        public const string Numbers = @"^([0-9])*$";
        public const string Decimal = @"^\d+(\.\d{1,2})?$";

        public const string AlphaNumeric =
          @"^[a-zA-Z0-9àáâäãåèéêëìíîïòóôöõøùúûüÿýñçčšžÀÁÂÄÃÅÈÉÊËÌÍÎÏÒÓÔÖÕØÙÚÛÜŸÝÑßÇŒÆČŠŽ∂ð\- ]*$";

        public const string AlphaNumericDot =
          @"^[a-zA-Z0-9àáâäãåèéêëìíîïòóôöõøùúûüÿýñçčšžÀÁÂÄÃÅÈÉÊËÌÍÎÏÒÓÔÖÕØÙÚÛÜŸÝÑßÇŒÆČŠŽ∂ð\.,\/:;'\(\)\[\]\{\}#*&|`~_\- ]*$";

        public const string Alpha = @"^[a-zA-ZàáâäãåèéêëìíîïòóôöõøùúûüÿýñçčšžÀÁÂÄÃÅÈÉÊËÌÍÎÏÒÓÔÖÕØÙÚÛÜŸÝÑßÇŒÆČŠŽ∂ð\- ]*$";

        public const string AmericanPhone = @"^[0-9]{3}\-[0-9]{3}\-[0-9]{4}( EXT[0-9]{6}){0,1}$";
        public const string Phone = @"^([0-9\-])*$";
        public const string UserName = @"^[a-zA-Z0-9\-_@.]*$";

        public const string UserNameNonSpace = @"^\S[a-zA-Z0-9\-_@.]*$";

        public const string Password = @"^([^\t])*$";
        public const string ContactUsName = @"^[-\\.\\,\\_a-zA-Z0-9\s]*$";

        public const string Email =
          @"^([-a-zA-Z0-9_.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        public const string URL = @"http://([\w-]+\.)+[\w-]+((/|(/?))[\w- ./?%&=]*)?";
        public const string PostalCode = @"^[a-zA-Z0-9]*$";
        public const string BusinessNumber = @"^([0-9]{9} [0-9a-zA-Z]*){0,1}$";

        public static string NonNegative = "[+]?[0-9]*\\.?[0-9]*";

        public static string Date = "(0[1-9]|[12][0-9]|3[01])[(-)|(\\.)|(/)]" +
                                    "(0[1-9]|1[012])[(-)|(\\.)|(/)]((175[7-9])|(17[6-9][0-9])|(1[8-9][0-9][0-9])|" +
                                    "([2-9][0-9][0-9][0-9]))";

        public static string DateFormat = "(0[1-9]|[12][0-9]|3[01]|[1-9])[/]" +
                                          "(0[1-9]|1[012])[/]((175[7-9])|(17[6-9][0-9])|(1[8-9][0-9][0-9])|" +
                                          "([2-9][0-9][0-9][0-9]))";
    }
}
