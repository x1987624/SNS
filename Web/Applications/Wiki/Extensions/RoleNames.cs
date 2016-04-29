using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    public static class RoleNamesExtention
    {
        public static string WikiAdministrator(this RoleNames roleNames)
        {
            return "WikiAdministrator";
        }
    }
}