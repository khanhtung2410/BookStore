using System.Collections.Generic;
using Bookstore.Roles.Dto;

namespace Bookstore.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}