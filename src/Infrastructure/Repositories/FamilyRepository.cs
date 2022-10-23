using Domain.Consts;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    private readonly FamilyDbContext _dbContext;

    public FamilyRepository(FamilyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ulong> AddNewFamily(Family family)
    {
        if (string.IsNullOrWhiteSpace(family.Name))
        {
            throw new FamilyException("Family name is empty");
        }

        var doesFamilyExist = await _dbContext.Families.AnyAsync(x => x.Name == family.Name).ConfigureAwait(false);
        
        
        if (doesFamilyExist)
        {
            throw new FamilyException($"Family with name {family.Name} already exists!");
        }

        await _dbContext.AddAsync(family).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        
        var userFamilyRole = new UserFamilyRole
        {
            Family = family,
            FamilyRoleId = (ulong)FamilyRoles.MainAdministrator,
            UserId = family.FounderId
        };

        await _dbContext.AddAsync(userFamilyRole).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return family.Id;
    }

    public async Task<bool> DeleteFamily(ulong id)
    {
        var family = await _dbContext.Families.FindAsync(id).ConfigureAwait(false);

        if (family is null)
        {
            return false;
        }

        _dbContext.Families.Remove(family);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<Family> GetFamilyById(ulong id)
    {
        return await _dbContext.Families.FindAsync(id).ConfigureAwait(false);
    }

    public async Task<bool> AddUserToFamily(ulong userId, ulong familyId)
    {
        var keyObject = new { userId, familyId };
        var userFamilyRoleCheck = await _dbContext.UserFamilyRoles.FindAsync(keyObject).ConfigureAwait(false);

        if (userFamilyRoleCheck is not null)
        {
            return false;
        }

        var newUserFamilyRole = new UserFamilyRole
        {
            FamilyId = familyId,
            UserId = userId,
            FamilyRoleId = (ulong)FamilyRoles.Child
        };
        
        await _dbContext.UserFamilyRoles.AddAsync(newUserFamilyRole);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteUserFromFamily(ulong userId, ulong familyId)
    {
        var keyObject = new { userId, familyId };
        var userFamilyRole = await _dbContext.UserFamilyRoles.FindAsync(keyObject).ConfigureAwait(false);

        if (userFamilyRole is null)
        {
            return false;
        }

        _dbContext.UserFamilyRoles.Remove(userFamilyRole);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> SetUserFamilyRole(UserFamilyRole userFamilyRole)
    {
        var keyObject = new { userFamilyRole.UserId, userFamilyRole.FamilyId };
        var dbEntity = await _dbContext.UserFamilyRoles.FindAsync(keyObject).ConfigureAwait(false);

        if (dbEntity is null)
        {
            return false;
        }

        dbEntity.FamilyRoleId = userFamilyRole.FamilyRoleId;
        
        _dbContext.UserFamilyRoles.Update(dbEntity);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<ICollection<UserFamilyRole>> GetFamilyMemberRolesForUser(ulong userId)
    {
        return await _dbContext.UserFamilyRoles.Where(x => x.UserId == userId)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}