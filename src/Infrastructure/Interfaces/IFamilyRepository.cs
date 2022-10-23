using Domain.Entities;

namespace Infrastructure.Interfaces;

public interface IFamilyRepository
{
    public Task<ulong> AddNewFamily(Family family);
    public Task<bool> DeleteFamily(ulong id);
    public Task<Family> GetFamilyById(ulong id);
    public Task<bool> AddUserToFamily(ulong userId, ulong familyId);
    public Task<bool> DeleteUserFromFamily(ulong userId, ulong familyId);
    public Task<bool> SetUserFamilyRole(UserFamilyRole userFamilyRole);
    public Task<ICollection<UserFamilyRole>> GetFamilyMemberRolesForUser(ulong userId);
}