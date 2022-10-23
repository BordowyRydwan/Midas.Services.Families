using Midas.Services;

namespace Application.Dto;

public class UserFamilyRoleDto
{
    public UserDto User { get; set; }
    public FamilyDto Family { get; set; }
    public FamilyRoleDto FamilyRole { get; set; }
}